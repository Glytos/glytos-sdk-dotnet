using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Folders that group agents inside an environment.</summary>
    public sealed class Folders
    {
        private readonly GlytosClient _client;

        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        internal Folders(GlytosClient client) => _client = client;

        /// <summary>The folders in the active environment.</summary>
        public Task<IReadOnlyList<AgentFolder>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<AgentFolder>>(HttpMethod.Get, "/agent-folders", cancellationToken: cancellationToken);

        /// <summary>Create a folder in the active environment.</summary>
        public Task<AgentFolder> CreateAsync(string name, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<AgentFolder>(HttpMethod.Post, "/agent-folders", new Dictionary<string, object?> { ["name"] = name }, cancellationToken: cancellationToken);

        /// <summary>Rename a folder.</summary>
        public Task<AgentFolder> RenameAsync(string folderUuid, string name, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<AgentFolder>(Patch, "/agent-folders/" + Uri.EscapeDataString(folderUuid), new Dictionary<string, object?> { ["name"] = name }, cancellationToken: cancellationToken);

        /// <summary>Delete a folder. The agents filed in it are deleted with it.</summary>
        public Task<JsonElement> DeleteAsync(string folderUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/agent-folders/" + Uri.EscapeDataString(folderUuid), cancellationToken: cancellationToken);
    }

    /// <summary>Bring an agent over from another platform.</summary>
    public sealed class Imports
    {
        private readonly GlytosClient _client;

        internal Imports(GlytosClient client) => _client = client;

        /// <summary>The platforms an agent can be brought over from.</summary>
        public Task<JsonElement> SourcesAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Get, "/imports/sources", cancellationToken: cancellationToken);

        /// <summary>Bring an agent over from another platform's export.</summary>
        public Task<JsonElement> CreateAsync(string source, object payload, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(
                HttpMethod.Post,
                "/imports/" + Uri.EscapeDataString(source),
                new Dictionary<string, object?> { ["payload"] = payload },
                cancellationToken: cancellationToken);

        /// <summary>Bring over an assistant definition, tools and all.</summary>
        public Task<JsonElement> AssistantAsync(object assistant, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(
                HttpMethod.Post,
                "/imports/openai-assistant",
                new Dictionary<string, object?> { ["assistant"] = assistant },
                cancellationToken: cancellationToken);
    }
}
