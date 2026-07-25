using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Campaigns: run outbound calling campaigns over your agents.</summary>
    public sealed class Campaigns
    {
        private readonly GlytosClient _client;

        internal Campaigns(GlytosClient client) => _client = client;

        /// <summary>List your calling campaigns.</summary>
        public Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Campaign>>(HttpMethod.Get, "/telephony/campaigns", cancellationToken: cancellationToken);

        /// <summary>Create a calling campaign for an agent.</summary>
        public Task<Campaign> CreateAsync(
            string name,
            string workflowUuid,
            string fromNumber,
            object? contacts = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["workflow_uuid"] = workflowUuid,
                ["from_number"] = fromNumber,
            };
            if (contacts is not null)
            {
                body["contacts"] = contacts;
            }

            return _client.RequestAsync<Campaign>(HttpMethod.Post, "/telephony/campaigns", body, cancellationToken: cancellationToken);
        }

        /// <summary>Retrieve a campaign by uuid.</summary>
        public Task<CampaignDetail> RetrieveAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<CampaignDetail>(HttpMethod.Get, "/telephony/campaigns/" + System.Uri.EscapeDataString(campaignUuid), cancellationToken: cancellationToken);

        /// <summary>Start dialing a campaign.</summary>
        public Task<JsonElement> StartAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Post, "/telephony/campaigns/" + System.Uri.EscapeDataString(campaignUuid) + "/start", cancellationToken: cancellationToken);

        /// <summary>Sync the campaign's contact list from a remote source URL.</summary>
        public Task<JsonElement> SyncContactsAsync(string campaignUuid, string sourceUrl, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Post, "/telephony/campaigns/" + System.Uri.EscapeDataString(campaignUuid) + "/contacts/sync", new Dictionary<string, object?> { ["source_url"] = sourceUrl }, cancellationToken: cancellationToken);
    }
}
