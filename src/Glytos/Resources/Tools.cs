using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Tools: manage the tools your agents can call.</summary>
    /// <remarks>
    /// A tool's kind is one of <c>static</c>, <c>http</c>, <c>mcp</c>, <c>code</c>,
    /// <c>integration</c> or <c>client</c>. An <c>integration</c> tool names its
    /// connection in its own config, so the model fills in arguments but never
    /// chooses the destination. <c>code</c> runs only in an operator-configured
    /// sandbox, and <c>client</c> is resolved by the browser during a web call, so
    /// both are inert unless that side is set up.
    /// </remarks>
    public sealed class Tools
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal Tools(GlytosClient client) => _client = client;

        /// <summary>List your saved tools.</summary>
        public Task<IReadOnlyList<Tool>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Tool>>(HttpMethod.Get, "/tools", cancellationToken: cancellationToken);

        /// <summary>
        /// Create a tool. <paramref name="kind"/> is one of <c>static</c>,
        /// <c>http</c>, <c>mcp</c>, <c>code</c>, <c>integration</c> or <c>client</c>.
        /// </summary>
        public Task<Tool> CreateAsync(
            string name,
            string kind,
            string? description = null,
            object? config = null,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["kind"] = kind,
            };
            if (description is not null)
            {
                body["description"] = description;
            }

            if (config is not null)
            {
                body["config"] = config;
            }

            if (parameters is not null)
            {
                body["parameters"] = parameters;
            }

            return _client.RequestAsync<Tool>(HttpMethod.Post, "/tools", body, cancellationToken: cancellationToken);
        }

        /// <summary>Update a tool. Only the fields you pass are changed.</summary>
        public Task<Tool> UpdateAsync(
            string toolUuid,
            string? name = null,
            string? description = null,
            string? kind = null,
            object? config = null,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (name is not null)
            {
                body["name"] = name;
            }

            if (description is not null)
            {
                body["description"] = description;
            }

            if (kind is not null)
            {
                body["kind"] = kind;
            }

            if (config is not null)
            {
                body["config"] = config;
            }

            if (parameters is not null)
            {
                body["parameters"] = parameters;
            }

            return _client.RequestAsync<Tool>(Patch, "/tools/" + System.Uri.EscapeDataString(toolUuid), body, cancellationToken: cancellationToken);
        }

        /// <summary>Delete a tool.</summary>
        public Task<JsonElement> DeleteAsync(string toolUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/tools/" + System.Uri.EscapeDataString(toolUuid), cancellationToken: cancellationToken);

        /// <summary>
        /// Ask an MCP server what it publishes, so a tool can be built from the
        /// server's own schema instead of one transcribed by hand. Returns the tool
        /// list itself, not the response envelope.
        /// </summary>
        public async Task<IReadOnlyList<McpTool>> DiscoverMcpAsync(
            string serverUrl,
            IDictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["server_url"] = serverUrl };
            if (headers is not null)
            {
                body["headers"] = headers;
            }

            var response = await _client.RequestAsync<McpDiscovery>(HttpMethod.Post, "/tools/mcp/discover", body, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Tools;
        }
    }
}
