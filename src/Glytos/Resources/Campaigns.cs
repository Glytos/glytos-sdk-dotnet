using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Glytos.Resources
{
    /// <summary>Campaigns: run outbound calling campaigns over your agents.</summary>
    public sealed class Campaigns
    {
        // netstandard2.0 has no Patch, so the verb is spelled out. The
        // other resources that patch do the same.
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly GlytosClient _client;

        internal Campaigns(GlytosClient client) => _client = client;

        /// <summary>List your calling campaigns.</summary>
        public Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken = default) =>
            _client.RequestAsync<IReadOnlyList<Campaign>>(HttpMethod.Get, "/telephony/campaigns", cancellationToken: cancellationToken);

        /// <summary>Create a campaign that dials a contact list with one agent.</summary>
        /// <param name="name">Display name for the campaign.</param>
        /// <param name="workflowUuid">The agent to run for each contact.</param>
        /// <param name="fromNumber">
        /// The caller id to dial from. It must be a number your organization has
        /// connected, or the campaign is refused.
        /// </param>
        /// <param name="contacts">
        /// Phone numbers in any spelling; they are converted to international form
        /// and deduplicated.
        /// </param>
        /// <param name="contactsCsv">
        /// The contents of a CSV file. The phone column is found by its header or by
        /// which column holds phone numbers, and every other column travels with that
        /// contact's call as a variable, so <c>{{name}}</c> in the agent's prompt means
        /// the person being called.
        /// </param>
        /// <param name="scheduledAt">
        /// Start dialing at a moment in the future. Left unset, the campaign is a draft
        /// until <see cref="StartAsync"/>.
        /// </param>
        /// <param name="callWindowStart">Start of the dialing hours, e.g. <c>"09:00"</c>.</param>
        /// <param name="callWindowEnd">End of the dialing hours, e.g. <c>"20:00"</c>.</param>
        /// <param name="timezone">An IANA name, e.g. <c>"Europe/Istanbul"</c>. Defaults to UTC.</param>
        /// <param name="suppressionPolicy">
        /// How much of the do-not-call list this campaign honours: <c>strict</c>
        /// (default, all of it), <c>transactional</c> (skip entries that only refused
        /// marketing), or <c>ignore</c> (skip entries the organization added for itself;
        /// requests people made on a call still apply).
        /// </param>
        /// <param name="overrideCallerRequests">
        /// Also call people who asked, on a call, not to be contacted again. Only valid
        /// with a <paramref name="suppressionPolicy"/> of <c>ignore</c>.
        /// </param>
        /// <param name="cancellationToken">Cancels the request.</param>
        public Task<Campaign> CreateAsync(
            string name,
            string workflowUuid,
            string fromNumber,
            IEnumerable<string>? contacts = null,
            string? contactsCsv = null,
            DateTimeOffset? scheduledAt = null,
            string? callWindowStart = null,
            string? callWindowEnd = null,
            string? timezone = null,
            string? suppressionPolicy = null,
            bool? overrideCallerRequests = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["workflow_uuid"] = workflowUuid,
                ["from_number"] = fromNumber,
            };
            if (contacts is not null)
            {
                body["contacts"] = new List<string>(contacts);
            }

            if (contactsCsv is not null)
            {
                body["contacts_csv"] = contactsCsv;
            }

            if (scheduledAt is not null)
            {
                body["scheduled_at"] = scheduledAt.Value.ToString("o");
            }

            if (callWindowStart is not null)
            {
                body["call_window_start"] = callWindowStart;
            }

            if (callWindowEnd is not null)
            {
                body["call_window_end"] = callWindowEnd;
            }

            if (timezone is not null)
            {
                body["timezone"] = timezone;
            }

            if (suppressionPolicy is not null)
            {
                body["suppression_policy"] = suppressionPolicy;
            }

            if (overrideCallerRequests is not null)
            {
                body["override_caller_requests"] = overrideCallerRequests;
            }

            return _client.RequestAsync<Campaign>(HttpMethod.Post, "/telephony/campaigns", body, cancellationToken: cancellationToken);
        }

        /// <summary>A campaign with its contacts and their outcomes.</summary>
        public Task<CampaignDetail> RetrieveAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<CampaignDetail>(HttpMethod.Get, "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid), cancellationToken: cancellationToken);

        /// <summary>Begin dialing, from the contacts that have not been called yet.</summary>
        public Task<Campaign> StartAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Campaign>(HttpMethod.Post, "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid) + "/start", cancellationToken: cancellationToken);

        /// <summary>
        /// End dialing at the next contact. Calls already handed to the carrier run to
        /// their end; undialed contacts stay ready, so <see cref="StartAsync"/> resumes.
        /// </summary>
        public Task<Campaign> StopAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Campaign>(HttpMethod.Post, "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid) + "/stop", cancellationToken: cancellationToken);

        /// <summary>
        /// Rename a campaign, or change when and within what hours it dials.
        /// </summary>
        /// <remarks>
        /// A rename is accepted at any point. The schedule and the calling window can
        /// only be changed before the campaign starts: moving the start of one already
        /// dialing would say nothing about the calls it has placed. Anything left null
        /// is left alone; to remove a schedule entirely use <see cref="UnscheduleAsync"/>,
        /// since omitting a field and clearing it are different instructions and only
        /// one of them can be expressed by absence.
        /// </remarks>
        public Task<Campaign> UpdateAsync(
            string campaignUuid,
            string? name = null,
            DateTimeOffset? scheduledAt = null,
            string? callWindowStart = null,
            string? callWindowEnd = null,
            string? timezone = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (name is not null)
            {
                body["name"] = name;
            }

            if (scheduledAt is not null)
            {
                body["scheduled_at"] = scheduledAt.Value.ToString("o");
            }

            if (callWindowStart is not null)
            {
                body["call_window_start"] = callWindowStart;
            }

            if (callWindowEnd is not null)
            {
                body["call_window_end"] = callWindowEnd;
            }

            if (timezone is not null)
            {
                body["timezone"] = timezone;
            }

            return _client.RequestAsync<Campaign>(
                Patch,
                "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid),
                body,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Clear a campaign's schedule, returning it to a draft that waits for
        /// <see cref="StartAsync"/>.
        /// </summary>
        public Task<Campaign> UnscheduleAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<Campaign>(
                Patch,
                "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid),
                new Dictionary<string, object?> { ["scheduled_at"] = null },
                cancellationToken: cancellationToken);

        /// <summary>
        /// Copy a campaign and its contact list into a fresh draft. Nothing dials and no
        /// outcome is copied, so this is how you run the same list again or reuse a setup
        /// against a new one.
        /// </summary>
        public Task<Campaign> DuplicateAsync(string campaignUuid, string? name = null, CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (name is not null)
            {
                body["name"] = name;
            }

            return _client.RequestAsync<Campaign>(
                HttpMethod.Post,
                "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid) + "/duplicate",
                body,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// The contacts and what came of each, as CSV text: phone, outcome, dialed_at,
        /// error, session_uuid. The session uuid joins a result back to the conversation
        /// that produced it.
        /// </summary>
        public Task<string> ExportAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestTextAsync(
                HttpMethod.Get,
                "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid) + "/export",
                cancellationToken);

        /// <summary>Remove a campaign and its contact list, stopping it first if running.</summary>
        public Task<JsonElement> DeleteAsync(string campaignUuid, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<JsonElement>(HttpMethod.Delete, "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid), cancellationToken: cancellationToken);

        /// <summary>Append contacts from the contents of a CSV file.</summary>
        public Task<ContactSyncResult> AddContactsAsync(string campaignUuid, string contactsCsv, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<ContactSyncResult>(
                HttpMethod.Post,
                "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid) + "/contacts/sync",
                new Dictionary<string, object?> { ["contacts_csv"] = contactsCsv },
                cancellationToken: cancellationToken);

        /// <summary>Append contacts from a CSV your own system serves over HTTP.</summary>
        public Task<ContactSyncResult> SyncContactsAsync(string campaignUuid, string sourceUrl, CancellationToken cancellationToken = default) =>
            _client.RequestAsync<ContactSyncResult>(
                HttpMethod.Post,
                "/telephony/campaigns/" + Uri.EscapeDataString(campaignUuid) + "/contacts/sync",
                new Dictionary<string, object?> { ["source_url"] = sourceUrl },
                cancellationToken: cancellationToken);

        /// <summary>
        /// How many of a contact list each suppression policy would reach, including how
        /// many of those people asked on a call not to be contacted again. Measure before
        /// choosing anything other than the default.
        /// </summary>
        public Task<SuppressionPreview> PreviewSuppressionAsync(
            IEnumerable<string>? contacts = null,
            string? contactsCsv = null,
            CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object?>();
            if (contacts is not null)
            {
                body["contacts"] = new List<string>(contacts);
            }

            if (contactsCsv is not null)
            {
                body["contacts_csv"] = contactsCsv;
            }

            return _client.RequestAsync<SuppressionPreview>(
                HttpMethod.Post,
                "/telephony/campaigns/suppression-preview",
                body,
                cancellationToken: cancellationToken);
        }
    }
}
