using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Knowledge base: manage documents and run hybrid retrieval.</summary>
    public sealed class KnowledgeBase
    {
        private readonly GlytosClient _client;

        internal KnowledgeBase(GlytosClient client) => _client = client;

        /// <summary>List your knowledge-base documents.</summary>
        public Task<IReadOnlyList<Document>> ListDocumentsAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Document>>(HttpMethod.Get, "/knowledge-base/documents", cancellationToken: cancellationToken);

        /// <summary>Create a knowledge-base document from raw text.</summary>
        public Task<Document> CreateDocumentAsync(
            string name,
            string content,
            int? chunkSize = null,
            int? chunkOverlap = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["content"] = content,
            };
            if (chunkSize is not null)
            {
                body["chunk_size"] = chunkSize;
            }

            if (chunkOverlap is not null)
            {
                body["chunk_overlap"] = chunkOverlap;
            }

            return _client.RequestAsync<Document>(HttpMethod.Post, "/knowledge-base/documents", body, cancellationToken: cancellationToken);
        }

        /// <summary>Run a hybrid (vector + full-text) search over your documents.</summary>
        public Task<IReadOnlyList<SearchHit>> SearchAsync(
            string query,
            int? topK = null,
            IReadOnlyList<int>? documentIds = null,
            double? minScore = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["query"] = query };
            if (topK is not null)
            {
                body["top_k"] = topK;
            }

            if (documentIds is not null)
            {
                body["document_ids"] = documentIds;
            }

            if (minScore is not null)
            {
                body["min_score"] = minScore;
            }

            return _client.RequestAsync<IReadOnlyList<SearchHit>>(HttpMethod.Post, "/knowledge-base/search", body, cancellationToken: cancellationToken);
        }
    }
}
