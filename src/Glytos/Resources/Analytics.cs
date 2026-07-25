using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Analytics: aggregated usage and performance metrics.</summary>
    public sealed class Analytics
    {
        private readonly GlytosClient _client;

        internal Analytics(GlytosClient client) => _client = client;

        /// <summary>
        /// Usage and performance overview for the last <paramref name="days"/> days
        /// (1-90, default 14 on the server when omitted).
        /// </summary>
        public Task<AnalyticsOverview> OverviewAsync(int? days = null, CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>();
            if (days is not null)
            {
                query["days"] = days;
            }

            return _client.RequestAsync<AnalyticsOverview>(HttpMethod.Get, "/analytics/overview", query: query, cancellationToken: cancellationToken);
        }
    }
}
