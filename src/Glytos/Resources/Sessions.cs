using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Sessions: conversation runs across your agents.</summary>
    public sealed class Sessions
    {
        private readonly GlytosClient _client;

        internal Sessions(GlytosClient client) => _client = client;

        /// <summary>List sessions across your agents.</summary>
        public Task<IReadOnlyList<Session>> ListAsync(
            IDictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Session>>(HttpMethod.Get, "/sessions", query: query, cancellationToken: cancellationToken);
    }
}
