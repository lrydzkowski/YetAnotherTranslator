# Implementation Plan: Polish-English Translation CLI Tool

**Branch**: `001-polish-english-translator` | **Date**: 2025-11-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-polish-english-translator/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a REPL-style CLI tool in .NET 10 for Polish-English translation with rich linguistic information, grammar review, and pronunciation playback. Core features use LLM (Claude Sonnet 4.5 via official Anthropic SDK) for translations, grammar analysis, and language detection. Pronunciation uses ElevenLabs API with caching. All operations are cached in PostgreSQL via EF Core. Configuration stored in JSON with secrets managed through Azure Key Vault. Three-project architecture: Core (business logic with handler classes), Infrastructure (LLM/TTS/database/Key Vault integrations), and CLI (PrettyPrompt + Spectre.Console REPL interface). FluentValidation for input validation. Direct handler invocation (no MediatR). History tracking for all operations with cache-bypass option.

## Technical Context

**Language/Version**: .NET 10 (C#)
**Primary Dependencies**: Official Anthropic SDK (Claude Sonnet 4.5), PrettyPrompt, Spectre.Console, FluentValidation, EF Core, Azure SDK (Key Vault), ElevenLabs SDK
**Storage**: PostgreSQL with EF Core for operation history and pronunciation cache
**Testing**: xUnit, NSubstitute (mocking), Testcontainers (PostgreSQL containers), WireMock.Net (external API mocking), Verify (snapshot testing)
**Target Platform**: Cross-platform CLI (.NET 10 runtime - Windows, Linux, macOS)
**Project Type**: Multi-project solution (Core + Infrastructure + CLI)
**Architecture Pattern**: Clean architecture with handler classes (no MediatR - direct handler invocation)
**Performance Goals**: Word translation <3s, pronunciation playback <2s, history retrieval <1s, configuration validation <1s
**Constraints**: Internet connectivity required for LLM/TTS/Azure Key Vault, text translation limited to 5000 characters
**Scale/Scope**: REPL CLI tool for single user, all history entries retained, caching for translations and pronunciations
**Testing Strategy**: Integration tests only - test individual features with real PostgreSQL (Testcontainers) and mocked external APIs (WireMock.Net)

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### I. Single Responsibility

- ✓ Core project: Business logic only (translation handlers, grammar review handlers)
- ✓ Infrastructure project: External integrations only (LLM, TTS, database, Azure Key Vault)
- ✓ CLI project: User interface only (REPL, command parsing, output formatting)
- Each project has clear separation of concerns

### II. Testability

- ✓ Core business logic uses interfaces for all external dependencies
- ✓ LLM provider abstraction allows testing without real API calls
- ✓ TTS provider abstraction allows testing without real API calls
- ✓ Database in a container in tests
- ✓ Azure Key Vault access through interface for testing with mock secrets

### III. CLI Standards

- ✓ REPL interface follows standard CLI conventions
- ✓ Output to stdout, errors to stderr
- ✓ Exit code 0 for success, non-zero for configuration/runtime errors
- ✓ Configuration via JSON file in standard user config directory

### IV. Error Handling

- ✓ Configuration validation fails fast at startup with descriptive messages
- ✓ All LLM/TTS/database/secret manager errors include context
- ✓ No silent failures - all errors reported to user
- ✓ Errors handled at appropriate level (validation at startup, runtime errors in handlers)

### V. Simplicity

- ✓ Direct handler invocation (no MediatR complexity)
- ✓ Handler classes provide clear command/query separation
- ✓ LLM-based approach eliminates need for separate spell-checking logic
- ✓ Caching through database (simple) vs. separate cache layer
- ✓ Direct Azure Key Vault integration (no multi-backend abstraction initially)

**STATUS**: ✓ PASS - All constitution principles satisfied

**POST-DESIGN RE-EVALUATION** (2025-11-02):

After completing Phase 1 design (data-model.md, contracts/, quickstart.md), all constitution principles remain satisfied:

### I. Single Responsibility (Re-verified)

- ✓ Data model separates domain models, database entities, and configuration models
- ✓ Command interface has 12 focused commands, each with single purpose
- ✓ Cache entities independent (translation, text translation, pronunciation separate)
- ✓ No God objects or multi-purpose utilities introduced

### II. Testability (Re-verified)

- ✓ All domain models are immutable value objects (easy to test)
- ✓ Database entities use EF Core (supports in-memory provider for testing)
- ✓ Cache key generation is pure function (deterministic, testable)
- ✓ All interfaces defined in Core, implementations in Infrastructure
- ✓ CQRS handlers testable with mocked dependencies

### III. CLI Standards (Re-verified)

- ✓ Command interface follows REPL conventions with `/` prefix
- ✓ Clear stdout/stderr separation documented in contracts
- ✓ Exit codes specified: 0 for success, 1 for errors
- ✓ Configuration via JSON file in standard user config directory
- ✓ Help command provides full command reference

### IV. Error Handling (Re-verified)

- ✓ All error cases documented in command interface contracts
- ✓ Configuration validation with clear error messages (JSON syntax errors with line/column)
- ✓ API failures include context (timeout, rate limit, authentication)
- ✓ Database failures handled gracefully (operation completes, warning shown)
- ✓ Startup validation fails fast with descriptive messages

### V. Simplicity (Re-verified)

- ✓ Cache implementation is simple: database table with SHA256 keys
- ✓ No separate cache layer abstraction - EF Core queries are sufficient
- ✓ Configuration structure is flat and straightforward
- ✓ Command parsing uses simple switch expression (no complex parser)
- ✓ Azure Key Vault only (no multi-backend abstraction complexity in v1)

**FINAL STATUS**: ✓ PASS - All constitution principles satisfied post-design

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── YetAnotherTranslator.Core/
│   ├── Handlers/              # Business logic handlers
│   │   ├── TranslateWordHandler.cs
│   │   ├── TranslateTextHandler.cs
│   │   ├── ReviewGrammarHandler.cs
│   │   ├── PlayPronunciationHandler.cs
│   │   └── GetHistoryHandler.cs
│   ├── Interfaces/            # Abstractions for Infrastructure
│   │   ├── ILlmProvider.cs
│   │   ├── ITtsProvider.cs
│   │   ├── ISecretsProvider.cs
│   │   └── IHistoryRepository.cs
│   ├── Models/                # Domain models
│   │   ├── TranslationResult.cs
│   │   ├── GrammarReviewResult.cs
│   │   └── HistoryEntry.cs
│   └── Validation/            # FluentValidation validators
│       ├── TranslateWordValidator.cs
│       └── TranslateTextValidator.cs
│
├── YetAnotherTranslator.Infrastructure/
│   ├── Llm/
│   │   └── AnthropicProvider.cs    # Claude SDK implementation
│   ├── Tts/
│   │   └── ElevenLabsProvider.cs   # ElevenLabs SDK implementation
│   ├── Secrets/
│   │   └── AzureKeyVaultProvider.cs
│   ├── Persistence/
│   │   ├── TranslatorDbContext.cs
│   │   ├── Entities/
│   │   └── Repositories/
│   └── Configuration/
│       └── ConfigurationValidator.cs
│
└── YetAnotherTranslator.Cli/
    ├── Repl/
    │   ├── ReplEngine.cs           # PrettyPrompt integration
    │   └── CommandParser.cs
    ├── Display/
    │   └── OutputFormatter.cs      # Spectre.Console formatting
    ├── Program.cs
    └── appsettings.json

tests/
└── YetAnotherTranslator.Tests.Integration/
    ├── Features/                       # Feature integration tests
    │   ├── TranslateWordTests.cs
    │   ├── TranslateTextTests.cs
    │   ├── ReviewGrammarTests.cs
    │   └── PlayPronunciationTests.cs
    ├── Infrastructure/                 # Test infrastructure
    │   ├── TestBase.cs                 # Base class with Testcontainers + WireMock setup
    │   └── WireMockFixtures/           # Reusable WireMock response fixtures
    └── Helpers/

.gitignore                      # Generated with dotnet new gitignore
YetAnotherTranslator.sln
```

**Structure Decision**: Multi-project .NET solution following clean architecture principles. Core contains business logic handlers and interfaces, Infrastructure implements those interfaces with concrete providers (official Anthropic SDK, ElevenLabs, Azure Key Vault, PostgreSQL), and CLI provides the REPL user interface. Handlers are invoked directly (no MediatR) for simplicity. This structure ensures testability through dependency injection. Based on user requirement to follow LexicaNext project practices.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation                  | Why Needed         | Simpler Alternative Rejected Because |
| -------------------------- | ------------------ | ------------------------------------ |
| [e.g., 4th project]        | [current need]     | [why 3 projects insufficient]        |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient]  |

