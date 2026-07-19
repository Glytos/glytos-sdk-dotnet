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

        /// <summary>List calls.</summary>
        public Task<IReadOnlyList<Call>> ListAsync(
            IDictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Call>>(HttpMethod.Get, "/calls", query: query, cancellationToken: cancellationToken);

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
    }
}
