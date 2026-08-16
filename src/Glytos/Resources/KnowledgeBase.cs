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

        /// <summary>Upload a file as a knowledge-base document.</summary>
        public Task<Document> UploadDocumentAsync(
            byte[] content,
            string filename = "document",
            CancellationToken cancellationToken = default) =>
            _client.UploadAsync<Document>(
                "/knowledge-base/documents/upload",
                new Dictionary<string, string>(),
                filename,
                content,
                cancellationToken);

        /// <inheritdoc cref="UploadDocumentAsync(byte[],string,CancellationToken)" />
        public Task<Document> UploadDocumentAsync(
            string content,
            string filename = "document",
            CancellationToken cancellationToken = default) =>
            UploadDocumentAsync(System.Text.Encoding.UTF8.GetBytes(content), filename, cancellationToken);

        /// <summary>Run a hybrid (vector + full-text) search over your documents.</summary>
        /// <summary>One document, including its extracted text.</summary>
        public Task<Document> RetrieveDocumentAsync(long documentId, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Document>(HttpMethod.Get, "/knowledge-base/documents/" + documentId.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken: cancellationToken);

        /// <summary>Delete a document, with its chunks and embeddings.</summary>
        public Task<System.Text.Json.JsonElement> DeleteDocumentAsync(long documentId, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<System.Text.Json.JsonElement>(HttpMethod.Delete, "/knowledge-base/documents/" + documentId.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken: cancellationToken);

        /// <summary>Hybrid (vector + full-text) search over your documents.</summary>
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
