using System;
using System.Security.Cryptography;
using System.Text;
using Glytos;
using Xunit;

namespace Glytos.Tests
{
    public class WebhookTests
    {
        // A fixed vector produced by the server signer (HMAC-SHA256 over "{ts}.{body}"),
        // so this SDK is proven byte-for-byte compatible with how Glytos signs deliveries.
        private const string Secret = "whsec_test_secret";
        private const string Ts = "1710000000";
        private const string Body = "{\"event\":\"call.completed\",\"id\":\"evt_123\"}";
        private const string Sig = "356d865446082d49c6bfa57299c45900ab0d6681363426341acbe7c8f07ba025";

        private static string Header(string ts, string sig) => $"t={ts},v1={sig}";

        [Fact]
        public void AcceptsAKnownSignature()
        {
            // Tolerance 0 so the fixed historical timestamp is not rejected as too old.
            Assert.True(Webhook.Verify(Body, Header(Ts, Sig), Secret, 0));
        }

        [Fact]
        public void RejectsATamperedBody()
        {
            Assert.False(Webhook.Verify(Body + "x", Header(Ts, Sig), Secret, 0));
        }

        [Fact]
        public void RejectsAWrongSecret()
        {
            Assert.False(Webhook.Verify(Body, Header(Ts, Sig), "wrong-secret", 0));
        }

        [Fact]
        public void RejectsAMalformedHeader()
        {
            Assert.False(Webhook.Verify(Body, "garbage", Secret, 0));
            Assert.False(Webhook.Verify(Body, $"t={Ts}", Secret, 0));
        }

        [Fact]
        public void RejectsAnExpiredDelivery()
        {
            // With the default tolerance the 2024 timestamp is far outside the window.
            Assert.False(Webhook.Verify(Body, Header(Ts, Sig), Secret));
        }

        [Fact]
        public void AcceptsAFreshDelivery()
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
            const string body = "{\"hello\":\"world\"}";
            const string secret = "whsec_live";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{ts}.{body}"));
            var sig = Convert.ToHexString(hash).ToLowerInvariant();
            Assert.True(Webhook.Verify(body, Header(ts, sig), secret));
        }
    }
}
