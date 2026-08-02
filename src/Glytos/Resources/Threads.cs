using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>
    /// Conversations with a text agent, in the vocabulary the rest of the industry uses:
    /// a thread holds the conversation, a run is one turn on it.
    /// </summary>
    /// <remarks>
    /// The same session API <see cref="GlytosClient.Agents"/> exposes, shaped so code
    /// written against a thread/run model reads the same here.
    /// </remarks>
    public sealed class Threads
    {
        private readonly GlytosClient _client;

        /// <summary>The messages on a thread.</summary>
        public ThreadMessages Messages { get; }

        /// <summary>The runs (turns) on a thread.</summary>
        public ThreadRuns Runs { get; }

        internal Threads(GlytosClient client)
        {
            _client = client;
            Messages = new ThreadMessages(client);
            Runs = new ThreadRuns(client);
        }

        /// <summary>Open a conversation with an agent.</summary>
        public async Task<ChatThread> CreateAsync(
            string agent,
            object? variables = null,
            object? version = null,
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

            var started = await _client
                .RequestAsync<ThreadDetail>(HttpMethod.Post, "/workflows/" + ThreadRef.Escape(agent) + "/sessions", body, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new ChatThread(started.Id, agent, started.Status);
        }

        /// <summary>The conversation so far, with its variables and cost.</summary>
        public Task<ThreadDetail> RetrieveAsync(ChatThread thread, CancellationToken cancellationToken = default)
        {
            ThreadRef.Check(thread);
            return _client.RequestAsync<ThreadDetail>(
                HttpMethod.Get,
                "/workflows/" + ThreadRef.Escape(thread.Agent) + "/sessions/" + ThreadRef.Escape(thread.Id),
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>The ids behind a thread, and the turn body shared by the plain and streamed calls.</summary>
    internal static class ThreadRef
    {
        internal static string Escape(string value) => Uri.EscapeDataString(value);

        internal static void Check(ChatThread thread)
        {
            if (thread is null)
            {
                throw new ArgumentNullException(nameof(thread));
            }

            if (string.IsNullOrEmpty(thread.Id) || string.IsNullOrEmpty(thread.Agent))
            {
                throw new ArgumentException("Glytos: a thread reference needs both id and agent.", nameof(thread));
            }
        }

        internal static string MessagesPath(ChatThread thread, string suffix)
        {
            Check(thread);
            return "/workflows/" + Escape(thread.Agent) + "/sessions/" + Escape(thread.Id) + "/messages" + suffix;
        }

        internal static Dictionary<string, object?> TurnBody(
            string content,
            IReadOnlyList<string>? images,
            string? instructions)
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
    }

    /// <summary>A thread's messages.</summary>
    public sealed class ThreadMessages
    {
        private readonly GlytosClient _client;

        internal ThreadMessages(GlytosClient client) => _client = client;

        /// <summary>
        /// Add a user message and run the agent on it. Returns that turn's reply.
        /// <paramref name="instructions"/> apply to this turn only and are never saved
        /// to the agent.
        /// </summary>
        public Task<JsonElement> CreateAsync(
            ChatThread thread,
            string content = "",
            IReadOnlyList<string>? images = null,
            string? instructions = null,
            CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(
                HttpMethod.Post,
                ThreadRef.MessagesPath(thread, string.Empty),
                ThreadRef.TurnBody(content, images, instructions),
                cancellationToken: cancellationToken);

        /// <summary>Every message in the conversation, oldest first.</summary>
        public async Task<IReadOnlyList<ThreadMessage>> ListAsync(
            ChatThread thread,
            CancellationToken cancellationToken = default)
        {
            ThreadRef.Check(thread);
            var detail = await _client
                .RequestAsync<ThreadDetail>(
                    HttpMethod.Get,
                    "/workflows/" + ThreadRef.Escape(thread.Agent) + "/sessions/" + ThreadRef.Escape(thread.Id),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return detail.Transcript ?? new List<ThreadMessage>();
        }
    }

    /// <summary>One turn on a thread.</summary>
    public sealed class ThreadRuns
    {
        private readonly GlytosClient _client;

        internal ThreadRuns(GlytosClient client) => _client = client;

        /// <summary>
        /// Run one turn and wait for it. A turn completes before it returns, so there is
        /// no run to poll: the reply is already in the result.
        /// </summary>
        public Task<JsonElement> CreateAsync(
            ChatThread thread,
            string content = "",
            IReadOnlyList<string>? images = null,
            string? instructions = null,
            CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(
                HttpMethod.Post,
                ThreadRef.MessagesPath(thread, string.Empty),
                ThreadRef.TurnBody(content, images, instructions),
                cancellationToken: cancellationToken);

        /// <summary>The same turn, delivered as it is written.</summary>
        public IAsyncEnumerable<StreamEvent> StreamAsync(
            ChatThread thread,
            string content = "",
            IReadOnlyList<string>? images = null,
            string? instructions = null,
            CancellationToken cancellationToken = default) =>
            _client.StreamAsync(
                HttpMethod.Post,
                ThreadRef.MessagesPath(thread, "/stream"),
                ThreadRef.TurnBody(content, images, instructions),
                cancellationToken);
    }
}
