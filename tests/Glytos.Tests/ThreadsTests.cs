using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Glytos;
using Xunit;

namespace Glytos.Tests
{
    // Threads, streaming, per-turn instructions and uploads. Mirrors the node, python,
    // go and php SDK tests so the surfaces cannot drift apart.
    public class ThreadsTests
    {
        [Fact]
        public async Task ThreadCreateCarriesTheAgentId()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"session_uuid\":\"ses_1\",\"status\":\"in_progress\"}");
            using var client = NewClient(handler);

            var thread = await client.Threads.CreateAsync(
                "wf_1",
                new Dictionary<string, object?> { ["name"] = "Ada" });

            Assert.NotNull(handler.LastRequest);
            Assert.EndsWith("/workflows/wf_1/sessions", handler.LastRequest!.RequestUri!.ToString());
            Assert.Equal("{\"variables\":{\"name\":\"Ada\"}}", handler.LastBody);
            // The agent id rides on the thread so no later call has to repeat it.
            Assert.Equal("ses_1", thread.Id);
            Assert.Equal("wf_1", thread.Agent);
        }

        [Fact]
        public async Task TurnSendsPerTurnInstructions()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{}");
            using var client = NewClient(handler);

            await client.Threads.Messages.CreateAsync(
                new ChatThread("ses_1", "wf_1"),
                "hello",
                instructions: "answer in French");

            Assert.NotNull(handler.LastRequest);
            Assert.EndsWith("/workflows/wf_1/sessions/ses_1/messages", handler.LastRequest!.RequestUri!.ToString());
            Assert.Equal(
                "{\"content\":\"hello\",\"additional_instructions\":\"answer in French\"}",
                handler.LastBody);
        }

        [Fact]
        public async Task AnIncompleteThreadIsRefused()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{}");
            using var client = NewClient(handler);

            await Assert.ThrowsAsync<System.ArgumentException>(
                () => client.Threads.Messages.CreateAsync(new ChatThread(string.Empty, "wf_1"), "hi"));
        }

        [Fact]
        public async Task StreamYieldsTokensThenTheFinishedRun()
        {
            var body = Sse(("token", "{\"delta\":\"He\"}"), ("token", "{\"delta\":\"llo\"}"), ("done", "{\"status\":\"completed\"}"));
            var handler = new StubHandler(HttpStatusCode.OK, body);
            using var client = NewClient(handler);

            var text = new StringBuilder();
            StreamEvent? last = null;
            await foreach (var received in client.Threads.Runs.StreamAsync(new ChatThread("s", "w"), "hi"))
            {
                if (received.Type == "token")
                {
                    text.Append(received.Delta);
                }

                last = received;
            }

            Assert.Equal("Hello", text.ToString());
            Assert.NotNull(last);
            Assert.Equal("done", last!.Type);
            Assert.Equal("completed", last.Run.GetProperty("status").GetString());
            Assert.EndsWith("/workflows/w/sessions/s/messages/stream", handler.LastRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task StreamEmitsAFinalEventWithoutATrailingBlankLine()
        {
            // The last block has no trailing blank line; it must still be delivered.
            var body = "event: token\ndata: {\"delta\":\"x\"}\n\nevent: done\ndata: {\"status\":\"completed\"}";
            var handler = new StubHandler(HttpStatusCode.OK, body);
            using var client = NewClient(handler);

            var types = new List<string>();
            await foreach (var received in client.Threads.Runs.StreamAsync(new ChatThread("s", "w")))
            {
                types.Add(received.Type);
            }

            Assert.Equal(new[] { "token", "done" }, types);
        }

        [Fact]
        public async Task StreamRaisesTheApiErrorOnRejection()
        {
            var handler = new StubHandler(
                HttpStatusCode.PaymentRequired,
                "{\"error\":{\"code\":\"insufficient_credit\",\"message\":\"no credit\"}}");
            using var client = NewClient(handler);

            var error = await Assert.ThrowsAsync<GlytosException>(async () =>
            {
                await foreach (var _ in client.Threads.Runs.StreamAsync(new ChatThread("s", "w")))
                {
                    // consuming the stream is what performs the request
                }
            });

            Assert.Equal(402, error.Status);
            Assert.Equal("insufficient_credit", error.ErrorCode);
            Assert.Equal("no credit", error.Message);
        }

        [Fact]
        public async Task FoldersAndImports()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{}");
            using var client = NewClient(handler);

            await client.Folders.CreateAsync("Sales");
            Assert.NotNull(handler.LastRequest);
            Assert.EndsWith("/agent-folders", handler.LastRequest!.RequestUri!.ToString());
            Assert.Equal("{\"name\":\"Sales\"}", handler.LastBody);

            await client.Folders.DeleteAsync("fld_1");
            Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
            Assert.EndsWith("/agent-folders/fld_1", handler.LastRequest.RequestUri!.ToString());

            await client.Imports.AssistantAsync(new Dictionary<string, object?> { ["name"] = "Support" });
            Assert.Equal("{\"assistant\":{\"name\":\"Support\"}}", handler.LastBody);
        }

        [Fact]
        public async Task UploadIsMultipartNotJson()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"file_uuid\":\"f_1\"}");
            using var client = NewClient(handler);

            var file = await client.Chat.UploadFileAsync("tok", "ses_1", "hello", "notes.txt");

            Assert.Equal("f_1", file.FileUuid);
            Assert.NotNull(handler.LastContentType);
            Assert.StartsWith("multipart/form-data", handler.LastContentType!);
            // The boundary has to be declared or the server cannot parse the body.
            Assert.Contains("boundary=", handler.LastContentType!);
            Assert.Contains("name=\"token\"", handler.LastBody!);
            Assert.Contains("name=\"session_uuid\"", handler.LastBody!);
            Assert.Contains("filename=\"notes.txt\"", handler.LastBody!);
        }

        [Fact]
        public async Task FolderMoveSendsAnExplicitNullToUnfile()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"uuid\":\"wf_1\",\"name\":\"A\",\"mode\":\"prompt\"}");
            using var client = NewClient(handler);

            await client.Agents.MoveToFolderAsync("wf_1", "fld_1");
            Assert.Equal("{\"folder_uuid\":\"fld_1\"}", handler.LastBody);

            // "sent as null" is what unfiles an agent; "not sent" would leave it where it is.
            await client.Agents.RemoveFromFolderAsync("wf_1");
            Assert.Equal("{\"folder_uuid\":null}", handler.LastBody);
        }

        [Fact]
        public void AgentsIsTheSameResourceAsWorkflows()
        {
            using var client = NewClient(new StubHandler(HttpStatusCode.OK, "{}"));

            Assert.Same(client.Workflows, client.Agents);
        }

        private static string Sse(params (string Name, string Data)[] blocks)
        {
            var builder = new StringBuilder();
            foreach (var block in blocks)
            {
                builder.Append("event: ").Append(block.Name).Append('\n');
                builder.Append("data: ").Append(block.Data).Append("\n\n");
            }

            return builder.ToString();
        }

        private static GlytosClient NewClient(HttpMessageHandler handler) =>
            new GlytosClient("gly_test", new GlytosClientOptions { HttpClient = new HttpClient(handler) });

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _body;

            public HttpRequestMessage? LastRequest { get; private set; }

            public string? LastBody { get; private set; }

            public string? LastContentType { get; private set; }

            public StubHandler(HttpStatusCode status, string body)
            {
                _status = status;
                _body = body;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                LastBody = null;
                LastContentType = null;
                if (request.Content is not null)
                {
                    LastContentType = request.Content.Headers.ContentType?.ToString();
                    LastBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                return new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body),
                };
            }
        }
    }
}
