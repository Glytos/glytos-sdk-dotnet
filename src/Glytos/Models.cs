using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glytos
{
    // Each model carries the fields you rely on plus an AdditionalData bag, so new API
    // fields are preserved rather than dropped and never break your build.

    /// <summary>
    /// A paginated list envelope. Some list endpoints wrap their results in
    /// <c>{ items, total, limit, offset }</c>; resources unwrap it so every list
    /// method returns a plain collection.
    /// </summary>
    internal sealed class Paginated<T>
    {
        [JsonPropertyName("items")]
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
    }

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
    public record Campaign
    {
        /// <summary>Unique id of the campaign.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// One of <c>draft</c>, <c>scheduled</c>, <c>running</c>, <c>waiting</c>,
        /// <c>stopped</c>, <c>completed</c>, <c>halted</c>, <c>out_of_credit</c>
        /// or <c>failed</c>.
        /// </summary>
        public string? Status { get; init; }

        /// <summary>
        /// Why a campaign stopped short of the end of its list, so <c>halted</c>
        /// and <c>out_of_credit</c> are actionable.
        /// </summary>
        public string? StatusDetail { get; init; }

        /// <summary>The agent this campaign dials with, when present.</summary>
        public string? WorkflowUuid { get; init; }

        /// <summary>The caller id this campaign dials from.</summary>
        public string? FromNumber { get; init; }

        /// <summary>When dialing starts, if it was scheduled for the future.</summary>
        public string? ScheduledAt { get; init; }

        /// <summary>When dialing actually began.</summary>
        public string? StartedAt { get; init; }

        /// <summary>When the campaign reached the end of its list, or was stopped.</summary>
        public string? FinishedAt { get; init; }

        /// <summary>Start of the dialing hours, read in <see cref="Timezone"/>.</summary>
        public string? CallWindowStart { get; init; }

        /// <summary>End of the dialing hours, read in <see cref="Timezone"/>.</summary>
        public string? CallWindowEnd { get; init; }

        /// <summary>The IANA zone the calling window is read in.</summary>
        public string? Timezone { get; init; }

        /// <summary>
        /// How much of the do-not-call list this campaign honours: <c>strict</c>,
        /// <c>transactional</c> or <c>ignore</c>.
        /// </summary>
        public string? SuppressionPolicy { get; init; }

        /// <summary>
        /// Whether this campaign also calls people who asked, on a call, not to be
        /// contacted again.
        /// </summary>
        public bool? OverrideCallerRequests { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>One dial target and what became of it.</summary>
    public sealed record CampaignContact
    {
        /// <summary>The number dialed, in international form.</summary>
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// One of <c>pending</c>, <c>dialing</c>, <c>answered</c>, <c>voicemail</c>,
        /// <c>no_answer</c>, <c>failed</c> or <c>suppressed</c>. Busy is not reported
        /// separately from <c>no_answer</c>: it needs per-carrier callbacks the
        /// platform does not collect.
        /// </summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>The carrier's own id for the call.</summary>
        public string? CallSid { get; init; }

        /// <summary>The carrier's own words when it refused the number.</summary>
        public string? Error { get; init; }

        /// <summary>The conversation this contact produced, if it answered.</summary>
        public string? SessionUuid { get; init; }

        /// <summary>
        /// The contact's other CSV columns, which reach the agent's prompt, so
        /// <c>{{name}}</c> means this person.
        /// </summary>
        public IDictionary<string, string>? Variables { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A campaign with its contact list.</summary>
    public sealed record CampaignDetail : Campaign
    {
        /// <summary>The contacts and their outcomes.</summary>
        public IReadOnlyList<CampaignContact> Contacts { get; init; } = new List<CampaignContact>();
    }

    /// <summary>How many of a contact list each suppression policy would reach.</summary>
    public sealed record SuppressionPreview
    {
        /// <summary>How many usable numbers the list held.</summary>
        public int Contacts { get; init; }

        /// <summary>How many of them are on the do-not-call list at all.</summary>
        public int SuppressedTotal { get; init; }

        /// <summary>How many of them asked, on a call, not to be contacted again.</summary>
        public int CallerRequested { get; init; }

        /// <summary>Reachable under the default policy.</summary>
        public int ReachedIfStrict { get; init; }

        /// <summary>Reachable if entries that only refused marketing are skipped.</summary>
        public int ReachedIfTransactional { get; init; }

        /// <summary>Reachable if the organization's own entries are skipped.</summary>
        public int ReachedIfIgnore { get; init; }

        /// <summary>Reachable if caller requests are overruled as well.</summary>
        public int ReachedIfOverride { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>What adding contacts to a campaign did.</summary>
    public sealed record ContactSyncResult
    {
        /// <summary>How many contacts were appended.</summary>
        public int Added { get; init; }

        /// <summary>How many were already on the list.</summary>
        public int Skipped { get; init; }

        /// <summary>How many rows held no usable phone number.</summary>
        public int Rejected { get; init; }

        /// <summary>
        /// The column read as the phone number, so a file read from the wrong one
        /// is distinguishable from one that could not be read at all.
        /// </summary>
        public string? PhoneColumn { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A number this organization must not call.</summary>
    public sealed record DncEntry
    {
        /// <summary>Unique id of the entry.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>The number, in international form.</summary>
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// How it got here: <c>agent</c> (the person asked on a call), <c>manual</c>,
        /// <c>import</c> or <c>api</c>.
        /// </summary>
        public string Source { get; init; } = string.Empty;

        /// <summary>How far it reaches: <c>all</c> or <c>marketing</c>.</summary>
        public string Scope { get; init; } = string.Empty;

        /// <summary>Why the number was suppressed.</summary>
        public string? Reason { get; init; }

        /// <summary>The last time a campaign or call was blocked by this entry.</summary>
        public string? LastMatchedAt { get; init; }

        /// <summary>When the entry was added.</summary>
        public string? CreatedAt { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A page of the do-not-call list.</summary>
    public sealed record DncList
    {
        /// <summary>The entries on this page.</summary>
        public IReadOnlyList<DncEntry> Items { get; init; } = new List<DncEntry>();

        /// <summary>How many entries the list holds in total.</summary>
        public int Total { get; init; }
    }

    /// <summary>What a bulk add to the do-not-call list did.</summary>
    public sealed record DncImportResult
    {
        /// <summary>How many numbers were suppressed.</summary>
        public int Added { get; init; }

        /// <summary>How many were already on the list.</summary>
        public int Duplicates { get; init; }

        /// <summary>How many were not phone numbers.</summary>
        public int Rejected { get; init; }

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
