using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Glytos.Resources;

namespace Glytos
{
    /// <summary>
    /// The Glytos API client.
    /// </summary>
    /// <remarks>
    /// Authenticate with your organization API key (it starts with <c>gly_</c>):
    /// <code>
    /// using var glytos = new GlytosClient("gly_...");
    /// var agents = await glytos.Workflows.ListAsync();
    /// </code>
    /// Never expose an API key in the browser; for in-browser voice, mint a short-lived
    /// token with <see cref="Resources.Calls.WebTokenAsync"/> instead.
    /// </remarks>
    public sealed class GlytosClient : IDisposable
    {
        /// <summary>The default public API base URL.</summary>
        public const string DefaultBaseUrl = "https://api.glytos.com/api/v1";

        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string? _environment;
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>Agents: prompt agents and visual workflows.</summary>
        public Workflows Workflows { get; }

        /// <summary>Calls: start and manage phone and web calls.</summary>
        public Calls Calls { get; }

        /// <summary>Phone numbers: search, provision, and assign numbers.</summary>
        public PhoneNumbers PhoneNumbers { get; }

        /// <summary>Sessions: conversation runs across your agents.</summary>
        public Sessions Sessions { get; }

        /// <summary>Webhooks: manage endpoints and verify delivery signatures.</summary>
        public Webhooks Webhooks { get; }

        /// <summary>Campaigns: run outbound calling campaigns over your agents.</summary>
        public Campaigns Campaigns { get; }

        /// <summary>Chat: mint hosted-chat tokens and post messages to text agents.</summary>
        public Chat Chat { get; }

        /// <summary>Tools: manage the tools your agents can call.</summary>
        public Tools Tools { get; }

        /// <summary>Knowledge base: manage documents and run hybrid retrieval.</summary>
        public KnowledgeBase KnowledgeBase { get; }

        /// <summary>Vector stores: group knowledge-base documents for retrieval.</summary>
        public VectorStores VectorStores { get; }

        /// <summary>Analytics: aggregated usage and performance metrics.</summary>
        public Analytics Analytics { get; }

        /// <summary>Creates a client with the given API key and default options.</summary>
        public GlytosClient(string apiKey)
            : this(apiKey, null)
        {
        }

        /// <summary>Creates a client with the given API key and options.</summary>
        public GlytosClient(string apiKey, GlytosClientOptions? options)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("Glytos: an API key is required.", nameof(apiKey));
            }

            options ??= new GlytosClientOptions();
            _apiKey = apiKey;
            _baseUrl = options.BaseUrl.TrimEnd('/');
            _environment = options.Environment;

            if (options.HttpClient is not null)
            {
                _httpClient = options.HttpClient;
                _ownsHttpClient = false;
            }
            else
            {
                _httpClient = new HttpClient();
                _ownsHttpClient = true;
                if (options.Timeout is { } timeout)
                {
                    _httpClient.Timeout = timeout;
                }
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                // Relaxed encoder so request bodies match the other SDKs on the wire (e.g. a
                // phone number "+1555..." is sent literally, not escaped to "+1555...").
                // Safe for JSON API payloads (this output is never embedded in HTML).
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            Workflows = new Workflows(this);
            Calls = new Calls(this);
            PhoneNumbers = new PhoneNumbers(this);
            Sessions = new Sessions(this);
            Webhooks = new Webhooks(this);
            Campaigns = new Campaigns(this);
            Chat = new Chat(this);
            Tools = new Tools(this);
            KnowledgeBase = new KnowledgeBase(this);
            VectorStores = new VectorStores(this);
            Analytics = new Analytics(this);
        }

        /// <summary>
        /// Low-level request against any endpoint. <paramref name="path"/> is relative to the
        /// API base (e.g. <c>"/workflows"</c>). Deserializes the JSON body into
        /// <typeparamref name="T"/>; use <see cref="JsonElement"/> for an untyped body. Throws
        /// <see cref="GlytosException"/> on a non-2xx response or a transport failure.
        /// </summary>
        public Task<T> RequestAsync<T>(
            string method,
            string path,
            object? body = null,
            IDictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default)
            => RequestAsync<T>(new HttpMethod(method), path, body, query, cancellationToken);

        /// <inheritdoc cref="RequestAsync{T}(string,string,object,IDictionary{string,object},CancellationToken)" />
        public async Task<T> RequestAsync<T>(
            HttpMethod method,
            string path,
            object? body = null,
            IDictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(method, BuildUri(path, query));
            request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            if (_environment is not null)
            {
                request.Headers.TryAddWithoutValidation("X-Environment-Id", _environment);
            }

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                throw new GlytosException(0, "network_error", exception.Message, null, exception);
            }

            using (response)
            {
                var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var requestId = response.Headers.TryGetValues("X-Request-Id", out var values)
                    ? FirstOrNull(values)
                    : null;

                if (!response.IsSuccessStatusCode)
                {
                    var (code, message) = ParseError(text, response.ReasonPhrase);
                    throw new GlytosException((int)response.StatusCode, code, message, requestId);
                }

                if (string.IsNullOrEmpty(text))
                {
                    return default!;
                }

                return JsonSerializer.Deserialize<T>(text, _jsonOptions)!;
            }
        }

        private string BuildUri(string path, IDictionary<string, object?>? query)
        {
            var uri = _baseUrl + path;
            if (query is null)
            {
                return uri;
            }

            var pairs = new List<string>();
            foreach (var pair in query)
            {
                if (pair.Value is null)
                {
                    continue;
                }

                pairs.Add(Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(Format(pair.Value)));
            }

            return pairs.Count == 0 ? uri : uri + "?" + string.Join("&", pairs);
        }

        private static string Format(object value) => value switch
        {
            bool b => b ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

        private static (string Code, string Message) ParseError(string text, string? reason)
        {
            var code = "error";
            var message = string.IsNullOrEmpty(reason) ? "Request failed" : reason!;
            if (string.IsNullOrEmpty(text))
            {
                return (code, message);
            }

            try
            {
                using var document = JsonDocument.Parse(text);
                if (document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("error", out var error)
                    && error.ValueKind == JsonValueKind.Object)
                {
                    if (error.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String)
                    {
                        code = c.GetString() ?? code;
                    }

                    if (error.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    {
                        message = m.GetString() ?? message;
                    }
                }
            }
            catch (JsonException)
            {
                // Non-JSON error body; keep the reason phrase.
            }

            return (code, message);
        }

        private static string? FirstOrNull(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                return value;
            }

            return null;
        }

        /// <summary>Disposes the underlying <see cref="HttpClient"/> when this client created it.</summary>
        public void Dispose()
        {
            if (_ownsHttpClient)
            {
                _httpClient.Dispose();
            }
        }
    }
}
