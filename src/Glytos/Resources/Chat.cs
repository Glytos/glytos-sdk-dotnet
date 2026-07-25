using System.Collections.Generic;
using System.Net.Http;
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
    }
}
