# Changelog

All notable changes to this project are documented in this file. The format is based
on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
