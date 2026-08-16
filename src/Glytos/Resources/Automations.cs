using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>
    /// Automations: "when this happens, do that". A webhook event fires an
    /// integration action, with no server of your own.
    /// </summary>
    /// <remarks>
    /// They run in the background job that already handles the event, never during
    /// a call, and a failure is recorded rather than allowed to affect the
    /// conversation.
    /// </remarks>
    public sealed class Automations
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal Automations(GlytosClient client) => _client = client;

        /// <summary>List your automations.</summary>
        public Task<IReadOnlyList<Automation>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Automation>>(HttpMethod.Get, "/automations", cancellationToken: cancellationToken);

        /// <summary>
        /// Create a rule. <paramref name="triggerEvent"/> is a webhook event type,
        /// and <paramref name="payloadTemplate"/> values may reference the event
        /// with <c>{{placeholders}}</c>.
        /// </summary>
        public Task<Automation> CreateAsync(
            string name,
            string triggerEvent,
            string connectionUuid,
            string action,
            object? payloadTemplate = null,
            object? conditions = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["trigger_event"] = triggerEvent,
                ["connection_uuid"] = connectionUuid,
                ["action"] = action,
            };
            if (payloadTemplate is not null)
            {
                body["payload_template"] = payloadTemplate;
            }

            if (conditions is not null)
            {
                body["conditions"] = conditions;
            }

            return _client.RequestAsync<Automation>(HttpMethod.Post, "/automations", body, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update an automation, including pausing it with
        /// <paramref name="isActive"/> false. Only the fields you pass are changed.
        /// </summary>
        public Task<Automation> UpdateAsync(
            string automationUuid,
            string? name = null,
            string? triggerEvent = null,
            string? connectionUuid = null,
            string? action = null,
            object? payloadTemplate = null,
            object? conditions = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (name is not null)
            {
                body["name"] = name;
            }

            if (triggerEvent is not null)
            {
                body["trigger_event"] = triggerEvent;
            }

            if (connectionUuid is not null)
            {
                body["connection_uuid"] = connectionUuid;
            }

            if (action is not null)
            {
                body["action"] = action;
            }

            if (payloadTemplate is not null)
            {
                body["payload_template"] = payloadTemplate;
            }

            if (conditions is not null)
            {
                body["conditions"] = conditions;
            }

            if (isActive is not null)
            {
                body["is_active"] = isActive;
            }

            return _client.RequestAsync<Automation>(Patch, "/automations/" + Uri.EscapeDataString(automationUuid), body, cancellationToken: cancellationToken);
        }

        /// <summary>Delete an automation.</summary>
        public Task<JsonElement> DeleteAsync(string automationUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/automations/" + Uri.EscapeDataString(automationUuid), cancellationToken: cancellationToken);

        /// <summary>
        /// Recent firings, newest first: what ran, and what came back.
        /// </summary>
        public Task<IReadOnlyList<AutomationRun>> RunsAsync(
            string automationUuid,
            int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>();
            if (limit is not null)
            {
                query["limit"] = limit;
            }

            return _client.RequestAsync<IReadOnlyList<AutomationRun>>(HttpMethod.Get, "/automations/" + Uri.EscapeDataString(automationUuid) + "/runs", query: query, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Fire it once against a payload you supply, so the rendered parameters
        /// and the destination's reply can be checked before a real event is
        /// trusted to it.
        /// </summary>
        public Task<AutomationTest> TestAsync(
            string automationUuid,
            object? payload = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["payload"] = payload ?? new Dictionary<string, object?>(),
            };

            return _client.RequestAsync<AutomationTest>(HttpMethod.Post, "/automations/" + Uri.EscapeDataString(automationUuid) + "/test", body, cancellationToken: cancellationToken);
        }
    }
}
