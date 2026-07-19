# Changelog

All notable changes to this project are documented in this file. The format is based
on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-07-19

### Added

- Initial release.
- `GlytosClient` with `Workflows`, `Calls`, `PhoneNumbers`, `Sessions` and `Webhooks`
  resources, plus a generic `RequestAsync<T>()` for any other endpoint.
- Forward-compatible model records and a typed `GlytosException`.
- `Webhook.Verify()` for webhook signature verification.
- `Glytos.Extensions.DependencyInjection` package with `services.AddGlytos(...)`.
- Multi-targets `net8.0` and `netstandard2.0`.
