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

        /// <summary>Delete a webhook endpoint.</summary>
        public Task<JsonElement> DeleteAsync(long endpointId, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/webhooks/endpoints/" + endpointId.ToString(CultureInfo.InvariantCulture), cancellationToken: cancellationToken);

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
