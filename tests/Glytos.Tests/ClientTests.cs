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
            var handler = new StubHandler(HttpStatusCode.OK, "[]");
            using var client = NewClient(handler);

            await client.Calls.ListAsync(new Dictionary<string, object?> { ["status"] = "completed", ["agent"] = null });

            Assert.Equal("?status=completed", handler.LastRequest!.RequestUri!.Query);
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
