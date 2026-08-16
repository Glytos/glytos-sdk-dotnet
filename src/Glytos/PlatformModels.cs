using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glytos
{
    // Models for the resources added after the first release. Same convention as
    // Models.cs: the fields you rely on plus an AdditionalData bag, so new API
    // fields are preserved rather than dropped and never break your build.

    /// <summary>A BYO SIP trunk registered with a carrier.</summary>
    public sealed record SipTrunk
    {
        /// <summary>Unique id of the trunk.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>The known-carrier preset this trunk was built from, when any.</summary>
        public string Preset { get; init; } = string.Empty;

        /// <summary>The carrier host calls are exchanged with.</summary>
        public string SipServer { get; init; } = string.Empty;

        /// <summary>The carrier port.</summary>
        public int SipPort { get; init; }

        /// <summary>udp, tcp or tls.</summary>
        public string Transport { get; init; } = string.Empty;

        /// <summary>The account or line id the carrier issued.</summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>registered, pending or failed. Only a registered trunk takes calls.</summary>
        public string? Status { get; init; }

        /// <summary>Why the trunk is not registered, when it is not.</summary>
        public string? StatusDetail { get; init; }

        /// <summary>When it last registered successfully.</summary>
        public string? LastRegisteredAt { get; init; }

        /// <summary>How many numbers are attached to it.</summary>
        public int NumberCount { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>
    /// A carrier whose connection settings are already known, so only the login
    /// has to be supplied.
    /// </summary>
    public sealed record SipPreset
    {
        /// <summary>The value to pass as the preset when creating a trunk.</summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>The carrier's name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>The carrier host.</summary>
        public string SipServer { get; init; } = string.Empty;

        /// <summary>The carrier port.</summary>
        public int SipPort { get; init; }

        /// <summary>udp, tcp or tls.</summary>
        public string Transport { get; init; } = string.Empty;

        /// <summary>The country the carrier serves.</summary>
        public string Country { get; init; } = string.Empty;

        /// <summary>
        /// Whether these settings have been confirmed against the live carrier,
        /// rather than taken from its documentation.
        /// </summary>
        public bool Verified { get; init; }

        /// <summary>Anything worth knowing before connecting this carrier.</summary>
        public string Note { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>The result of re-checking a trunk against its carrier.</summary>
    public sealed record SipTrunkTest
    {
        /// <summary>Whether the trunk is usable.</summary>
        public bool Ok { get; init; }

        /// <summary>What happened, in words.</summary>
        public string Detail { get; init; } = string.Empty;

        /// <summary>
        /// Whether the carrier answered at all. A carrier that refused the
        /// credentials is a different problem from one that never replied, and
        /// only the first is worth changing the password over.
        /// </summary>
        public bool Reachable { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A set of saved conversations replayed against one agent.</summary>
    public sealed record TestSuite
    {
        /// <summary>Unique id of the suite.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>The agent the cases run against.</summary>
        public string WorkflowUuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Fields not modelled above, including the cases themselves.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>The outcome of running every case in a suite.</summary>
    public sealed record TestSuiteRun
    {
        /// <summary>The suite that was run.</summary>
        public string SuiteUuid { get; init; } = string.Empty;

        /// <summary>Whether every case passed.</summary>
        public bool Passed { get; init; }

        /// <summary>How many cases ran.</summary>
        public int Total { get; init; }

        /// <summary>How many of them passed.</summary>
        public int PassedCount { get; init; }

        /// <summary>Fields not modelled above, including the per-case results.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A third-party destination the platform can act on.</summary>
    public sealed record Integration
    {
        /// <summary>The value to pass as the integration key.</summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>The credential fields a connection has to supply.</summary>
        public IReadOnlyList<string> RequiredCredentials { get; init; } = new List<string>();

        /// <summary>Which of those are safe to show back, so a form can be pre-filled.</summary>
        public IReadOnlyList<string>? PublicCredentials { get; init; }

        /// <summary>Whether an automation may fire this integration.</summary>
        public bool SupportsAutomation { get; init; }

        /// <summary>Fields not modelled above, including the action schemas.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>
    /// One configured destination. An organization can hold several per
    /// integration, so an agent tool or an automation names the connection.
    /// </summary>
    public sealed record IntegrationConnection
    {
        /// <summary>Unique id of the connection.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Which integration it configures.</summary>
        public string IntegrationKey { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Whether it is usable.</summary>
        public bool IsActive { get; init; }

        /// <summary>How many automations point at it.</summary>
        public int AutomationCount { get; init; }

        /// <summary>Fields not modelled above. Credentials come back masked.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>What an integration action returned.</summary>
    public sealed record IntegrationResult
    {
        /// <summary>The destination's reply.</summary>
        public JsonElement Result { get; init; }
    }

    /// <summary>A rule that fires an integration action when an event happens.</summary>
    public sealed record Automation
    {
        /// <summary>Unique id of the automation.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Whether it is currently firing.</summary>
        public bool IsActive { get; init; }

        /// <summary>A webhook event type, for example <c>session.completed</c>.</summary>
        public string TriggerEvent { get; init; } = string.Empty;

        /// <summary>The destination it acts on.</summary>
        public string ConnectionUuid { get; init; } = string.Empty;

        /// <summary>Which integration that connection belongs to.</summary>
        public string IntegrationKey { get; init; } = string.Empty;

        /// <summary>The action it runs.</summary>
        public string Action { get; init; } = string.Empty;

        /// <summary>Fields not modelled above, including the template and conditions.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>One firing of an automation.</summary>
    public sealed record AutomationRun
    {
        /// <summary>The event that fired it.</summary>
        public string EventType { get; init; } = string.Empty;

        /// <summary>How it ended.</summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>Why it failed, when it did.</summary>
        public string? Error { get; init; }

        /// <summary>How long the destination took.</summary>
        public int DurationMs { get; init; }

        /// <summary>When it ran.</summary>
        public string CreatedAt { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A trial firing: the rendered parameters and the destination's reply.</summary>
    public sealed record AutomationTest
    {
        /// <summary>What would be sent, with the template filled in.</summary>
        public JsonElement Params { get; init; }

        /// <summary>What the destination replied.</summary>
        public JsonElement Result { get; init; }
    }

    /// <summary>The organization's prepaid balance.</summary>
    public sealed record CreditBalance
    {
        /// <summary>How much credit is left.</summary>
        public decimal Balance { get; init; }

        /// <summary>The currency the balance is held in.</summary>
        public string Currency { get; init; } = string.Empty;
    }

    /// <summary>One entry in the credit ledger.</summary>
    public sealed record CreditTransaction
    {
        /// <summary>Positive for a top-up, negative for a debit.</summary>
        public decimal Amount { get; init; }

        /// <summary>What kind of movement it was.</summary>
        public string Kind { get; init; } = string.Empty;

        /// <summary>What it was for.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>The balance after it was applied.</summary>
        public decimal BalanceAfter { get; init; }

        /// <summary>When it happened.</summary>
        public string CreatedAt { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>Aggregate usage and cost for the organization.</summary>
    public sealed record UsageSummary
    {
        /// <summary>Total metered units.</summary>
        public decimal TotalUnits { get; init; }

        /// <summary>What those units cost.</summary>
        public decimal TotalCost { get; init; }

        /// <summary>How many usage records make it up.</summary>
        public int RecordCount { get; init; }

        /// <summary>The currency the cost is in.</summary>
        public string Currency { get; init; } = string.Empty;
    }

    /// <summary>One of Development, Staging or Production.</summary>
    public sealed record Environment
    {
        /// <summary>Unique id of the environment.</summary>
        public string Uuid { get; init; } = string.Empty;

        /// <summary>
        /// The stable id to pass as the client's environment: dev, staging or prod.
        /// </summary>
        public string Kind { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Whether this is the one used when none is named.</summary>
        public bool IsDefault { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>One entry in the model, transcriber and voice catalog.</summary>
    public sealed record Provider
    {
        /// <summary>The value to pass as the provider.</summary>
        public string Key { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>llm, stt, tts or realtime.</summary>
        public string ServiceType { get; init; } = string.Empty;

        /// <summary>The model used when an agent names none.</summary>
        public string DefaultModel { get; init; } = string.Empty;

        /// <summary>
        /// Whether it can be selected. An unavailable provider is shown as "Soon"
        /// rather than hidden.
        /// </summary>
        public bool Available { get; init; }

        /// <summary>Fields not modelled above, including the models and voices.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A key for calling this API. The secret is never returned after creation.</summary>
    public record ApiKey
    {
        /// <summary>Unique id of the key.</summary>
        public long Id { get; init; }

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>The leading characters of the secret, for recognising it.</summary>
        public string KeyPrefix { get; init; } = string.Empty;

        /// <summary>Whether it still works.</summary>
        public bool IsActive { get; init; }

        /// <summary>When it was last used, if ever.</summary>
        public string? LastUsedAt { get; init; }

        /// <summary>When it was created.</summary>
        public string? CreatedAt { get; init; }

        /// <summary>When it retires, when it does.</summary>
        public string? ExpiresAt { get; init; }

        /// <summary>What it may do, when it carries its own permissions.</summary>
        public IReadOnlyList<string>? Scopes { get; init; }

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>A newly created key: the one and only time the secret is returned.</summary>
    public sealed record CreatedApiKey : ApiKey
    {
        /// <summary>The secret. Store it now; it is not returned again.</summary>
        public string Key { get; init; } = string.Empty;
    }

    /// <summary>A workspace.</summary>
    public sealed record Organization
    {
        /// <summary>Unique id of the organization.</summary>
        public long Id { get; init; }

        /// <summary>Display name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>The url-safe form of the name.</summary>
        public string Slug { get; init; } = string.Empty;

        /// <summary>Immutable: where this organization's data lives.</summary>
        public string Region { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>One deployed stack data can live in.</summary>
    public sealed record Region
    {
        /// <summary>The value an organization carries as its region.</summary>
        public string Code { get; init; } = string.Empty;

        /// <summary>Display name.</summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>Empty for the stack you are already talking to.</summary>
        public string ApiBaseUrl { get; init; } = string.Empty;

        /// <summary>Fields not modelled above.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>One tool an MCP server publishes, as discovered from the server.</summary>
    public sealed record McpTool
    {
        /// <summary>The tool's name on the server.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>What it does, when the server says.</summary>
        public string? Description { get; init; }

        /// <summary>Fields not modelled above, including the parameter schema.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalData { get; init; }
    }

    /// <summary>The envelope MCP discovery returns.</summary>
    internal sealed class McpDiscovery
    {
        [JsonPropertyName("tools")]
        public IReadOnlyList<McpTool> Tools { get; init; } = new List<McpTool>();
    }
}
