# Glytos

[![CI](https://github.com/Glytos/glytos-sdk-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/Glytos/glytos-sdk-dotnet/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Glytos)](https://www.nuget.org/packages/Glytos)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

The official [Glytos](https://glytos.com) server SDK for .NET.

Call the Glytos API from your backend with an API key. Build agents once and run
them as **text** or as **voice**: hold a threaded conversation, stream a reply as it
is written, place phone calls, mint browser web-call tokens, manage numbers, and
verify webhooks. Targets `net8.0` and `netstandard2.0`.

> Never ship an API key to the browser. For in-browser voice, use the `@glytos/web`
> package with a short-lived token you mint here.

## Install

```bash
dotnet add package Glytos
```

## Quickstart

```csharp
using Glytos;

using var glytos = new GlytosClient("gly_...");

// List your agents
var agents = await glytos.Agents.ListAsync();

// Talk to one as text
var thread = await glytos.Threads.CreateAsync(agents[0].Uuid);
var run = await glytos.Threads.Runs.CreateAsync(thread, "What are your opening hours?");
Console.WriteLine(run.GetProperty("messages")[0].GetProperty("content").GetString());

// Or mint a web-call token and talk to the same agent in the browser
var token = await glytos.Calls.WebTokenAsync(workflowUuid: agents[0].Uuid);
Console.WriteLine($"{token.Token} {token.WsUrl}");
```

### Streaming

A long answer should not arrive as one silent wait:

```csharp
await foreach (var e in glytos.Threads.Runs.StreamAsync(thread, "Summarise the policy"))
{
    if (e.Type == "token") Console.Write(e.Delta);
    if (e.Type == "done") Console.WriteLine();
}
```

### Per-turn instructions

Extra context for one turn only, applied below the agent's own instructions and
never saved to it:

```csharp
await glytos.Threads.Runs.CreateAsync(
    thread,
    "Rate this transcript",
    instructions: "Score 1-5 and reply as JSON.");
```

Scope the client to an environment or a regional stack, or hand it an `HttpClient`
(e.g. from `IHttpClientFactory`) via options:

```csharp
using var glytos = new GlytosClient("gly_...", new GlytosClientOptions
{
    Environment = "prod",
});

var overview = await glytos.RequestAsync<JsonElement>("GET", "/analytics/overview");
```

## Resources

| Property | Methods |
| --- | --- |
| `glytos.Agents` (alias `Workflows`) | `ListAsync`, `RetrieveAsync`, `CreateAsync`, `RenameAsync`, `PublishAsync`, `PromoteAsync`, `DuplicateAsync`, `ArchiveAsync`, `DeleteAsync`, `TemplatesAsync`, `ExportAsync`, `MoveToFolderAsync`, `RemoveFromFolderAsync`, `VersionsAsync`, `StartSessionAsync`, `SendMessageAsync`, `StreamMessageAsync`, `RunTextAsync` |
| `glytos.Threads` | `CreateAsync`, `RetrieveAsync`, `Messages.CreateAsync`, `Messages.ListAsync`, `Runs.CreateAsync`, `Runs.StreamAsync` |
| `glytos.Folders` | `ListAsync`, `CreateAsync`, `RenameAsync`, `DeleteAsync` |
| `glytos.Imports` | `SourcesAsync`, `CreateAsync`, `AssistantAsync` |
| `glytos.Chat` | `TokenAsync`, `MessagesAsync`, `StreamAsync`, `UploadFileAsync` |
| `glytos.Calls` | `CreateAsync`, `ListAsync`, `RetrieveAsync`, `WebTokenAsync`, `ControlAsync` |
| `glytos.PhoneNumbers` | `SearchAsync`, `ListAsync`, `ProvisionAsync`, `ImportNumberAsync`, `InstantAsync`, `AssignAsync`, `ReleaseAsync`, `ProvidersAsync` |
| `glytos.KnowledgeBase` | `ListDocumentsAsync`, `CreateDocumentAsync`, `UploadDocumentAsync`, `SearchAsync` |
| `glytos.VectorStores` | `ListAsync`, `CreateAsync`, `RetrieveAsync`, `DeleteAsync`, `UploadDocumentAsync` |
| `glytos.Tools` | `ListAsync`, `CreateAsync`, `RetrieveAsync`, `UpdateAsync`, `DeleteAsync` |
| `glytos.Campaigns` | `ListAsync`, `CreateAsync`, `RetrieveAsync`, `StartAsync`, `StopAsync`, `DeleteAsync`, `AddContactsAsync`, `SyncContactsAsync`, `PreviewSuppressionAsync` |
| `glytos.Dnc` | `ListAsync`, `AddAsync`, `ImportAsync`, `SetScopeAsync`, `RemoveAsync` |
| `glytos.Sessions` | `ListAsync` |
| `glytos.Analytics` | `OverviewAsync` |
| `glytos.Webhooks` | `ListAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `EventsAsync`, `DeliveriesAsync`, `RedeliverAsync`, `Verify` |

`Agents` and `Workflows` are the same resource under two names: the product calls
them agents, the API path is `/workflows`. Either works.

### Text and voice are separate

An agent is one definition. Nothing forces it to do both:

- A **text** agent needs only `Threads` (or `Chat` for a browser widget).
- A **voice** agent adds `Calls`, `PhoneNumbers` and `Campaigns`.
- The same agent can do both, if you want it to.

Any endpoint without a dedicated helper is one call away with
`glytos.RequestAsync<T>(method, path, body, query)`, or
`glytos.StreamAsync(method, path, body)` for a Server-Sent Events one.

## Outbound calling

A campaign dials a list of contacts with one agent. Upload the list as CSV text:
the phone column is found by its header or by which column holds phone numbers,
and every other column travels with that contact, so `{{name}}` in the agent's
prompt means the person being called.

```csharp
var campaign = await glytos.Campaigns.CreateAsync(
    "March outreach",
    agent.Uuid,
    "+15551230000", // must be a number you have connected
    contactsCsv: File.ReadAllText("leads.csv"),
    scheduledAt: new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero),
    callWindowStart: "09:00",
    callWindowEnd: "20:00",
    timezone: "Europe/Istanbul");
```

Left unscheduled, a campaign stays a draft until `StartAsync`. `StopAsync` ends it
at the next contact, leaving the undialed ones ready to resume. `RetrieveAsync`
returns each contact's outcome and, where one answered, the session it produced.

Every outbound call is checked against your do-not-call list first, whether it
comes from a campaign or from `Calls.CreateAsync`. Agents add to that list
themselves when someone asks not to be contacted again:

```csharp
await glytos.Dnc.AddAsync("+15551230000", "asked on a call");
```

A campaign chooses how much of the list applies. The default, `strict`, honours
all of it. `transactional` still calls people who only refused marketing, which
is what you want for a call about someone's own order. `ignore` skips entries
your organization added for itself, but requests people made on a call still
apply unless you also set `overrideCallerRequests`. Measure before you choose:

```csharp
var preview = await glytos.Campaigns.PreviewSuppressionAsync(
    contactsCsv: File.ReadAllText("leads.csv"));
Console.WriteLine(
    $"{preview.ReachedIfStrict} of {preview.Contacts} reachable; " +
    $"{preview.CallerRequested} asked us not to call");
```

## ASP.NET Core

Install the DI integration and register the client against `IHttpClientFactory`:

```bash
dotnet add package Glytos.Extensions.DependencyInjection
```

```csharp
builder.Services.AddGlytos(builder.Configuration["Glytos:ApiKey"]!, options =>
{
    options.Environment = "prod";
});
```

Then inject `GlytosClient` into your controllers, minimal-API handlers, or services:

```csharp
app.MapGet("/agents", async (GlytosClient glytos) => await glytos.Workflows.ListAsync());
```

## Errors

Non-2xx responses (and transport failures, with `Status` `0`) throw a
`GlytosException` carrying the API error code, HTTP status, and the request id:

```csharp
try
{
    await glytos.Workflows.RetrieveAsync("missing");
}
catch (GlytosException ex)
{
    Console.WriteLine($"{ex.Status} {ex.ErrorCode} {ex.Message} {ex.RequestId}");
}
```

## Webhooks

Verify a delivery came from Glytos before trusting it. Pass the **raw** request body,
the `X-Glytos-Signature` header, and your endpoint secret:

```csharp
using Glytos;

var ok = Webhook.Verify(rawBody, Request.Headers["X-Glytos-Signature"], webhookSecret);
if (!ok)
{
    return BadRequest();
}
```

## License

MIT
