namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net.Http;
    using Glytos;

    /// <summary>
    /// Dependency-injection helpers for registering a <see cref="GlytosClient"/>.
    /// </summary>
    public static class GlytosServiceCollectionExtensions
    {
        private const string HttpClientName = "Glytos";

        /// <summary>
        /// Registers a singleton <see cref="GlytosClient"/> backed by an
        /// <see cref="IHttpClientFactory"/>-managed <see cref="HttpClient"/>. Inject
        /// <see cref="GlytosClient"/> to use it.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="apiKey">Your organization API key (starts with <c>gly_</c>).</param>
        /// <param name="configure">Optional hook to set the environment, base URL, etc.</param>
        public static IServiceCollection AddGlytos(
            this IServiceCollection services,
            string apiKey,
            Action<GlytosClientOptions>? configure = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("Glytos: an API key is required.", nameof(apiKey));
            }

            services.AddHttpClient(HttpClientName);
            services.AddSingleton(serviceProvider =>
            {
                var options = new GlytosClientOptions();
                configure?.Invoke(options);
                options.HttpClient ??= serviceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(HttpClientName);
                return new GlytosClient(apiKey, options);
            });

            return services;
        }
    }
}
