using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Tools: manage the tools your agents can call.</summary>
    public sealed class Tools
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal Tools(GlytosClient client) => _client = client;

        /// <summary>List your saved tools.</summary>
        public Task<IReadOnlyList<Tool>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Tool>>(HttpMethod.Get, "/tools", cancellationToken: cancellationToken);

        /// <summary>Create a tool. <paramref name="kind"/> is <c>"http"</c>, <c>"static"</c>, or <c>"mcp"</c>.</summary>
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
    }
}
