using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>
    /// SIP trunks: connect a carrier directly, with no third party in between.
    /// </summary>
    /// <remarks>
    /// A trunk registers with the carrier using credentials it issued you. Numbers
    /// are attached to a registered trunk through
    /// <see cref="PhoneNumbers.ImportNumberAsync"/> with a trunk uuid.
    /// </remarks>
    public sealed class SipTrunks
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal SipTrunks(GlytosClient client) => _client = client;

        /// <summary>
        /// Carriers whose connection settings are already known, so only the login
        /// has to be supplied.
        /// </summary>
        public Task<IReadOnlyList<SipPreset>> PresetsAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<SipPreset>>(HttpMethod.Get, "/telephony/sip-trunks/presets", cancellationToken: cancellationToken);

        /// <summary>List your trunks and their registration state.</summary>
        public Task<IReadOnlyList<SipTrunk>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<SipTrunk>>(HttpMethod.Get, "/telephony/sip-trunks", cancellationToken: cancellationToken);

        /// <summary>
        /// Register a trunk. Give a <paramref name="preset"/> and the server, port
        /// and transport are filled in from it; otherwise set them through
        /// <paramref name="options"/>. The password is stored encrypted and is
        /// never returned.
        /// </summary>
        public Task<SipTrunk> CreateAsync(
            string username,
            string password,
            string? name = null,
            string? preset = null,
            IDictionary<string, object?>? options = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["username"] = username,
                ["password"] = password,
            };
            if (name is not null)
            {
                body["name"] = name;
            }

            if (preset is not null)
            {
                body["preset"] = preset;
            }

            if (options is not null)
            {
                foreach (var pair in options)
                {
                    body[pair.Key] = pair.Value;
                }
            }

            return _client.RequestAsync<SipTrunk>(HttpMethod.Post, "/telephony/sip-trunks", body, cancellationToken: cancellationToken);
        }

        /// <summary>Update a trunk. Only the fields you pass are changed.</summary>
        public Task<SipTrunk> UpdateAsync(
            string trunkUuid,
            IDictionary<string, object?> fields,
            CancellationToken cancellationToken = default) =>
            _client.RequestAsync<SipTrunk>(Patch, "/telephony/sip-trunks/" + Uri.EscapeDataString(trunkUuid), fields, cancellationToken: cancellationToken);

        /// <summary>
        /// Remove a trunk. Numbers attached to it stop receiving calls.
        /// </summary>
        public Task<JsonElement> DeleteAsync(string trunkUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/telephony/sip-trunks/" + Uri.EscapeDataString(trunkUuid), cancellationToken: cancellationToken);

        /// <summary>
        /// Re-check the trunk against its carrier now, rather than waiting for the
        /// next reconcile. <see cref="SipTrunkTest.Reachable"/> separates "the
        /// carrier refused these credentials" from "nobody answered"; only the
        /// first is worth changing the password over.
        /// </summary>
        public Task<SipTrunkTest> TestAsync(string trunkUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<SipTrunkTest>(HttpMethod.Post, "/telephony/sip-trunks/" + Uri.EscapeDataString(trunkUuid) + "/test", cancellationToken: cancellationToken);
    }
}
