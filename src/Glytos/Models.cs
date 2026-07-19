using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glytos
{
    // Each model carries the fields you rely on plus an AdditionalData bag, so new API
    // fields are preserved rather than dropped and never break your build.

    /// <summary>An agent: a prompt agent or a visual workflow.</summary>
    public sealed record Workflow
    {
        /// <summary>Unique id of the agent.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary><c>"prompt"</c> or <c>"workflow"</c>.</summary>
        public string Mode { get; init; } = string.Empty;

        /// <summary>Lifecycle status, when present.</summary>
        public string? Status { get; init; }

        /// <summary>Whether the agent is archived.</summary>
        public bool? Archived { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A phone or web call.</summary>
    public sealed record Call
    {
        /// <summary>Unique id of the call.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Current call status.</summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A short-lived token for an in-browser web call.</summary>
    public sealed record WebCallToken
    {
        /// <summary>The web-call token to hand to the browser SDK.</summary>
        public string Token { get; init; } = string.Empty;

        /// <summary>The realtime WebSocket URL the browser connects to.</summary>
        public string WsUrl { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A phone number on your account.</summary>
    public sealed record PhoneNumber
    {
        /// <summary>Unique id of the number.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>The number in E.164 format.</summary>
        public string E164 { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A conversation session (run) of an agent.</summary>
    public sealed record Session
    {
        /// <summary>Unique id of the session.</summary>
        public string SessionUuid { get; init; } = string.Empty;

        /// <summary>The agent this session ran against, when applicable.</summary>
        public string? WorkflowUuid { get; init; }

        /// <summary>Channel/mode of the session.</summary>
        public string? Mode { get; init; }

        /// <summary>Current status.</summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>When the session was created (ISO 8601).</summary>
        public string? CreatedAt { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A webhook endpoint subscription.</summary>
    public sealed record WebhookEndpoint
    {
        /// <summary>Unique id of the endpoint.</summary>
        public long Id { get; init; }

        /// <summary>The destination URL deliveries are POSTed to.</summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>The event types this endpoint is subscribed to.</summary>
        public IReadOnlyList<string> Events { get; init; } = new List<string>();

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }
}
