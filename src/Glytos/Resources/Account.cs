using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>
    /// Test suites: saved conversations replayed against an agent, to catch
    /// prompt regressions.
    /// </summary>
    public sealed class TestSuites
    {
        private readonly GlytosClient _client;

        internal TestSuites(GlytosClient client) => _client = client;

        /// <summary>List your test suites.</summary>
        public Task<IReadOnlyList<TestSuite>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<TestSuite>>(HttpMethod.Get, "/test-suites", cancellationToken: cancellationToken);

        /// <summary>Create a suite of cases against one agent.</summary>
        public Task<TestSuite> CreateAsync(
            string workflowUuid,
            string name,
            object? cases = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["workflow_uuid"] = workflowUuid,
                ["name"] = name,
            };
            if (cases is not null)
            {
                body["cases"] = cases;
            }

            return _client.RequestAsync<TestSuite>(HttpMethod.Post, "/test-suites", body, cancellationToken: cancellationToken);
        }

        /// <summary>Rename a suite, repoint it at another agent, or rewrite its cases.</summary>
        public Task<TestSuite> UpdateAsync(
            string suiteUuid,
            string? name = null,
            string? workflowUuid = null,
            object? cases = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (name is not null)
            {
                body["name"] = name;
            }
            if (workflowUuid is not null)
            {
                body["workflow_uuid"] = workflowUuid;
            }
            if (cases is not null)
            {
                body["cases"] = cases;
            }

            return _client.RequestAsync<TestSuite>(HttpMethod.Put, "/test-suites/" + Uri.EscapeDataString(suiteUuid), body, cancellationToken: cancellationToken);
        }

        /// <summary>Delete a suite.</summary>
        public Task<JsonElement> DeleteAsync(string suiteUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/test-suites/" + Uri.EscapeDataString(suiteUuid), cancellationToken: cancellationToken);

        /// <summary>
        /// Run every case and report which passed. This runs the agent, so it
        /// spends credit.
        /// </summary>
        public Task<TestSuiteRun> RunAsync(string suiteUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<TestSuiteRun>(HttpMethod.Post, "/test-suites/" + Uri.EscapeDataString(suiteUuid) + "/run", cancellationToken: cancellationToken);
    }

    /// <summary>Billing: credit balance, ledger and usage.</summary>
    public sealed class Billing
    {
        private readonly GlytosClient _client;

        internal Billing(GlytosClient client) => _client = client;

        /// <summary>
        /// The current prepaid balance. Worth checking before a large outbound
        /// run: a call is refused below the minimum, so a campaign that runs out
        /// simply stops.
        /// </summary>
        public Task<CreditBalance> CreditsAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<CreditBalance>(HttpMethod.Get, "/billing/credits", cancellationToken: cancellationToken);

        /// <summary>The credit ledger: top-ups and debits, newest first.</summary>
        public Task<IReadOnlyList<CreditTransaction>> TransactionsAsync(
            string? kind = null,
            int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>();
            if (kind is not null)
            {
                query["kind"] = kind;
            }

            if (limit is not null)
            {
                query["limit"] = limit;
            }

            return _client.RequestAsync<IReadOnlyList<CreditTransaction>>(HttpMethod.Get, "/billing/credits/transactions", query: query, cancellationToken: cancellationToken);
        }

        /// <summary>Aggregate usage and cost for the organization.</summary>
        public Task<UsageSummary> UsageAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<UsageSummary>(HttpMethod.Get, "/billing/usage", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Environments: Development, Staging and Production. Set the client's
    /// environment to a kind or a uuid to scope reads and calls; agents are
    /// created in Development whatever it is set to.
    /// </summary>
    public sealed class Environments
    {
        private readonly GlytosClient _client;

        internal Environments(GlytosClient client) => _client = client;

        /// <summary>The organization's three environments.</summary>
        public Task<IReadOnlyList<Environment>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Environment>>(HttpMethod.Get, "/environments", cancellationToken: cancellationToken);
    }

    /// <summary>Providers: the model, transcriber and voice catalog.</summary>
    public sealed class Providers
    {
        private readonly GlytosClient _client;

        internal Providers(GlytosClient client) => _client = client;

        /// <summary>
        /// Every provider and model, and whether it is available to you. An
        /// unavailable provider is shown as "Soon" rather than hidden.
        /// </summary>
        public Task<IReadOnlyList<Provider>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Provider>>(HttpMethod.Get, "/providers", cancellationToken: cancellationToken);

        /// <summary>
        /// One provider's live models and voices, fetched from the provider itself
        /// where it publishes them. <paramref name="language"/> narrows a long
        /// voice list.
        /// </summary>
        public Task<JsonElement> ResourcesAsync(
            string serviceType,
            string key,
            string? language = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>();
            if (language is not null)
            {
                query["language"] = language;
            }

            var path = "/providers/" + Uri.EscapeDataString(serviceType) + "/" + Uri.EscapeDataString(key) + "/resources";
            return _client.RequestAsync<JsonElement>(HttpMethod.Get, path, query: query, cancellationToken: cancellationToken);
        }
    }

    /// <summary>API keys: keys for calling this API.</summary>
    public sealed class ApiKeys
    {
        private readonly GlytosClient _client;

        internal ApiKeys(GlytosClient client) => _client = client;

        /// <summary>
        /// List the keys on the organization. Secrets are never returned.
        /// </summary>
        public Task<IReadOnlyList<ApiKey>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<ApiKey>>(HttpMethod.Get, "/api-keys", cancellationToken: cancellationToken);

        /// <summary>
        /// Create a key. The secret is in the response and nowhere else, so store
        /// it now.
        /// </summary>
        /// <remarks>
        /// <paramref name="scopes"/> bounds what the key may do and cannot exceed
        /// what you hold. Leave it null and the key inherits your permissions,
        /// which means it stops working if you leave the organization.
        /// <paramref name="expiresInDays"/> retires the key on its own. Omitting
        /// both is exactly the behaviour keys have always had.
        /// </remarks>
        public Task<CreatedApiKey> CreateAsync(
            string name,
            int? expiresInDays = null,
            IEnumerable<string>? scopes = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["name"] = name };
            if (expiresInDays is not null)
            {
                body["expires_in_days"] = expiresInDays;
            }

            if (scopes is not null)
            {
                body["scopes"] = new List<string>(scopes);
            }

            return _client.RequestAsync<CreatedApiKey>(HttpMethod.Post, "/api-keys", body, cancellationToken: cancellationToken);
        }

        /// <summary>Revoke a key immediately.</summary>
        public Task<JsonElement> DeleteAsync(long keyId, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/api-keys/" + keyId.ToString(CultureInfo.InvariantCulture), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Organizations: the one this key belongs to, and the regions data can live
    /// in.
    /// </summary>
    public sealed class Organizations
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal Organizations(GlytosClient client) => _client = client;

        /// <summary>The organization behind this API key.</summary>
        public Task<Organization> RetrieveAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Organization>(HttpMethod.Get, "/organization", cancellationToken: cancellationToken);

        /// <summary>
        /// Rename the organization. Its region is fixed at creation and cannot be
        /// changed.
        /// </summary>
        public Task<Organization> UpdateAsync(string name, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Organization>(Patch, "/organization", new Dictionary<string, object?> { ["name"] = name }, cancellationToken: cancellationToken);

        /// <summary>
        /// The regions this deployment offers. Each is a separate stack with its
        /// own base URL, so reaching an organization in another region means
        /// pointing the client's base URL there with a key issued there.
        /// </summary>
        public Task<IReadOnlyList<Region>> RegionsAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Region>>(HttpMethod.Get, "/regions", cancellationToken: cancellationToken);
    }
}
