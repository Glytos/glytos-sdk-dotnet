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

    /// <summary>A delivery attempt for a webhook event.</summary>
    public sealed record WebhookDelivery
    {
        /// <summary>Unique id of the delivery.</summary>
        public long Id { get; init; }

        /// <summary>The event type that was delivered.</summary>
        public string? EventType { get; init; }

        /// <summary>Delivery status (e.g. <c>"delivered"</c>, <c>"failed"</c>).</summary>
        public string? Status { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A single stored version (revision) of an agent.</summary>
    public sealed record WorkflowVersion
    {
        /// <summary>The version number, when present.</summary>
        public int? Version { get; init; }

        /// <summary>When this version was created (ISO 8601), when present.</summary>
        public string? CreatedAt { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>An outbound calling campaign.</summary>
    public sealed record Campaign
    {
        /// <summary>Unique id of the campaign.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Current campaign status.</summary>
        public string? Status { get; init; }

        /// <summary>The agent this campaign dials with, when present.</summary>
        public string? WorkflowUuid { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>Full detail for one campaign, including its contacts when present.</summary>
    public sealed record CampaignDetail
    {
        /// <summary>Unique id of the campaign.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Current campaign status.</summary>
        public string? Status { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A saved tool the agent can call.</summary>
    public sealed record Tool
    {
        /// <summary>Unique id of the tool.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Tool kind (e.g. <c>"http"</c>, <c>"static"</c>, <c>"mcp"</c>).</summary>
        public string Kind { get; init; } = string.Empty;

        /// <summary>Human-readable description, when present.</summary>
        public string? Description { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A knowledge-base document.</summary>
    public sealed record Document
    {
        /// <summary>Unique id of the document.</summary>
        public long Id { get; init; }

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A single hit from a knowledge-base search.</summary>
    public sealed record SearchHit
    {
        /// <summary>The id of the document this hit came from, when present.</summary>
        public long? DocumentId { get; init; }

        /// <summary>Relevance score of the hit.</summary>
        public double Score { get; init; }

        /// <summary>The matched text content, when present.</summary>
        public string? Content { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A vector store over your knowledge base.</summary>
    public sealed record VectorStore
    {
        /// <summary>Unique id of the vector store.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>Full detail for one vector store, including its documents when present.</summary>
    public sealed record VectorStoreDetail
    {
        /// <summary>Unique id of the vector store.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>Aggregated usage and performance metrics over a time window.</summary>
    public sealed record AnalyticsOverview
    {
        /// <summary>All metric fields returned by the overview endpoint.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A short-lived token authorizing a hosted chat conversation.</summary>
    public sealed record ChatToken
    {
        /// <summary>The chat token to send in subsequent message calls.</summary>
        public string Token { get; init; } = string.Empty;

        /// <summary>The agent this token is scoped to.</summary>
        public string WorkflowUuid { get; init; } = string.Empty;

        /// <summary>Lifetime of the token, in seconds.</summary>
        public int ExpiresIn { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>The result of posting one message to a hosted chat conversation.</summary>
    public sealed record ChatMessageResult
    {
        /// <summary>The conversation session this turn belongs to, when present.</summary>
        public string? SessionUuid { get; init; }

        /// <summary>Fields not modelled above (e.g. the assistant reply messages).</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }
}
