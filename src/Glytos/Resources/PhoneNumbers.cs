using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Phone numbers: search carrier inventory, provision, and assign numbers.</summary>
    public sealed class PhoneNumbers
    {
        private readonly GlytosClient _client;

        internal PhoneNumbers(GlytosClient client) => _client = client;

        /// <summary>Search carrier inventory for available numbers.</summary>
        public Task<JsonElement> SearchAsync(IDictionary<string, object?> query, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Get, "/telephony/numbers/search", query: query, cancellationToken: cancellationToken);

        /// <summary>List the numbers on your account.</summary>
        public Task<IReadOnlyList<PhoneNumber>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<PhoneNumber>>(HttpMethod.Get, "/telephony/numbers", cancellationToken: cancellationToken);

        /// <summary>Provision (buy) a number by its E.164 value.</summary>
        public Task<PhoneNumber> ProvisionAsync(
            string e164,
            IDictionary<string, object?>? options = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["e164"] = e164 };
            if (options is not null)
            {
                foreach (var pair in options)
                {
                    body[pair.Key] = pair.Value;
                }
            }

            return _client.RequestAsync<PhoneNumber>(HttpMethod.Post, "/telephony/numbers", body, cancellationToken: cancellationToken);
        }

        /// <summary>Assign a number to an agent.</summary>
        public Task<PhoneNumber> AssignAsync(string numberUuid, object body, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<PhoneNumber>(HttpMethod.Post, "/telephony/numbers/" + System.Uri.EscapeDataString(numberUuid) + "/assign", body, cancellationToken: cancellationToken);

        /// <summary>Release (delete) a number.</summary>
        public Task<JsonElement> ReleaseAsync(string numberUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/telephony/numbers/" + System.Uri.EscapeDataString(numberUuid), cancellationToken: cancellationToken);

        /// <summary>List the telephony providers available to your organization.</summary>
        public Task<JsonElement> ProvidersAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Get, "/telephony/providers", cancellationToken: cancellationToken);

        /// <summary>Import an existing number you already own on a carrier.</summary>
        public Task<PhoneNumber> ImportNumberAsync(
            string e164,
            string? provider = null,
            string? providerSid = null,
            object? credentials = null,
            string? workflowUuid = null,
            string? sipTrunkUuid = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["e164"] = e164 };
            if (provider is not null)
            {
                body["provider"] = provider;
            }

            if (providerSid is not null)
            {
                body["provider_sid"] = providerSid;
            }

            if (credentials is not null)
            {
                body["credentials"] = credentials;
            }

            if (workflowUuid is not null)
            {
                body["workflow_uuid"] = workflowUuid;
            }

            if (sipTrunkUuid is not null)
            {
                body["sip_trunk_uuid"] = sipTrunkUuid;
            }

            return _client.RequestAsync<PhoneNumber>(HttpMethod.Post, "/telephony/numbers/import", body, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Provision an instant platform number. The country and provider are sent as query
        /// parameters, not a request body.
        /// </summary>
        public Task<PhoneNumber> InstantAsync(
            string? country = null,
            string? provider = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>();
            if (country is not null)
            {
                query["country"] = country;
            }

            if (provider is not null)
            {
                query["provider"] = provider;
            }

            return _client.RequestAsync<PhoneNumber>(HttpMethod.Post, "/telephony/numbers/instant", query: query, cancellationToken: cancellationToken);
        }
    }
}
