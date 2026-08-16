using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Glytos;
using Xunit;

namespace Glytos.Tests
{
    /// <summary>
    /// The resources added after the first release. Same stub handler as the rest
    /// of the suite: assert the method, path and body, so a wrong path or a
    /// renamed field is caught without a live API.
    /// </summary>
    public class ResourceTests
    {
        [Fact]
        public async Task SipTrunkCreatePostsTheLogin()
        {
            var handler = new StubHandler(HttpStatusCode.Created, "{\"uuid\":\"trunk_1\"}");
            using var client = NewClient(handler);

            await client.SipTrunks.CreateAsync("line-1", "secret", preset: "netgsm");

            Assert.EndsWith("/telephony/sip-trunks", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Contains("\"username\":\"line-1\"", handler.LastBody);
            Assert.Contains("\"preset\":\"netgsm\"", handler.LastBody);
        }

        [Fact]
        public async Task SipTrunkTestReportsReachableSeparatelyFromOk()
        {
            // A carrier that refused the credentials is a different problem from one
            // that never answered, and only the first is worth a new password.
            var handler = new StubHandler(HttpStatusCode.OK, "{\"ok\":false,\"detail\":\"no reply\",\"reachable\":false}");
            using var client = NewClient(handler);

            var result = await client.SipTrunks.TestAsync("trunk_1");

            Assert.EndsWith("/telephony/sip-trunks/trunk_1/test", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.False(result.Ok);
            Assert.False(result.Reachable);
        }

        [Fact]
        public async Task ImportNumberCanNameASipTrunk()
        {
            var handler = new StubHandler(HttpStatusCode.Created, "{\"uuid\":\"num_1\"}");
            using var client = NewClient(handler);

            await client.PhoneNumbers.ImportNumberAsync("+905321234567", sipTrunkUuid: "trunk_1");

            Assert.Equal("{\"e164\":\"+905321234567\",\"sip_trunk_uuid\":\"trunk_1\"}", handler.LastBody);
        }

        [Fact]
        public async Task ConnectionRunAddressesTheConnection()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{}}");
            using var client = NewClient(handler);

            await client.Integrations.Connections.RunAsync(
                "conn_1",
                "post_message",
                new Dictionary<string, object?> { ["text"] = "A lead came in" });

            Assert.EndsWith("/integrations/connections/conn_1/run", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Contains("\"action\":\"post_message\"", handler.LastBody);
            Assert.Contains("\"text\":\"A lead came in\"", handler.LastBody);
        }

        [Fact]
        public async Task ConnectionRunSendsAnObjectForNoParams()
        {
            // Null would encode as null, which the API reads as a missing field
            // rather than "no parameters".
            var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{}}");
            using var client = NewClient(handler);

            await client.Integrations.Connections.RunAsync("conn_1", "ping");

            Assert.Contains("\"params\":{}", handler.LastBody);
        }

        [Fact]
        public async Task AutomationCreateCarriesTheTriggerAndTemplate()
        {
            var handler = new StubHandler(HttpStatusCode.Created, "{\"uuid\":\"auto_1\"}");
            using var client = NewClient(handler);

            await client.Automations.CreateAsync(
                "Tell sales",
                "session.completed",
                "conn_1",
                "post_message",
                payloadTemplate: new Dictionary<string, object?> { ["text"] = "Call from {{from_number}}" });

            Assert.Contains("\"trigger_event\":\"session.completed\"", handler.LastBody);
            Assert.Contains("Call from {{from_number}}", handler.LastBody);
            // Conditions were not given, so they are absent rather than empty.
            Assert.DoesNotContain("\"conditions\"", handler.LastBody);
        }

        [Fact]
        public async Task TestSuiteRunPostsToTheSuite()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"suite_uuid\":\"s1\",\"passed\":false,\"total\":3,\"passed_count\":2,\"results\":[]}");
            using var client = NewClient(handler);

            var result = await client.TestSuites.RunAsync("s1");

            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.EndsWith("/test-suites/s1/run", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal(2, result.PassedCount);
        }

        [Fact]
        public async Task BillingReadsTheBalance()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"balance\":12.5,\"currency\":\"USD\"}");
            using var client = NewClient(handler);

            var balance = await client.Billing.CreditsAsync();

            Assert.EndsWith("/billing/credits", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Equal(12.5m, balance.Balance);
            Assert.Equal("USD", balance.Currency);
        }

        [Fact]
        public async Task BillingTransactionsPassTheirFilters()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "[]");
            using var client = NewClient(handler);

            await client.Billing.TransactionsAsync("debit", 10);

            var query = handler.LastRequest!.RequestUri!.Query;
            Assert.Contains("kind=debit", query);
            Assert.Contains("limit=10", query);
        }

        [Fact]
        public async Task ApiKeyCreateOmitsUnstatedLimits()
        {
            // Omitting both is exactly the behaviour keys have always had, so an SDK
            // that sent nulls would change what an unchanged caller gets.
            var handler = new StubHandler(HttpStatusCode.Created, "{\"id\":1,\"key\":\"gly_x\"}");
            using var client = NewClient(handler);

            await client.ApiKeys.CreateAsync("CI");
            Assert.Equal("{\"name\":\"CI\"}", handler.LastBody);

            await client.ApiKeys.CreateAsync("CI", 90, new[] { "workflow:read" });
            Assert.Contains("\"expires_in_days\":90", handler.LastBody);
            Assert.Contains("\"scopes\":[\"workflow:read\"]", handler.LastBody);
        }

        [Fact]
        public async Task DiscoverMcpReturnsTheToolListNotTheEnvelope()
        {
            var handler = new StubHandler(
                HttpStatusCode.OK,
                "{\"tools\":[{\"name\":\"search\"},{\"name\":\"fetch\"}]}");
            using var client = NewClient(handler);

            var tools = await client.Tools.DiscoverMcpAsync("https://mcp.example.com");

            Assert.EndsWith("/tools/mcp/discover", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Equal(2, tools.Count);
            Assert.Equal("search", tools[0].Name);
        }

        [Fact]
        public async Task KnowledgeBaseDocumentsCanBeReadAndDeleted()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"id\":7,\"name\":\"Refunds\"}");
            using var client = NewClient(handler);

            await client.KnowledgeBase.RetrieveDocumentAsync(7);
            Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
            Assert.EndsWith("/knowledge-base/documents/7", handler.LastRequest.RequestUri!.AbsolutePath);

            await client.KnowledgeBase.DeleteDocumentAsync(7);
            Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        }

        [Fact]
        public async Task ImportsConnectAndPullCarryTheOtherPlatformKey()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"agents\":[]}");
            using var client = NewClient(handler);

            await client.Imports.ConnectAsync("vapi", "vapi_key");
            Assert.EndsWith("/imports/vapi/connect", handler.LastRequest!.RequestUri!.AbsolutePath);
            Assert.Equal("{\"api_key\":\"vapi_key\"}", handler.LastBody);

            await client.Imports.PullAsync("vapi", "vapi_key", new[] { "a1" });
            Assert.EndsWith("/imports/vapi/pull", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Contains("\"agent_ids\":[\"a1\"]", handler.LastBody);
        }

        [Fact]
        public async Task CallControlHelpersSpellOutTheAction()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{}");
            using var client = NewClient(handler);

            await client.Calls.SayAsync("call_1", "One moment");
            Assert.Equal("{\"action\":\"say\",\"text\":\"One moment\"}", handler.LastBody);

            await client.Calls.EndAsync("call_1");
            Assert.Equal("{\"action\":\"end\"}", handler.LastBody);
        }

        [Fact]
        public async Task EnvironmentsAndProvidersRead()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "[]");
            using var client = NewClient(handler);

            await client.Environments.ListAsync();
            Assert.EndsWith("/environments", handler.LastRequest!.RequestUri!.AbsolutePath);

            await client.Providers.ListAsync();
            Assert.EndsWith("/providers", handler.LastRequest.RequestUri!.AbsolutePath);
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
