# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture

This is a .NET library providing a provider-agnostic abstraction for LLM content generation, published as NuGet packages to GitHub Packages.

### Two Solutions

- **`Library.sln`** — Full solution including `Content.Tests`. Used for development and CI test runs.
- **`Packages.sln`** — Publishable packages only (`Content.Abstractions`, `Content.ChatGpt`, `Content.Claude`, `Content.Bedrock`, `Content.Memory`). Used for NuGet publishing on push to master.

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
- Registration: `services.UseChatGpt(options)` + `services.UseChatGptModel<Response, MyOptions>()`.

**`Content.Claude`** implements both abstractions via the `Anthropic` SDK:
- `ClaudeStreamableContent` — Streaming, supports `ReasoningLevel` mapped to thinking budget tokens.
- `ClaudeTextContent<Response>` — Non-streaming, deserializes to `Response`, handles ` ```json ``` ` fences.
- Registration: `services.UseClaude(options)` + `services.UseClaudeModel<Response, MyOptions>()`.

**`Content.Bedrock`** implements both abstractions via `AWSSDK.BedrockRuntime` using the model-agnostic Converse API, so any Bedrock text model works (Nova, Claude on Bedrock, etc.):
- `BedrockStreamableContent` — `ConverseStreamAsync`, yields `ContentBlockDeltaEvent` text.
- `BedrockTextContent<Response>` — `ConverseAsync`, deserializes to `Response`, handles ` ```json ``` ` fences.
- Registration: `services.UseBedrock(clientFactory, optionsFactory)` + `services.UseBedrockModel<Response, MyOptions>()`. The client factory admits the default AWS credential chain (`new AmazonBedrockRuntimeClient()`) in Lambda.

**`Content.Memory`** — In-process implementations for tests and local development:
- `MemoryStreamableContent` — Deterministic: repeats the last non-system message in chunks (a model that always follows the "repeat the user's message" instruction).
- `MemoryTextContent<Response>` — Produces responses with a function configured at registration.
- Registration: `services.UseMemory()` + `services.UseMemoryModel<Response>(produce)`.

### Tests (`Content.Tests`)

Shared abstract suites test the abstraction; implementations plug in via `Services` overrides:

- **`StreamableContentTests`** — Non-empty responses, minimal instruction following (parrot prompt), multi-chunk streaming, consecutive requests.
- **`TextContentTests`** — Typed JSON response production.

`MemoryStreamableContentTests`/`MemoryTextContentTests` run everywhere. `BedrockStreamableContentTests`/`BedrockTextContentTests` require `ContentAccessKeyId`, `ContentSecretAccessKey`, `ContentRegion`, `ContentBedrockModelId` environment variables (provided by repository secrets in CI). ChatGpt/Claude subclasses can be added the same way when API-key secrets are available.

### Key Dependencies

- `Staticsoft.TryReturn` — Fluent exception handling (`.On<TException>(handler).Result()`).
- `Staticsoft.Extensions.DependencyInjection` — Used in all projects with DI extensions.
- `Staticsoft.Testing.Unit` — Base test infrastructure (`TestBase<T>` with `Services`/`SUT`) in `Content.Tests`.

### Conventions

- Namespaces follow `Staticsoft.Content.*` (mapped from project names via `<RootNamespace>Staticsoft.$(MSBuildProjectName)</RootNamespace>`).
- Target framework: `net10.0` for all projects.
- Package versioning: `{VersionPrefix}-{github.run_number}` (e.g., `1.0.0-42`), published to `https://nuget.pkg.github.com/Staticsoft/index.json`.
- `NuGet.Config` includes both `nuget.org` and the Staticsoft GitHub Packages feed (credentials pre-configured for restoring internal packages).
