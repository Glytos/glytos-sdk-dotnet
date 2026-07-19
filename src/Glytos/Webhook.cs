using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Glytos
{
    /// <summary>
    /// Webhook signature verification.
    /// </summary>
    /// <remarks>
    /// Matches the server scheme: HMAC-SHA256 over <c>"{timestamp}.{body}"</c>, delivered in
    /// the <c>X-Glytos-Signature: t=&lt;timestamp&gt;,v1=&lt;hex&gt;</c> header.
    /// </remarks>
    public static class Webhook
    {
        /// <summary>
        /// Returns <c>true</c> only if <paramref name="signatureHeader"/> is a valid signature
        /// for the given raw body.
        /// </summary>
        /// <param name="payload">The RAW request body exactly as received (do not re-encode).</param>
        /// <param name="signatureHeader">The <c>X-Glytos-Signature</c> header value.</param>
        /// <param name="secret">Your endpoint signing secret.</param>
        /// <param name="toleranceSeconds">
        /// Reject deliveries older than this many seconds to blunt replay attacks. Pass <c>0</c>
        /// to disable the timestamp check.
        /// </param>
        public static bool Verify(string payload, string signatureHeader, string secret, int toleranceSeconds = 300)
        {
            if (payload is null || signatureHeader is null || secret is null)
            {
                return false;
            }

            string? timestamp = null;
            string? provided = null;
            foreach (var piece in signatureHeader.Split(','))
            {
                var index = piece.IndexOf('=');
                if (index <= 0)
                {
                    continue;
                }

                var key = piece.Substring(0, index).Trim();
                var value = piece.Substring(index + 1).Trim();
                if (key == "t")
                {
                    timestamp = value;
                }
                else if (key == "v1")
                {
                    provided = value;
                }
            }

            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(provided))
            {
                return false;
            }

            byte[] hash;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp + "." + payload));
            }

            var expected = ToHex(hash);
            if (!FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(provided!)))
            {
                return false;
            }

            if (toleranceSeconds > 0)
            {
                if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var ts))
                {
                    return false;
                }

                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (Math.Abs(now - ts) > toleranceSeconds)
                {
                    return false;
                }
            }

            return true;
        }

        private static string ToHex(byte[] bytes)
        {
            var chars = new char[bytes.Length * 2];
            var i = 0;
            foreach (var b in bytes)
            {
                chars[i++] = HexChar(b >> 4);
                chars[i++] = HexChar(b & 0xF);
            }

            return new string(chars);
        }

        private static char HexChar(int value) => (char)(value < 10 ? '0' + value : 'a' + (value - 10));

        // Constant-time comparison, portable across all target frameworks.
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}
