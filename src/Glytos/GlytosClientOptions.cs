using System;
using System.Net.Http;

namespace Glytos
{
    /// <summary>
    /// Options for <see cref="GlytosClient"/>. All are optional; the defaults target the
    /// public Glytos API and the organization's default (Development) environment.
    /// </summary>
    public sealed class GlytosClientOptions
    {
        /// <summary>API base URL. Override to target a regional stack.</summary>
        public string BaseUrl { get; set; } = GlytosClient.DefaultBaseUrl;

        /// <summary>
        /// The environment to act in: <c>"dev"</c>, <c>"staging"</c>, <c>"prod"</c>, or an
        /// environment uuid. Scopes reads and calls; new agents are always created in
        /// Development. Defaults to the organization's default environment.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// A pre-configured <see cref="System.Net.Http.HttpClient"/> to send requests with
        /// (e.g. from <c>IHttpClientFactory</c>). When set, the caller owns its lifetime and
        /// <see cref="GlytosClient.Dispose"/> will not dispose it. When null, the client
        /// creates and owns its own.
        /// </summary>
        public HttpClient? HttpClient { get; set; }

        /// <summary>Request timeout. Ignored when an external <see cref="HttpClient"/> is supplied.</summary>
        public TimeSpan? Timeout { get; set; }
    }
}
