using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Agents: prompt agents and visual workflows.</summary>
    public sealed class Workflows
    {
        private readonly GlytosClient _client;

        internal Workflows(GlytosClient client) => _client = client;

        /// <summary>List your agents (prompt agents and visual workflows).</summary>
        public Task<IReadOnlyList<Workflow>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Workflow>>(HttpMethod.Get, "/workflows", cancellationToken: cancellationToken);

        /// <summary>Retrieve a single agent by uuid.</summary>
        public Task<Workflow> RetrieveAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Get, "/workflows/" + Uri(workflowUuid), cancellationToken: cancellationToken);

        /// <summary>Create an agent. <paramref name="mode"/> is <c>"prompt"</c> (default) or <c>"workflow"</c>.</summary>
        public Task<Workflow> CreateAsync(
            string name,
            string mode = "prompt",
            object? config = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["name"] = name, ["mode"] = mode };
            if (config is not null)
            {
                body["config"] = config;
            }

            return _client.RequestAsync<Workflow>(HttpMethod.Post, "/workflows", body, cancellationToken: cancellationToken);
        }

        /// <summary>Publish the current draft so the agent goes live.</summary>
        public Task<Workflow> PublishAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Post, "/workflows/" + Uri(workflowUuid) + "/publish", cancellationToken: cancellationToken);

        /// <summary>Delete an agent.</summary>
        public Task<JsonElement> DeleteAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/workflows/" + Uri(workflowUuid), cancellationToken: cancellationToken);

        /// <summary>Ready-made starter workflow graphs.</summary>
        public Task<IReadOnlyList<Workflow>> TemplatesAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Workflow>>(HttpMethod.Get, "/workflows/templates", cancellationToken: cancellationToken);

        /// <summary>Full detail for one session of an agent (transcript, cost, latency, ...).</summary>
        public Task<Session> SessionAsync(string workflowUuid, string sessionUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Session>(HttpMethod.Get, "/workflows/" + Uri(workflowUuid) + "/sessions/" + Uri(sessionUuid), cancellationToken: cancellationToken);

        /// <summary>The run-event log for a session (routing decisions, tool calls, ...).</summary>
        public Task<JsonElement> SessionEventsAsync(string workflowUuid, string sessionUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Get, "/workflows/" + Uri(workflowUuid) + "/sessions/" + Uri(sessionUuid) + "/events", cancellationToken: cancellationToken);

        private static string Uri(string value) => System.Uri.EscapeDataString(value);
    }
}
