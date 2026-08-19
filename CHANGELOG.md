# Changelog

All notable changes to this project are documented in this file. The format is based
on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.5.0] - 2026-08-19

### Added

- `Campaigns.UpdateAsync`, `UnscheduleAsync`, `DuplicateAsync` and `ExportAsync`.
  A rename is accepted at any point; the schedule and the calling window can only
  be changed before a campaign starts. `UnscheduleAsync` is separate because
  `UpdateAsync` drops a null argument, and clearing a schedule has to send one.
- `Campaign` gained `Counts`, `WorkflowName` and `Imported`; `CampaignCounts` is
  new. Measure progress against `Counts.Dialable` rather than `Counts.Total`.
- `ContactSyncResult` gained `Duplicates` and `OnDoNotCall`.
- `GlytosClient.RequestTextAsync` for endpoints that do not answer in JSON.

## [0.4.0] - 2026-08-17

### Added

- `SipTrunks` - connect a carrier directly over SIP, with no third party in
  between: `PresetsAsync`, `ListAsync`, `CreateAsync`, `UpdateAsync`,
  `DeleteAsync`, `TestAsync`. Numbers are attached to a registered trunk through
  `PhoneNumbers.ImportNumberAsync`, which now takes `sipTrunkUuid`.
- `Integrations` and `Integrations.Connections` - the destinations an agent or an
  automation can act on, and the named connections holding their credentials.
- `Automations` - fire an integration action when an event happens: `ListAsync`,
  `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `RunsAsync`, `TestAsync`.
- `TestSuites` - `ListAsync`, `CreateAsync`, `DeleteAsync`, `RunAsync`.
- `Billing` - `CreditsAsync`, `TransactionsAsync`, `UsageAsync`. Checking the
  balance before a long outbound run no longer needs a raw `RequestAsync` call.
- `Environments.ListAsync`, `Providers.ListAsync`, `Providers.ResourcesAsync`,
  `ApiKeys.ListAsync`/`CreateAsync`/`DeleteAsync`,
  `Organizations.RetrieveAsync`/`UpdateAsync`/`RegionsAsync`.
- `KnowledgeBase.RetrieveDocumentAsync` and
  `KnowledgeBase.DeleteDocumentAsync`. Documents could be created and listed but
  never read back or removed.
- `Tools.DiscoverMcpAsync` - ask an MCP server what it publishes, instead of
  transcribing its schema by hand.
- `Imports.ConnectAsync` and `Imports.PullAsync` - list the agents on another
  platform with its API key, then bring over the ones you pick. The key is never
  stored.
- `Calls.SayAsync`, `Calls.TransferAsync` and `Calls.EndAsync`, which spell out
  what each control action requires. `Calls.ControlAsync` still takes a raw
  object.
- `Workflows.CreateAsync` takes `primaryChannel`.

### Fixed

- `Tools` documented `kind` as http / static / mcp. The API has accepted `code`,
  `integration` and `client` since they shipped, and the summary now says what
  each of the six does.
- The README advertised a `Tools.RetrieveAsync` that does not exist.

## [0.3.0] - 2026-08-09

### Added

- `Dnc` - the numbers your organization must not call: `Dnc.ListAsync`,
  `Dnc.AddAsync`, `Dnc.ImportAsync`, `Dnc.SetScopeAsync`, `Dnc.RemoveAsync`. Every
  outbound call is checked against this list, whether it comes from a campaign or
  from `Calls.CreateAsync`.
- `Campaigns.StopAsync`, `Campaigns.DeleteAsync` and `Campaigns.AddContactsAsync`
  (upload a contact list as CSV text rather than serving it over HTTP).
- `Campaigns.PreviewSuppressionAsync` - how many of a contact list each
  suppression policy would reach, including how many of those people asked on a
  call not to be contacted again.
- `Campaigns.CreateAsync` gained `contactsCsv`, `scheduledAt`, `callWindowStart`
  /`callWindowEnd`, `timezone`, `suppressionPolicy` and `overrideCallerRequests`.
- `CampaignContact`, `SuppressionPreview`, `ContactSyncResult`, `DncEntry`,
  `DncList` and `DncImportResult` models. `Campaign` gained its scheduling,
  calling-window and suppression fields; `CampaignDetail` now derives from it and
  carries `Contacts`.

### Changed

- `Campaigns.StartAsync` and `Campaigns.SyncContactsAsync` return `Campaign` and
  `ContactSyncResult` rather than a raw `JsonElement`.

### Fixed

- `Campaigns.CreateAsync` took `contacts` as an untyped `object`, inviting the
  record shape the API rejects with a 422. It is an `IEnumerable<string>` of
  phone numbers.

## [0.2.0] - 2026-08-02

### Added

- `Threads` - conversations with a text agent in thread/run vocabulary:
  `Threads.CreateAsync`, `Threads.RetrieveAsync`, `Threads.Messages.CreateAsync/ListAsync`,
  `Threads.Runs.CreateAsync/StreamAsync`.
- Streaming. `Threads.Runs.StreamAsync`, `Agents.StreamMessageAsync` and
  `Chat.StreamAsync` return an `IAsyncEnumerable<StreamEvent>` of `token` deltas and
  a terminal `done` carrying the finished run.
- Per-turn instructions on every text turn (`instructions`), applied below the
  agent's own and never saved to it.
- File uploads: `Chat.UploadFileAsync`, `KnowledgeBase.UploadDocumentAsync`,
  `VectorStores.UploadDocumentAsync`, plus `UploadAsync<T>()` for any other
  multipart endpoint.
- `Folders` - group agents inside an environment, and
  `Agents.MoveToFolderAsync` / `Agents.RemoveFromFolderAsync` to file one.
- `Imports` - bring an agent over from another platform, and `Agents.ExportAsync`
  for the portable, secret-free JSON that imports back.
- `Agents` as an alias of `Workflows`, matching what the product calls them.

### Changed

- `SendMessageAsync` takes an optional `instructions` argument before the
  cancellation token.

## [0.1.0] - 2026-07-19

### Added

- Initial release.
- `GlytosClient` with `Workflows`, `Calls`, `PhoneNumbers`, `Sessions` and `Webhooks`
  resources, plus a generic `RequestAsync<T>()` for any other endpoint.
- Forward-compatible model records and a typed `GlytosException`.
- `Webhook.Verify()` for webhook signature verification.
- `Glytos.Extensions.DependencyInjection` package with `services.AddGlytos(...)`.
- Multi-targets `net8.0` and `netstandard2.0`.
