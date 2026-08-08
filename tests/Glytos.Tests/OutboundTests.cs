using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Glytos;
using Xunit;

namespace Glytos.Tests
{
    /// <summary>Campaigns and the do-not-call list.</summary>
    public class OutboundTests
    {
        [Fact]
        public async Task ContactsAreSentAsPlainNumbers()
        {
            // The API takes a list of strings. Sending objects is rejected with a 422,
            // so the shape of this field is worth a test of its own.
            var handler = new StubHandler(HttpStatusCode.Created, "{\"uuid\":\"cmp_1\",\"name\":\"Promo\"}");
            using var client = NewClient(handler);

            await client.Campaigns.CreateAsync(
                "Promo",
                "wf_1",
                "+15551230000",
                contacts: new[] { "+15551230001", "0532 123 45 67" });

            Assert.Contains("\"contacts\":[\"+15551230001\",\"0532 123 45 67\"]", handler.LastBody);
        }

        [Fact]
        public async Task CreateCarriesTheScheduleAndTheCallingWindow()
        {
            var handler = new StubHandler(HttpStatusCode.Created, "{\"uuid\":\"cmp_1\",\"name\":\"Promo\"}");
            using var client = NewClient(handler);

            await client.Campaigns.CreateAsync(
                "Promo",
                "wf_1",
                "+15551230000",
                contactsCsv: "phone,name\n+15551230001,Ada\n",
                scheduledAt: new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero),
                callWindowStart: "09:00",
                callWindowEnd: "20:00",
                timezone: "Europe/Istanbul",
                suppressionPolicy: "ignore",
                overrideCallerRequests: true);

            // A DateTimeOffset is serialized for the caller: passing one and getting a
            // 422 back is a poor trade for the one line it saves.
            Assert.Contains("\"scheduled_at\":\"2026-03-01T09:00:00.0000000+00:00\"", handler.LastBody);
            Assert.Contains("\"call_window_start\":\"09:00\"", handler.LastBody);
            Assert.Contains("\"timezone\":\"Europe/Istanbul\"", handler.LastBody);
            Assert.Contains("\"suppression_policy\":\"ignore\"", handler.LastBody);
            Assert.Contains("\"override_caller_requests\":true", handler.LastBody);
        }

        [Fact]
        public async Task CreateOmitsEverythingTheCallerLeftAlone()
        {
            var handler = new StubHandler(HttpStatusCode.Created, "{\"uuid\":\"cmp_1\",\"name\":\"Promo\"}");
            using var client = NewClient(handler);

            await client.Campaigns.CreateAsync("Promo", "wf_1", "+15551230000");

            Assert.Equal(
                "{\"name\":\"Promo\",\"workflow_uuid\":\"wf_1\",\"from_number\":\"+15551230000\"}",
                handler.LastBody);
        }

        [Fact]
        public async Task RetrieveDecodesContactOutcomes()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"uuid\":\"cmp_1\",\"name\":\"Promo\",\"status\":\"completed\"," +
                "\"suppression_policy\":\"strict\",\"contacts\":[" +
                "{\"phone\":\"+15551230001\",\"status\":\"answered\",\"session_uuid\":\"s1\"," +
                "\"variables\":{\"name\":\"Ada\"}}," +
                "{\"phone\":\"+15551230002\",\"status\":\"suppressed\"}]}");
            using var client = NewClient(handler);

            var campaign = await client.Campaigns.RetrieveAsync("cmp_1");

            Assert.Equal("strict", campaign.SuppressionPolicy);
            Assert.Equal(2, campaign.Contacts.Count);
            Assert.Equal("s1", campaign.Contacts[0].SessionUuid);
            Assert.Equal("Ada", campaign.Contacts[0].Variables!["name"]);
            Assert.Equal("suppressed", campaign.Contacts[1].Status);
        }

        [Fact]
        public async Task StopAndDeleteAddressTheCampaign()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"cmp_1\",\"status\":\"stopped\"}");
            using var client = NewClient(handler);

            var campaign = await client.Campaigns.StopAsync("cmp_1");
            Assert.Equal("stopped", campaign.Status);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.EndsWith("/telephony/campaigns/cmp_1/stop", handler.LastRequest.RequestUri!.AbsolutePath);

            await client.Campaigns.DeleteAsync("cmp_1");
            Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
            Assert.EndsWith("/telephony/campaigns/cmp_1", handler.LastRequest.RequestUri!.AbsolutePath);
        }

        [Fact]
        public async Task AddContactsPostsCsvTextAndNoSourceUrl()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"added\":2,\"skipped\":1,\"rejected\":0,\"phone_column\":\"telefon\"}");
            using var client = NewClient(handler);

            var result = await client.Campaigns.AddContactsAsync("cmp_1", "telefon;isim\n+905321234567;Ada\n");

            Assert.EndsWith("/telephony/campaigns/cmp_1/contacts/sync", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.DoesNotContain("source_url", handler.LastBody);
            // The column actually read is what separates a file parsed from the wrong
            // column from one that could not be parsed at all.
            Assert.Equal("telefon", result.PhoneColumn);
            Assert.Equal(2, result.Added);
        }

        [Fact]
        public async Task PreviewSuppressionReportsCallerRequests()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"contacts\":100,\"suppressed_total\":12,\"caller_requested\":4," +
                "\"reached_if_strict\":88,\"reached_if_transactional\":92," +
                "\"reached_if_ignore\":96,\"reached_if_override\":100}");
            using var client = NewClient(handler);

            var preview = await client.Campaigns.PreviewSuppressionAsync(new[] { "+15551230001" });

            Assert.EndsWith("/telephony/campaigns/suppression-preview", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Equal(4, preview.CallerRequested);
            Assert.Equal(88, preview.ReachedIfStrict);
        }

        [Fact]
        public async Task DncListPassesSearchAndPaging()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"items\":[],\"total\":0}");
            using var client = NewClient(handler);

            await client.Dnc.ListAsync("0555", limit: 50, offset: 100);

            Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
            Assert.EndsWith("/dnc", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("?search=0555&limit=50&offset=100", handler.LastRequest.RequestUri.Query);
        }

        [Fact]
        public async Task DncListOmitsUnsetFilters()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"items\":[],\"total\":0}");
            using var client = NewClient(handler);

            await client.Dnc.ListAsync();

            Assert.Equal(string.Empty, handler.LastRequest!.RequestUri!.Query);
        }

        [Fact]
        public async Task DncEntriesDecodeTheirSourceAndScope()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"items\":[{\"uuid\":\"d1\",\"phone\":\"+15551230001\"," +
                "\"source\":\"agent\",\"scope\":\"all\"}],\"total\":1}");
            using var client = NewClient(handler);

            var list = await client.Dnc.ListAsync();

            Assert.Equal(1, list.Total);
            Assert.Equal("agent", list.Items[0].Source);
            Assert.Equal("all", list.Items[0].Scope);
        }

        [Fact]
        public async Task DncScopeAndRemovalCarryThePhoneNumber()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"uuid\":\"d1\",\"phone\":\"+15551230001\",\"source\":\"manual\",\"scope\":\"marketing\"}");
            using var client = NewClient(handler);

            var entry = await client.Dnc.SetScopeAsync("+15551230001", "marketing");

            Assert.Equal("PATCH", handler.LastRequest!.Method.Method);
            // A phone number is a path parameter, so "+" is escaped rather than read as
            // a space by anything in the way.
            Assert.EndsWith("/dnc/%2B15551230001", handler.LastRequest.RequestUri!.AbsoluteUri);
            Assert.Equal("{\"scope\":\"marketing\"}", handler.LastBody);
            Assert.Equal("marketing", entry.Scope);

            await client.Dnc.RemoveAsync("+15551230001");
            Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
            Assert.EndsWith("/dnc/%2B15551230001", handler.LastRequest.RequestUri!.AbsoluteUri);
        }

        [Fact]
        public async Task DncOmitsAnUnstatedReasonRatherThanSendingNull()
        {
            // The reason is a plain string server-side, not a nullable one, so a
            // null is a 422 rather than "no reason given".
            var handler = new StubHandler(HttpStatusCode.Created, "{\"uuid\":\"d1\"}");
            using var client = NewClient(handler);

            await client.Dnc.AddAsync("+15551230001");
            Assert.Equal("{\"phone\":\"+15551230001\"}", handler.LastBody);

            await client.Dnc.ImportAsync(new[] { "+15551230001" });
            Assert.Equal("{\"phones\":[\"+15551230001\"]}", handler.LastBody);
        }

        [Fact]
        public async Task DncImportReportsWhatItDid()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"added\":8,\"duplicates\":1,\"rejected\":2}");
            using var client = NewClient(handler);

            var result = await client.Dnc.ImportAsync(new[] { "+15551230001", "not a number" }, "CRM export");

            Assert.EndsWith("/dnc/import", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Contains("\"reason\":\"CRM export\"", handler.LastBody);
            Assert.Equal(8, result.Added);
            Assert.Equal(2, result.Rejected);
        }

        private static GlytosClient NewClient(HttpMessageHandler handler) =>
            new GlytosClient("gly_test", new GlytosClientOptions
            {
                HttpClient = new HttpClient(handler),
            });

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _body;

            public HttpRequestMessage? LastRequest { get; private set; }

            public string? LastBody { get; private set; }

            public StubHandler(HttpStatusCode status, string body)
            {
                _status = status;
                _body = body;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                LastBody = request.Content is null
                    ? null
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false);

                return new HttpResponseMessage(_status) { Content = new StringContent(_body) };
            }
        }
    }
}
