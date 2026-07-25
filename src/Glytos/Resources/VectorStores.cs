using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Vector stores: group knowledge-base documents for retrieval.</summary>
    public sealed class VectorStores
    {
        private readonly GlytosClient _client;

        internal VectorStores(GlytosClient client) => _client = client;

        /// <summary>List your vector stores.</summary>
        public Task<IReadOnlyList<VectorStore>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<VectorStore>>(HttpMethod.Get, "/vector-stores", cancellationToken: cancellationToken);

        /// <summary>Create a vector store.</summary>
        public Task<VectorStore> CreateAsync(string name, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<VectorStore>(HttpMethod.Post, "/vector-stores", new Dictionary<string, object?> { ["name"] = name }, cancellationToken: cancellationToken);

        /// <summary>Retrieve a vector store by uuid.</summary>
        public Task<VectorStoreDetail> RetrieveAsync(string vectorStoreUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<VectorStoreDetail>(HttpMethod.Get, "/vector-stores/" + System.Uri.EscapeDataString(vectorStoreUuid), cancellationToken: cancellationToken);

        /// <summary>Delete a vector store.</summary>
        public Task<JsonElement> DeleteAsync(string vectorStoreUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/vector-stores/" + System.Uri.EscapeDataString(vectorStoreUuid), cancellationToken: cancellationToken);
    }
}
