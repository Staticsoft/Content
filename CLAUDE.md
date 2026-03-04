# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture

This is a .NET library providing a provider-agnostic abstraction for LLM content generation, published as NuGet packages to GitHub Packages.

### Two Solutions

- **`Library.sln`** — Full solution including `Content.Tests`. Used for development and CI test runs.
- **`Packages.sln`** — Publishable packages only (`Content.Abstractions`, `Content.ChatGpt`, `Content.Claude`). Used for NuGet publishing on push to master.

### Core Abstractions (`Content.Abstractions`)

Two interfaces define the contract all provider implementations must satisfy:

- **`TextContent<Response>`** — Single-shot: takes a string prompt, returns a typed `Response` deserialized from JSON.
- **`StreamableContent`** — Streaming: takes `IEnumerable<Message>`, returns `IAsyncEnumerable<string>` token-by-token.
- **`Message`** — Shared model with `Author` enum (`System`/`User`/`Assistant`) and `Text`.
- **`ContentException`** — Base exception for all provider errors.

### Provider Implementations

**`Content.ChatGpt`** implements both abstractions via `Betalgo.Ranul.OpenAI`:
- `ChatGptStreamableContent` — Streaming via `IOpenAIService`, supports `ReasoningLevel` (`None`/`Low`/`Medium`/`High`).
- `ChatGptTextContent<Response>` — Non-streaming, forces `ResponseFormat.JsonObject`, deserializes to `Response`. Handles model responses wrapped in ` ```json ``` ` fences.
- Two options classes: `ChatGptContentOptions` (global: API key, model, reasoning) and `ChatGptContentOptions<Response>` (per-type: model, system message).
- Registration: `services.UseChatGpt(options)` + `services.UseChatGptModel<Response, MyOptions>()`.

**`Content.Claude`** — In progress. No implementation files yet.

**`Content.Memory`** — Stub. Intended for in-memory/test implementations.

### Key Dependencies

- `Staticsoft.TryReturn` — Fluent exception handling (`.On<TException>(handler).Result()`).
- `Staticsoft.Extensions.DependencyInjection` — Used in all projects with DI extensions.
- `Staticsoft.Testing.Unit` — Base test infrastructure in `Content.Tests`.

### Conventions

- Namespaces follow `Staticsoft.Content.*` (mapped from project names via `<RootNamespace>Staticsoft.$(MSBuildProjectName)</RootNamespace>`).
- Target framework: `net80` for all projects except `Content.Memory` which targets `net60`.
- Package versioning: `{VersionPrefix}-{github.run_number}` (e.g., `1.0.0-42`), published to `https://nuget.pkg.github.com/Staticsoft/index.json`.
- `NuGet.Config` includes both `nuget.org` and the Staticsoft GitHub Packages feed (credentials pre-configured for restoring internal packages).