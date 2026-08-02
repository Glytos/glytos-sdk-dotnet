using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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

        /// <summary>The same resource as <c>Workflows</c>, under the word the product uses.</summary>
        public Workflows Agents { get; }

        /// <summary>Threads: conversations with a text agent, and the runs on them.</summary>
        public Threads Threads { get; }

        /// <summary>Folders: group agents inside an environment.</summary>
        public Folders Folders { get; }

        /// <summary>Imports: bring an agent over from another platform.</summary>
        public Imports Imports { get; }

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
            Agents = Workflows;
            Threads = new Threads(this);
            Folders = new Folders(this);
            Imports = new Imports(this);
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

            return await SendAndReadAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<T> SendAndReadAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
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

        /// <summary>
        /// Stream a Server-Sent Events endpoint, yielding one parsed event at a time.
        /// </summary>
        /// <remarks>
        /// The reply arrives as it is written rather than after the last token, which is
        /// the whole difference on a long answer. The terminal <c>done</c> event carries
        /// the same payload the non-streamed call returns.
        /// </remarks>
        public IAsyncEnumerable<StreamEvent> StreamAsync(
            string method,
            string path,
            object? body = null,
            CancellationToken cancellationToken = default)
            => StreamAsync(new HttpMethod(method), path, body, cancellationToken);

        /// <inheritdoc cref="StreamAsync(string,string,object,CancellationToken)" />
        public async IAsyncEnumerable<StreamEvent> StreamAsync(
            HttpMethod method,
            string path,
            object? body = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(method, BuildUri(path, null));
            request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);
            request.Headers.TryAddWithoutValidation("Accept", "text/event-stream");
            if (_environment is not null)
            {
                request.Headers.TryAddWithoutValidation("X-Environment-Id", _environment);
            }

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await SendStreamAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var (code, message) = ParseError(text, response.ReasonPhrase);
                var requestId = response.Headers.TryGetValues("X-Request-Id", out var values)
                    ? FirstOrNull(values)
                    : null;
                throw new GlytosException((int)response.StatusCode, code, message, requestId);
            }

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var name = string.Empty;
            var data = new StringBuilder();
            while (true)
            {
                string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                // Events are separated by a blank line.
                if (line.Length == 0)
                {
                    var complete = BuildStreamEvent(name, data.ToString());
                    name = string.Empty;
                    data.Clear();
                    if (complete is not null)
                    {
                        yield return complete;
                    }

                    continue;
                }

                if (line.StartsWith("event:", StringComparison.Ordinal))
                {
                    name = line.Substring(6).Trim();
                }
                else if (line.StartsWith("data:", StringComparison.Ordinal))
                {
                    if (data.Length > 0)
                    {
                        data.Append('\n');
                    }

                    data.Append(line.Substring(5).Trim());
                }
            }

            // A stream that ends without a trailing blank line still has one event to give.
            var last = BuildStreamEvent(name, data.ToString());
            if (last is not null)
            {
                yield return last;
            }
        }

        /// <summary>
        /// Upload a file. Separate from <see cref="RequestAsync{T}(string,string,object,IDictionary{string,object},CancellationToken)"/>
        /// because the body is multipart, so the Content-Type has to carry the boundary.
        /// </summary>
        public async Task<T> UploadAsync<T>(
            string path,
            IDictionary<string, string> fields,
            string filename,
            byte[] content,
            CancellationToken cancellationToken = default)
        {
            var form = new MultipartFormDataContent();
            foreach (var field in fields)
            {
                var part = new StringContent(field.Value, Encoding.UTF8);
                // Quoted explicitly: .NET writes `name=x` bare, and RFC 7578 wants `name="x"`.
                part.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = Quote(field.Key),
                };
                form.Add(part);
            }

            var file = new ByteArrayContent(content);
            file.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
            file.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = Quote("file"),
                FileName = Quote(filename),
            };
            form.Add(file);

            using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path, null)) { Content = form };
            request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            if (_environment is not null)
            {
                request.Headers.TryAddWithoutValidation("X-Environment-Id", _environment);
            }

            return await SendAndReadAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }

        private static string Quote(string value) => "\"" + value.Replace("\"", string.Empty) + "\"";

        private async Task<HttpResponseMessage> SendStreamAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                throw new GlytosException(0, "network_error", exception.Message, null, exception);
            }
        }

        private static StreamEvent? BuildStreamEvent(string name, string data)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(data))
            {
                return null;
            }

            JsonElement parsed = default;
            try
            {
                using var document = JsonDocument.Parse(data);
                parsed = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                // Non-JSON payload; the event still carries its type.
            }

            switch (name)
            {
                case "token":
                    return new StreamEvent("token", ReadString(parsed, "delta", string.Empty), string.Empty, default);
                case "error":
                    return new StreamEvent("error", string.Empty, ReadString(parsed, "message", "stream failed"), default);
                case "done":
                    return new StreamEvent("done", string.Empty, string.Empty, parsed);
                default:
                    return null;
            }
        }

        private static string ReadString(JsonElement element, string property, string fallback)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var value))
            {
                return fallback;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? fallback;
            }

            return value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                ? fallback
                : value.ToString();
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
