using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Calls: start and manage phone and web calls.</summary>
    public sealed class Calls
    {
        private readonly GlytosClient _client;

        internal Calls(GlytosClient client) => _client = client;

        /// <summary>Start an outbound phone call, or run a transient (inline) agent.</summary>
        public Task<Call> CreateAsync(object body, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Call>(HttpMethod.Post, "/calls", body, cancellationToken: cancellationToken);

        /// <summary>List calls. The endpoint is paginated; its items are returned.</summary>
        public async Task<IReadOnlyList<Call>> ListAsync(
            IDictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default)
        {
            var page = await _client.RequestAsync<Paginated<Call>>(
                HttpMethod.Get, "/calls", query: query, cancellationToken: cancellationToken).ConfigureAwait(false);
            return page.Items;
        }

        /// <summary>Retrieve a call by uuid.</summary>
        public Task<Call> RetrieveAsync(string callUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Call>(HttpMethod.Get, "/calls/" + System.Uri.EscapeDataString(callUuid), cancellationToken: cancellationToken);

        /// <summary>
        /// Mint a short-lived, workflow-scoped token for an in-browser web call. Hand the
        /// returned <see cref="WebCallToken"/> to the browser and connect with <c>@glytos/web</c>.
        /// </summary>
        public Task<WebCallToken> WebTokenAsync(
            string? workflowUuid = null,
            object? agent = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (workflowUuid is not null)
            {
                body["workflow_uuid"] = workflowUuid;
            }

            if (agent is not null)
            {
                body["agent"] = agent;
            }

            return _client.RequestAsync<WebCallToken>(HttpMethod.Post, "/calls/web-token", body, cancellationToken: cancellationToken);
        }

        /// <summary>Control an in-progress call (e.g. transfer, hang up).</summary>
        public Task<JsonElement> ControlAsync(string callUuid, object body, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Post, "/calls/" + System.Uri.EscapeDataString(callUuid) + "/control", body, cancellationToken: cancellationToken);

        /// <summary>Make the agent speak a line on a call in progress.</summary>
        public Task<JsonElement> SayAsync(string callUuid, string text, CancellationToken cancellationToken = default) =>
            ControlAsync(callUuid, new Dictionary<string, object?> { ["action"] = "say", ["text"] = text }, cancellationToken);

        /// <summary>Hand a call in progress to a person.</summary>
        public Task<JsonElement> TransferAsync(string callUuid, string toNumber, CancellationToken cancellationToken = default) =>
            ControlAsync(callUuid, new Dictionary<string, object?> { ["action"] = "transfer", ["to_number"] = toNumber }, cancellationToken);

        /// <summary>Hang up a call in progress.</summary>
        public Task<JsonElement> EndAsync(string callUuid, CancellationToken cancellationToken = default) =>
            ControlAsync(callUuid, new Dictionary<string, object?> { ["action"] = "end" }, cancellationToken);
    }
}
