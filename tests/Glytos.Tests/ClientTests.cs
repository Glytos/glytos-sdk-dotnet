using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Glytos;
using Xunit;

namespace Glytos.Tests
{
    public class ClientTests
    {
        [Fact]
        public void RequiresAnApiKey()
        {
            Assert.Throws<ArgumentException>(() => new GlytosClient(string.Empty));
        }

        [Fact]
        public async Task SuccessfulRequestDeserializesAndSendsAuthHeaders()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "[{\"uuid\":\"wf_1\",\"name\":\"A\",\"mode\":\"prompt\"}]");
            using var client = NewClient(handler, environment: "prod");

            var agents = await client.Workflows.ListAsync();

            Assert.Single(agents);
            Assert.Equal("wf_1", agents[0].Uuid);
            Assert.NotNull(handler.LastRequest);
            Assert.Equal("gly_test", handler.LastRequest!.Headers.GetValues("X-API-Key").First());
            Assert.Equal("prod", handler.LastRequest.Headers.GetValues("X-Environment-Id").First());
            Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
            Assert.EndsWith("/workflows", handler.LastRequest.RequestUri!.ToString());
        }

        [Fact]
        public async Task CreateSerializesTheJsonBody()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"wf_2\",\"name\":\"A\",\"mode\":\"prompt\"}");
            using var client = NewClient(handler);

            await client.Workflows.CreateAsync("My Agent");

            Assert.NotNull(handler.LastBody);
            Assert.Equal("{\"name\":\"My Agent\",\"mode\":\"prompt\"}", handler.LastBody);
        }

        [Fact]
        public async Task DropsNullQueryParameters()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"items\":[]}");
            using var client = NewClient(handler);

            await client.Calls.ListAsync(new Dictionary<string, object?> { ["status"] = "completed", ["agent"] = null });

            Assert.Equal("?status=completed", handler.LastRequest!.RequestUri!.Query);
        }

        [Fact]
        public async Task PaginatedListUnwrapsItems()
        {
            // /calls wraps results in an {items, total, ...} envelope; the SDK returns the items.
            var handler = new StubHandler(HttpStatusCode.OK, "{\"items\":[{\"uuid\":\"call_1\",\"status\":\"completed\"}],\"total\":1}");
            using var client = NewClient(handler);

            var calls = await client.Calls.ListAsync();

            Assert.Single(calls);
            Assert.Equal("call_1", calls[0].Uuid);
        }

        [Fact]
        public async Task ErrorResponseThrowsGlytosException()
        {
            var handler = new StubHandler(HttpStatusCode.NotFound, "{\"error\":{\"code\":\"not_found\",\"message\":\"Nope\"}}", "req_2");
            using var client = NewClient(handler);

            var exception = await Assert.ThrowsAsync<GlytosException>(() => client.Workflows.RetrieveAsync("missing"));

            Assert.Equal(404, exception.Status);
            Assert.Equal("not_found", exception.ErrorCode);
            Assert.Equal("Nope", exception.Message);
            Assert.Equal("req_2", exception.RequestId);
        }

        [Fact]
        public async Task PromoteSendsTargetEnvironmentId()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"wf_1\",\"name\":\"A\",\"mode\":\"prompt\"}");
            using var client = NewClient(handler);

            await client.Workflows.PromoteAsync("wf_1", "env_9");

            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.EndsWith("/workflows/wf_1/promote", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("{\"target_environment_id\":\"env_9\"}", handler.LastBody);
        }

        [Fact]
        public async Task UpdateConfigUsesPutAndWrapsConfig()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"wf_1\",\"name\":\"A\",\"mode\":\"prompt\"}");
            using var client = NewClient(handler);

            await client.Workflows.UpdateConfigAsync("wf_1", new Dictionary<string, object?> { ["greeting"] = "hi" });

            Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
            Assert.EndsWith("/workflows/wf_1/config", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("{\"config\":{\"greeting\":\"hi\"}}", handler.LastBody);
        }

        [Fact]
        public async Task InstantSendsQueryParametersNotABody()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"pn_1\",\"e164\":\"+15551230000\"}");
            using var client = NewClient(handler);

            await client.PhoneNumbers.InstantAsync(country: "US", provider: "twilio");

            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.EndsWith("/telephony/numbers/instant", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("?country=US&provider=twilio", handler.LastRequest.RequestUri.Query);
            Assert.Null(handler.LastBody);
        }

        [Fact]
        public async Task CampaignsCreateSerializesTheBodyAndOmitsContacts()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"cmp_1\",\"name\":\"Promo\"}");
            using var client = NewClient(handler);

            var campaign = await client.Campaigns.CreateAsync("Promo", "wf_1", "+15551230000");

            Assert.Equal("cmp_1", campaign.Uuid);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.EndsWith("/telephony/campaigns", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("{\"name\":\"Promo\",\"workflow_uuid\":\"wf_1\",\"from_number\":\"+15551230000\"}", handler.LastBody);
        }

        [Fact]
        public async Task ToolsUpdateUsesPatchAndOmitsNullFields()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"tool_1\",\"name\":\"New\",\"kind\":\"http\"}");
            using var client = NewClient(handler);

            await client.Tools.UpdateAsync("tool_1", name: "New");

            Assert.Equal("PATCH", handler.LastRequest!.Method.Method);
            Assert.EndsWith("/tools/tool_1", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("{\"name\":\"New\"}", handler.LastBody);
        }

        [Fact]
        public async Task KnowledgeBaseSearchPostsToSearchEndpoint()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "[{\"document_id\":5,\"score\":0.9,\"content\":\"x\"}]");
            using var client = NewClient(handler);

            var hits = await client.KnowledgeBase.SearchAsync("hello", topK: 3);

            Assert.Single(hits);
            Assert.Equal(5L, hits[0].DocumentId);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.EndsWith("/knowledge-base/search", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("{\"query\":\"hello\",\"top_k\":3}", handler.LastBody);
        }

        [Fact]
        public async Task AnalyticsOverviewSendsDaysQuery()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"total_calls\":10}");
            using var client = NewClient(handler);

            var overview = await client.Analytics.OverviewAsync(7);

            Assert.NotNull(overview.AdditionalData);
            Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
            Assert.EndsWith("/analytics/overview", handler.LastRequest.RequestUri!.AbsolutePath);
            Assert.Equal("?days=7", handler.LastRequest.RequestUri.Query);
            Assert.Null(handler.LastBody);
        }

        private static GlytosClient NewClient(HttpMessageHandler handler, string? environment = null) =>
            new GlytosClient("gly_test", new GlytosClientOptions
            {
                HttpClient = new HttpClient(handler),
                Environment = environment,
            });

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _body;
            private readonly string? _requestId;

            public HttpRequestMessage? LastRequest { get; private set; }

            public string? LastBody { get; private set; }

            public StubHandler(HttpStatusCode status, string body, string? requestId = null)
            {
                _status = status;
                _body = body;
                _requestId = requestId;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                if (request.Content is not null)
                {
                    LastBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                var response = new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body),
                };
                if (_requestId is not null)
                {
                    response.Headers.TryAddWithoutValidation("X-Request-Id", _requestId);
                }

                return response;
            }
        }
    }
}
