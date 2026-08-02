using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glytos
{
    /// <summary>
    /// A conversation with a text agent. Created by <see cref="Resources.Threads.CreateAsync"/>,
    /// it carries both ids the later calls need, so no call has to repeat the agent id.
    /// </summary>
    /// <remarks>
    /// Named <c>ChatThread</c> rather than <c>Thread</c> so it never collides with
    /// <see cref="System.Threading.Thread"/> in a file that imports both.
    /// </remarks>
    public sealed class ChatThread
    {
        /// <summary>Id of the conversation.</summary>
        public string Id { get; }

        /// <summary>Id of the agent the conversation runs on.</summary>
        public string Agent { get; }

        /// <summary>Conversation status, when the server reported one.</summary>
        public string? Status { get; }

        /// <summary>References an existing conversation by its two ids.</summary>
        public ChatThread(string id, string agent, string? status = null)
        {
            Id = id;
            Agent = agent;
            Status = status;
        }
    }

    /// <summary>One message in a conversation.</summary>
    public sealed record ThreadMessage
    {
        /// <summary><c>"user"</c>, <c>"assistant"</c> or <c>"system"</c>.</summary>
        public string Role { get; init; } = string.Empty;

        /// <summary>What was said.</summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>When it was said (ISO 8601), when recorded.</summary>
        public string? Ts { get; init; }

        /// <summary>The workflow node this message came from, for a visual agent.</summary>
        public string? NodeId { get; init; }

        /// <summary>Images attached to this message.</summary>
        public IReadOnlyList<string>? Images { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A thread plus everything recorded about it: full transcript, variables, cost.</summary>
    public sealed record ThreadDetail
    {
        /// <summary>Id of the conversation.</summary>
        [JsonPropertyName("session_uuid")]
        public string Id { get; init; } = string.Empty;

        /// <summary>Id of the agent the conversation ran on, when the server reported it.</summary>
        [JsonPropertyName("workflow_uuid")]
        public string? Agent { get; init; }

        /// <summary>Conversation status.</summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>Anything the agent opened with; empty for a silent opening.</summary>
        public IReadOnlyList<ThreadMessage>? Messages { get; init; }

        /// <summary>The whole conversation, oldest first.</summary>
        public IReadOnlyList<ThreadMessage>? Transcript { get; init; }

        /// <summary>Variables carried by the conversation.</summary>
        public IDictionary<string, JsonElement>? Variables { get; init; }

        /// <summary>When the conversation started (ISO 8601).</summary>
        public string? CreatedAt { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>One event from a streamed turn.</summary>
    public sealed class StreamEvent
    {
        /// <summary><c>"token"</c>, <c>"done"</c> or <c>"error"</c>.</summary>
        public string Type { get; }

        /// <summary>On a <c>token</c> event, the piece of the reply just written.</summary>
        public string Delta { get; }

        /// <summary>On an <c>error</c> event, what went wrong.</summary>
        public string Message { get; }

        /// <summary>
        /// On a <c>done</c> event, the same payload a non-streamed turn returns.
        /// <see cref="JsonValueKind.Undefined"/> on the other event types.
        /// </summary>
        public JsonElement Run { get; }

        internal StreamEvent(string type, string delta, string message, JsonElement run)
        {
            Type = type;
            Delta = delta;
            Message = message;
            Run = run;
        }
    }

    /// <summary>A folder grouping agents inside one environment.</summary>
    public sealed record AgentFolder
    {
        /// <summary>Unique id of the folder.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>How many agents are filed in it.</summary>
        public int? AgentCount { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A file attached to one conversation.</summary>
    public sealed record ChatFile
    {
        /// <summary>Unique id of the stored file.</summary>
        public string FileUuid { get; init; } = string.Empty;

        /// <summary>Original file name.</summary>
        public string? Filename { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }
}
