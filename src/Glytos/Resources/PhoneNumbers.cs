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
    }
}
