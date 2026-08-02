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

        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        internal Workflows(GlytosClient client) => _client = client;

        /// <summary>
        /// List your agents (prompt agents and visual workflows). Pass an optional query to
        /// filter, e.g. <c>{ ["archived"] = true }</c> or <c>{ ["environment"] = "all" }</c>.
        /// </summary>
        public Task<IReadOnlyList<Workflow>> ListAsync(
            IDictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Workflow>>(HttpMethod.Get, "/workflows", query: query, cancellationToken: cancellationToken);

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

        /// <summary>Rename an agent (updates the display name only).</summary>
        public Task<Workflow> RenameAsync(string workflowUuid, string name, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(Patch, "/workflows/" + Uri(workflowUuid), new Dictionary<string, object?> { ["name"] = name }, cancellationToken: cancellationToken);

        /// <summary>
        /// Export an agent as portable, secret-free JSON. It imports back through
        /// <c>Imports.CreateAsync("glytos", ...)</c>, on this account or another.
        /// </summary>
        public Task<JsonElement> ExportAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Get, "/workflows/" + Uri(workflowUuid) + "/export", cancellationToken: cancellationToken);

        /// <summary>File an agent into a folder. Both must be in the same environment.</summary>
        public Task<Workflow> MoveToFolderAsync(string workflowUuid, string folderUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(Patch, "/workflows/" + Uri(workflowUuid), new Dictionary<string, object?> { ["folder_uuid"] = folderUuid }, cancellationToken: cancellationToken);

        /// <summary>Take an agent out of its folder, leaving it ungrouped.</summary>
        public Task<Workflow> RemoveFromFolderAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(Patch, "/workflows/" + Uri(workflowUuid), new Dictionary<string, object?> { ["folder_uuid"] = null }, cancellationToken: cancellationToken);

        /// <summary>Duplicate an agent, returning the new copy.</summary>
        public Task<Workflow> DuplicateAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Post, "/workflows/" + Uri(workflowUuid) + "/duplicate", cancellationToken: cancellationToken);

        /// <summary>Archive an agent (hide it without deleting).</summary>
        public Task<Workflow> ArchiveAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Post, "/workflows/" + Uri(workflowUuid) + "/archive", cancellationToken: cancellationToken);

        /// <summary>Unarchive a previously archived agent.</summary>
        public Task<Workflow> UnarchiveAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Post, "/workflows/" + Uri(workflowUuid) + "/unarchive", cancellationToken: cancellationToken);

        /// <summary>Promote an agent by moving it into the target environment.</summary>
        public Task<Workflow> PromoteAsync(string workflowUuid, string targetEnvironmentId, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Post, "/workflows/" + Uri(workflowUuid) + "/promote", new Dictionary<string, object?> { ["target_environment_id"] = targetEnvironmentId }, cancellationToken: cancellationToken);

        /// <summary>List the stored versions (revisions) of an agent.</summary>
        public Task<IReadOnlyList<WorkflowVersion>> VersionsAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<WorkflowVersion>>(HttpMethod.Get, "/workflows/" + Uri(workflowUuid) + "/versions", cancellationToken: cancellationToken);

        /// <summary>Replace the agent's workflow graph definition.</summary>
        public Task<Workflow> UpdateDefinitionAsync(string workflowUuid, object graph, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Put, "/workflows/" + Uri(workflowUuid) + "/definition", new Dictionary<string, object?> { ["graph"] = graph }, cancellationToken: cancellationToken);

        /// <summary>Replace the agent's configuration block.</summary>
        public Task<Workflow> UpdateConfigAsync(string workflowUuid, object config, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Workflow>(HttpMethod.Put, "/workflows/" + Uri(workflowUuid) + "/config", new Dictionary<string, object?> { ["config"] = config }, cancellationToken: cancellationToken);

        /// <summary>Start a new conversation session with the agent.</summary>
        public Task<Session> StartSessionAsync(
            string workflowUuid,
            object? variables = null,
            int? version = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (variables is not null)
            {
                body["variables"] = variables;
            }

            if (version is not null)
            {
                body["version"] = version;
            }

            return _client.RequestAsync<Session>(HttpMethod.Post, "/workflows/" + Uri(workflowUuid) + "/sessions", body, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Send a message to an existing session and get the turn's reply.
        /// <paramref name="instructions"/> apply to this turn only and are never saved
        /// to the agent.
        /// </summary>
        public Task<JsonElement> SendMessageAsync(
            string workflowUuid,
            string sessionUuid,
            string content,
            object? images = null,
            string? instructions = null,
            CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(
                HttpMethod.Post,
                "/workflows/" + Uri(workflowUuid) + "/sessions/" + Uri(sessionUuid) + "/messages",
                TurnBody(content, images, instructions),
                cancellationToken: cancellationToken);

        /// <summary>The same turn, delivered as it is written.</summary>
        public IAsyncEnumerable<StreamEvent> StreamMessageAsync(
            string workflowUuid,
            string sessionUuid,
            string content,
            object? images = null,
            string? instructions = null,
            CancellationToken cancellationToken = default) =>
            _client.StreamAsync(
                HttpMethod.Post,
                "/workflows/" + Uri(workflowUuid) + "/sessions/" + Uri(sessionUuid) + "/messages/stream",
                TurnBody(content, images, instructions),
                cancellationToken);

        private static Dictionary<string, object?> TurnBody(string content, object? images, string? instructions)
        {
            var body = new Dictionary<string, object?> { ["content"] = content };
            if (images is not null)
            {
                body["images"] = images;
            }

            if (instructions is not null)
            {
                body["additional_instructions"] = instructions;
            }

            return body;
        }

        /// <summary>
        /// Run the agent over a full message list in one shot. <paramref name="messages"/> is a
        /// list of <c>{ role, content }</c> objects.
        /// </summary>
        public Task<JsonElement> RunTextAsync(string workflowUuid, object messages, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Post, "/workflows/" + Uri(workflowUuid) + "/runs/text", new Dictionary<string, object?> { ["messages"] = messages }, cancellationToken: cancellationToken);

        private static string Uri(string value) => System.Uri.EscapeDataString(value);
    }
}
