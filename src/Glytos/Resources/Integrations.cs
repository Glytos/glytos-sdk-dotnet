using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>
    /// Integrations: third-party destinations - Slack, Discord, Telegram, a
    /// generic webhook, Cal.com - and the connections that hold their credentials.
    /// </summary>
    /// <remarks>
    /// A connection is reachable three ways: run it directly through
    /// <see cref="Connections"/>, give an agent a tool of kind <c>integration</c>
    /// that names it so the model can act mid-conversation, or fire it from an
    /// automation when an event happens.
    /// </remarks>
    public sealed class Integrations
    {
        private readonly GlytosClient _client;

        internal Integrations(GlytosClient client)
        {
            _client = client;
            Connections = new IntegrationConnections(client);
        }

        /// <summary>The configured destinations behind an integration.</summary>
        public IntegrationConnections Connections { get; }

        /// <summary>
        /// The catalog: what can be connected, and the actions each one offers.
        /// </summary>
        public Task<IReadOnlyList<Integration>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Integration>>(HttpMethod.Get, "/integrations", cancellationToken: cancellationToken);

        /// <summary>
        /// Run an action using whatever credentials the organization saved for this
        /// integration key.
        /// </summary>
        /// <remarks>
        /// Prefer <see cref="IntegrationConnections.RunAsync"/>: this resolves the
        /// organization's stored credentials for the integration, which is
        /// ambiguous once there is more than one destination for it.
        /// </remarks>
        public Task<IntegrationResult> RunAsync(
            string integrationKey,
            string action,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["action"] = action,
                ["params"] = parameters ?? new Dictionary<string, object?>(),
            };

            return _client.RequestAsync<IntegrationResult>(HttpMethod.Post, "/integrations/" + Uri.EscapeDataString(integrationKey) + "/run", body, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// The configured destinations behind an integration. An organization can hold
    /// several per integration - two Slack workspaces, three calendars - which is
    /// why an agent tool or an automation names the connection rather than the
    /// integration.
    /// </summary>
    public sealed class IntegrationConnections
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal IntegrationConnections(GlytosClient client) => _client = client;

        /// <summary>List configured connections, optionally for one integration.</summary>
        public Task<IReadOnlyList<IntegrationConnection>> ListAsync(
            string? integrationKey = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>();
            if (integrationKey is not null)
            {
                query["integration_key"] = integrationKey;
            }

            return _client.RequestAsync<IReadOnlyList<IntegrationConnection>>(HttpMethod.Get, "/integrations/connections", query: query, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Configure a destination. <paramref name="data"/> carries the
        /// integration's required credentials; they are encrypted at rest and
        /// masked when read back.
        /// </summary>
        public Task<IntegrationConnection> CreateAsync(
            string integrationKey,
            string name,
            object data,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["integration_key"] = integrationKey,
                ["name"] = name,
                ["data"] = data,
            };

            return _client.RequestAsync<IntegrationConnection>(HttpMethod.Post, "/integrations/connections", body, cancellationToken: cancellationToken);
        }

        /// <summary>Update a connection. Only the fields you pass are changed.</summary>
        public Task<IntegrationConnection> UpdateAsync(
            string connectionUuid,
            string? name = null,
            object? data = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (name is not null)
            {
                body["name"] = name;
            }

            if (data is not null)
            {
                body["data"] = data;
            }

            if (isActive is not null)
            {
                body["is_active"] = isActive;
            }

            return _client.RequestAsync<IntegrationConnection>(Patch, "/integrations/connections/" + Uri.EscapeDataString(connectionUuid), body, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a connection. Automations pointing at it stop firing.
        /// </summary>
        public Task<JsonElement> DeleteAsync(string connectionUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/integrations/connections/" + Uri.EscapeDataString(connectionUuid), cancellationToken: cancellationToken);

        /// <summary>Run one of the integration's actions through this connection.</summary>
        public Task<IntegrationResult> RunAsync(
            string connectionUuid,
            string action,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["action"] = action,
                ["params"] = parameters ?? new Dictionary<string, object?>(),
            };

            return _client.RequestAsync<IntegrationResult>(HttpMethod.Post, "/integrations/connections/" + Uri.EscapeDataString(connectionUuid) + "/run", body, cancellationToken: cancellationToken);
        }
    }
}
