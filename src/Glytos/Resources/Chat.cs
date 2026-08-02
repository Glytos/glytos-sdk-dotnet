using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Chat: mint hosted-chat tokens and post messages to text agents.</summary>
    public sealed class Chat
    {
        private readonly GlytosClient _client;

        internal Chat(GlytosClient client) => _client = client;

        /// <summary>Mint a short-lived token authorizing a hosted chat with an agent.</summary>
        public Task<ChatToken> TokenAsync(string workflowUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<ChatToken>(HttpMethod.Post, "/chat/token", new Dictionary<string, object?> { ["workflow_uuid"] = workflowUuid }, cancellationToken: cancellationToken);

        /// <summary>
        /// Post a message to a hosted chat. The conversation is authorized by the
        /// <paramref name="token"/> in the body; pass <paramref name="sessionUuid"/> to continue
        /// an existing conversation.
        /// </summary>
        public Task<ChatMessageResult> MessagesAsync(
            string token,
            string content,
            string? sessionUuid = null,
            object? images = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["token"] = token,
                ["content"] = content,
            };
            if (sessionUuid is not null)
            {
                body["session_uuid"] = sessionUuid;
            }

            if (images is not null)
            {
                body["images"] = images;
            }

            return _client.RequestAsync<ChatMessageResult>(HttpMethod.Post, "/chat/messages", body, cancellationToken: cancellationToken);
        }

        /// <summary>The same turn, delivered as it is written.</summary>
        public IAsyncEnumerable<StreamEvent> StreamAsync(
            string token,
            string content,
            string? sessionUuid = null,
            object? images = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["token"] = token,
                ["content"] = content,
            };
            if (sessionUuid is not null)
            {
                body["session_uuid"] = sessionUuid;
            }

            if (images is not null)
            {
                body["images"] = images;
            }

            return _client.StreamAsync(HttpMethod.Post, "/chat/stream", body, cancellationToken);
        }

        /// <summary>
        /// Attach a file to one conversation. Its text is put in front of the agent for
        /// that conversation only - it does not join the knowledge base.
        /// </summary>
        public Task<ChatFile> UploadFileAsync(
            string token,
            string sessionUuid,
            byte[] content,
            string filename = "file",
            CancellationToken cancellationToken = default) =>
            _client.UploadAsync<ChatFile>(
                "/chat/files",
                new Dictionary<string, string> { ["token"] = token, ["session_uuid"] = sessionUuid },
                filename,
                content,
                cancellationToken);

        /// <inheritdoc cref="UploadFileAsync(string,string,byte[],string,CancellationToken)" />
        public Task<ChatFile> UploadFileAsync(
            string token,
            string sessionUuid,
            string content,
            string filename = "file",
            CancellationToken cancellationToken = default) =>
            UploadFileAsync(token, sessionUuid, Encoding.UTF8.GetBytes(content), filename, cancellationToken);
    }
}
