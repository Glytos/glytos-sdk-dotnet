using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>
    /// Dnc: the numbers your organization must not call.
    /// </summary>
    /// <remarks>
    /// Every outbound call is checked against this list, whether it comes from a
    /// campaign or from <see cref="Calls.CreateAsync"/>. Agents add to it themselves
    /// when a caller asks not to be contacted again.
    /// </remarks>
    public sealed class Dnc
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal Dnc(GlytosClient client) => _client = client;

        /// <summary>Suppressed numbers, newest first.</summary>
        /// <param name="search">
        /// Normalized before matching, so a number typed the way it appears on a contact
        /// list finds the entry stored in international form.
        /// </param>
        /// <param name="limit">How many entries to return.</param>
        /// <param name="offset">How many entries to skip.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        public Task<DncList> ListAsync(
            string? search = null,
            int? limit = null,
            int? offset = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, object?>
            {
                ["search"] = search,
                ["limit"] = limit,
                ["offset"] = offset,
            };

            return _client.RequestAsync<DncList>(HttpMethod.Get, "/dnc", query: query, cancellationToken: cancellationToken);
        }

        /// <summary>Suppress a number.</summary>
        /// <remarks>
        /// Any spelling is accepted and stored in international form. Adding one already
        /// on the list returns the existing entry rather than failing.
        /// </remarks>
        public Task<DncEntry> AddAsync(string phone, string? reason = null, CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["phone"] = phone };
            WithReason(body, reason);

            return _client.RequestAsync<DncEntry>(HttpMethod.Post, "/dnc", body, cancellationToken: cancellationToken);
        }

        /// <summary>Suppress many numbers at once, e.g. a list exported from your CRM.</summary>
        public Task<DncImportResult> ImportAsync(
            IEnumerable<string> phones,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?> { ["phones"] = new List<string>(phones) };
            WithReason(body, reason);

            return _client.RequestAsync<DncImportResult>(HttpMethod.Post, "/dnc/import", body, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Add the reason only when there is one. The server takes a plain string,
        /// not a nullable one, and the client's ignore-nulls setting covers object
        /// properties rather than dictionary entries, so a null would be sent and
        /// rejected.
        /// </summary>
        private static void WithReason(IDictionary<string, object?> body, string? reason)
        {
            if (reason is not null)
            {
                body["reason"] = reason;
            }
        }

        /// <summary>Change how far a suppression reaches.</summary>
        /// <param name="phone">The suppressed number.</param>
        /// <param name="scope">
        /// <c>all</c> covers every call; <c>marketing</c> still allows a transactional
        /// call about the person's own order.
        /// </param>
        /// <param name="cancellationToken">Cancels the request.</param>
        public Task<DncEntry> SetScopeAsync(string phone, string scope, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<DncEntry>(
                Patch,
                "/dnc/" + Uri.EscapeDataString(phone),
                new Dictionary<string, object?> { ["scope"] = scope },
                cancellationToken: cancellationToken);

        /// <summary>Take a number off the list, so it can be called again.</summary>
        public Task<JsonElement> RemoveAsync(string phone, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/dnc/" + Uri.EscapeDataString(phone), cancellationToken: cancellationToken);
    }
}
