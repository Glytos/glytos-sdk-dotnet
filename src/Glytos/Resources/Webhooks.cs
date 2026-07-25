using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Webhooks: manage endpoints and verify delivery signatures.</summary>
    public sealed class Webhooks
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal Webhooks(GlytosClient client) => _client = client;

        /// <summary>List your webhook endpoints.</summary>
        public Task<IReadOnlyList<WebhookEndpoint>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<WebhookEndpoint>>(HttpMethod.Get, "/webhooks/endpoints", cancellationToken: cancellationToken);

        /// <summary>Create a webhook endpoint subscribed to the given events.</summary>
        public Task<WebhookEndpoint> CreateAsync(
            string url,
            IReadOnlyList<string> events,
            IDictionary<string, object?>? options = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["url"] = url, ["events"] = events };
            if (options is not null)
            {
                foreach (var pair in options)
                {
                    body[pair.Key] = pair.Value;
                }
            }

            return _client.RequestAsync<WebhookEndpoint>(HttpMethod.Post, "/webhooks/endpoints", body, cancellationToken: cancellationToken);
        }

        /// <summary>Update a webhook endpoint. Only the fields you pass are changed.</summary>
        public Task<WebhookEndpoint> UpdateAsync(
            long endpointId,
            string? url = null,
            IReadOnlyList<string>? events = null,
            bool? isActive = null,
            int? timeoutSeconds = null,
            object? headers = null,
            string? authHeader = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (url is not null)
            {
                body["url"] = url;
            }

            if (events is not null)
            {
                body["events"] = events;
            }

            if (isActive is not null)
            {
                body["is_active"] = isActive;
            }

            if (timeoutSeconds is not null)
            {
                body["timeout_seconds"] = timeoutSeconds;
            }

            if (headers is not null)
            {
                body["headers"] = headers;
            }

            if (authHeader is not null)
            {
                body["auth_header"] = authHeader;
            }

            return _client.RequestAsync<WebhookEndpoint>(Patch, "/webhooks/endpoints/" + endpointId.ToString(CultureInfo.InvariantCulture), body, cancellationToken: cancellationToken);
        }

        /// <summary>Delete a webhook endpoint.</summary>
        public Task<JsonElement> DeleteAsync(long endpointId, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/webhooks/endpoints/" + endpointId.ToString(CultureInfo.InvariantCulture), cancellationToken: cancellationToken);

        /// <summary>List recent webhook delivery attempts, optionally filtered.</summary>
        public Task<IReadOnlyList<WebhookDelivery>> DeliveriesAsync(
            string? eventType = null,
            string? status = null,
            int? limit = null,
            int? offset = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>();
            if (eventType is not null)
            {
                query["event_type"] = eventType;
            }

            if (status is not null)
            {
                query["status"] = status;
            }

            if (limit is not null)
            {
                query["limit"] = limit;
            }

            if (offset is not null)
            {
                query["offset"] = offset;
            }

            return _client.RequestAsync<IReadOnlyList<WebhookDelivery>>(HttpMethod.Get, "/webhooks/deliveries", query: query, cancellationToken: cancellationToken);
        }

        /// <summary>Re-attempt a previously failed webhook delivery.</summary>
        public Task<JsonElement> RedeliverAsync(long deliveryId, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Post, "/webhooks/deliveries/" + deliveryId.ToString(CultureInfo.InvariantCulture) + "/redeliver", cancellationToken: cancellationToken);

        /// <summary>The catalog of webhook event types you can subscribe to.</summary>
        public Task<JsonElement> EventsAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Get, "/webhooks/events", cancellationToken: cancellationToken);

        /// <summary>
        /// Verify a webhook delivery signature. Delegates to <see cref="Webhook.Verify"/>; pass
        /// the RAW request body, the <c>X-Glytos-Signature</c> header, and the endpoint secret.
        /// </summary>
        public bool Verify(string payload, string signatureHeader, string secret, int toleranceSeconds = 300) =>
            Webhook.Verify(payload, signatureHeader, secret, toleranceSeconds);
    }
}
