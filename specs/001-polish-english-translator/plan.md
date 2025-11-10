# Implementation Plan: Polish-English Translation CLI Tool

**Branch**: `001-polish-english-translator` | **Date**: 2025-11-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-polish-english-translator/spec.md` + User request for self-contained handler structure

**Note**: This plan incorporates the user's requirement to keep commands self-contained in the Core project with each handler in its own namespace alongside its models, validators, and interfaces.

## Summary

A REPL-style CLI tool for Polish-English translation with word translation, text translation, grammar review, and pronunciation playback. Uses LLM (Anthropic Claude) for translations and grammar review, ElevenLabs for text-to-speech, PostgreSQL for history and caching, and Azure Key Vault for secrets management.

**Key Architectural Decision**: Handlers are self-contained in the Core project - each handler resides in its own namespace (`YetAnotherTranslator.Core.Handlers.{HandlerName}`) with its models, validator, and any necessary interfaces. This promotes modularity and makes each feature independently understandable.

## Technical Context

**Language/Version**: .NET 10 (C#)
**Primary Dependencies**:

- Official Anthropic SDK (LLM provider)
- PrettyPrompt v4.x (REPL input)
- Spectre.Console v0.49+ (CLI output)
- FluentValidation v11.10+ (input validation)
- EF Core 10.0 with Npgsql (PostgreSQL ORM)
- Azure.Security.KeyVault.Secrets v4.8.0 + Azure.Identity v1.17.0 (secrets management)
- ElevenLabs-DotNet v3.6.0 (text-to-speech)
- PortAudioSharp v0.3.0 (cross-platform audio playback)

**Storage**: PostgreSQL 16+ with EF Core for operation history and caching (translations, pronunciations)
**Testing**: xUnit with NSubstitute (mocking), Testcontainers (PostgreSQL containers), WireMock.Net (external API mocking), Verify (snapshot testing) - integration tests only
**Target Platform**: Cross-platform CLI (Windows, macOS, Linux)
**Project Type**: Multi-project solution (Core + Infrastructure + CLI)
**Performance Goals**:

- Word translation <3s (SC-001)
- Pronunciation playback <2s (SC-006)
- History retrieval <1s (SC-008)
- Configuration validation <1s (SC-011)

**Constraints**:

- Text snippets limited to 5000 characters (FR-024/SC-004)
- Requires internet connectivity for LLM/TTS APIs
- Azure Key Vault authentication via `az login` or managed identity

**Scale/Scope**:

- 4 primary operations (word translation, text translation, grammar review, pronunciation)
- Unlimited operation history (FR-045)
- Multi-project .NET solution with clean architecture

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### I. Single Responsibility ✓

**Status**: PASS

Each component has one clear purpose:

- **TranslateWordHandler**: Translates individual words only
- **TranslateTextHandler**: Translates text snippets only
- **ReviewGrammarHandler**: Reviews English grammar only
- **PlayPronunciationHandler**: Plays pronunciation audio only
- **GetHistoryHandler**: Retrieves operation history only

Each handler is self-contained in its own namespace with its models, validator, and interfaces.

### II. Testability ✓

**Status**: PASS

All business logic is testable through:

- **Dependency Injection**: All handlers use constructor injection for dependencies
- **Interfaces**: Infrastructure dependencies abstracted behind interfaces (ILlmProvider, ITtsProvider, ISecretsProvider, IHistoryRepository)
- **Integration Tests**: Real PostgreSQL via Testcontainers, mocked external APIs via WireMock.Net
- **No concrete implementation coupling**: Handlers depend on interfaces, not concrete classes

### III. CLI Standards ✓

**Status**: PASS

- **Arguments**: Configuration via config.json and command-line flags (--no-cache)
- **Output**: Results written to stdout (via Spectre.Console)
- **Errors**: Error messages written to stderr
- **Exit codes**: 0 for success, non-zero for errors (configuration failures, etc.)

### IV. Error Handling ✓

**Status**: PASS

- **Fail fast**: Configuration validation at startup, API failures exit immediately
- **Descriptive messages**: FluentValidation provides detailed validation errors, LLM/TTS failures include context
- **No silent failures**: All exceptions caught and reported with context
- **Appropriate level**: Configuration errors at startup, operation errors per-command

### V. Simplicity ✓

**Status**: PASS with Justification

- **Boring solutions**: Direct handler invocation (no MediatR), standard .NET DI, EF Core
- **Explicit over implicit**: Handler methods explicitly named (HandleAsync), clear parameter lists
- **Simpler approach**: No command/query objects, no pipeline behaviors, no event sourcing

**Complexity Justification**: Three-project structure (Core + Infrastructure + CLI) is necessary for clean architecture separation:

1. **Core**: Business logic with no external dependencies
2. **Infrastructure**: External integrations (LLM, TTS, secrets, database)
3. **CLI**: User interface (REPL, display)

This is the simplest structure that achieves testability and dependency inversion without violating clean architecture principles.

### Re-evaluation Post-Design ✓

**Status**: PASS

After Phase 1 design, the architecture remains compliant:

- **Self-contained handlers**: Each handler namespace is independently understandable
- **No additional complexity**: No new abstractions or patterns introduced
- **Clean boundaries**: Core → Infrastructure interface contracts, CLI → Core handler invocations
- **Testability maintained**: Integration tests verify end-to-end behavior with real database

## Project Structure

### Documentation (this feature)

```text
specs/001-polish-english-translator/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (completed)
├── data-model.md        # Phase 1 output (completed, to be updated)
├── quickstart.md        # Phase 1 output (completed)
├── contracts/           # Phase 1 output (completed)
│   └── command-interface.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── YetAnotherTranslator.Core/          # Business logic (no external dependencies)
│   └── Handlers/                       # Self-contained handler namespaces
│       ├── TranslateWord/              # Word translation feature
│       │   ├── TranslateWordHandler.cs          # Main handler
│       │   ├── TranslateWordRequest.cs          # Request model
│       │   ├── TranslateWordValidator.cs        # FluentValidation validator
│       │   ├── TranslationResult.cs             # Result model
│       │   └── Translation.cs                   # Nested translation model
│       │
│       ├── TranslateText/              # Text translation feature
│       │   ├── TranslateTextHandler.cs
│       │   ├── TranslateTextRequest.cs
│       │   ├── TranslateTextValidator.cs
│       │   └── TextTranslationResult.cs
│       │
│       ├── ReviewGrammar/              # Grammar review feature
│       │   ├── ReviewGrammarHandler.cs
│       │   ├── ReviewGrammarRequest.cs
│       │   ├── ReviewGrammarValidator.cs
│       │   ├── GrammarReviewResult.cs
│       │   ├── GrammarIssue.cs
│       │   └── VocabularySuggestion.cs
│       │
│       ├── PlayPronunciation/          # Pronunciation playback feature
│       │   ├── PlayPronunciationHandler.cs
│       │   ├── PlayPronunciationRequest.cs
│       │   ├── PlayPronunciationValidator.cs
│       │   └── PronunciationResult.cs
│       │
│       └── GetHistory/                 # History retrieval feature
│           ├── GetHistoryHandler.cs
│           ├── GetHistoryRequest.cs
│           ├── GetHistoryValidator.cs
│           ├── HistoryEntry.cs
│           └── CommandType.cs                   # Enum for command types
│   │
│   └── Interfaces/                     # Shared interfaces (not in handler namespaces)
│       ├── ILlmProvider.cs             # Interface for LLM operations
│       ├── ITtsProvider.cs             # Interface for TTS operations
│       ├── IAudioPlayer.cs             # Interface for audio playback
│       ├── ISecretsProvider.cs         # Interface for secrets management
│       └── IHistoryRepository.cs       # Interface for history access
│
├── YetAnotherTranslator.Infrastructure/  # External integrations
│   ├── Llm/                            # LLM provider implementation
│   │   └── AnthropicProvider.cs        # Implements ILlmProvider from Core handlers
│   ├── Tts/                            # TTS provider implementation
│   │   ├── ElevenLabsProvider.cs       # Implements ITtsProvider (text-to-speech generation)
│   │   └── PortAudioPlayer.cs          # Implements IAudioPlayer (audio playback via PortAudioSharp)
│   ├── Secrets/                        # Secret manager implementation
│   │   └── AzureKeyVaultProvider.cs    # Implements ISecretsProvider
│   ├── Persistence/                    # Database implementation
│   │   ├── TranslatorDbContext.cs      # EF Core DbContext
│   │   ├── Entities/                   # Database entities
│   │   │   ├── HistoryEntryEntity.cs
│   │   │   ├── TranslationCacheEntity.cs
│   │   │   ├── TextTranslationCacheEntity.cs
│   │   │   └── PronunciationCacheEntity.cs
│   │   ├── Repositories/               # Repository implementations
│   │   │   └── HistoryRepository.cs    # Implements IHistoryRepository
│   │   └── Migrations/                 # EF Core migrations
│   └── Configuration/                  # Configuration models and validation
│       ├── ApplicationConfiguration.cs
│       ├── SecretManagerConfiguration.cs
│       ├── LlmProviderOptions.cs
│       ├── TtsProviderOptions.cs
│       ├── DatabaseOptions.cs
│       └── Validators/                 # FluentValidation validators for config
│
└── YetAnotherTranslator.Cli/           # User interface
    ├── Repl/                           # REPL engine
    │   ├── ReplEngine.cs               # Main REPL loop
    │   ├── CommandParser.cs            # Parse user commands
    │   └── Command.cs                  # Command model
    ├── Display/                        # Output formatting
    │   └── DisplayFormatter.cs         # Spectre.Console table formatting
    └── Program.cs                      # Entry point, DI setup, config loading

tests/
└── YetAnotherTranslator.Tests.Integration/
    ├── Features/                       # Feature integration tests
    │   ├── TranslateWordTests.cs       # Tests TranslateWordHandler end-to-end
    │   ├── TranslateTextTests.cs
    │   ├── ReviewGrammarTests.cs
    │   └── PlayPronunciationTests.cs
    ├── Infrastructure/                 # Test infrastructure
    │   ├── TestBase.cs                 # Base class with Testcontainers + WireMock setup
    │   └── WireMockFixtures/           # Reusable WireMock response fixtures
    │       ├── AnthropicFixtures.cs
    │       └── ElevenLabsFixtures.cs
    └── Helpers/
        └── VerifySettings.cs           # Verify snapshot configuration
```

**Structure Decision**: Multi-project solution with self-contained handler namespaces

**Key Design Principles**:

1. **Self-Contained Handlers**: Each handler namespace (`YetAnotherTranslator.Core.Handlers.{HandlerName}`) contains:

   - Handler class with business logic
   - Request/Result models specific to that handler
   - FluentValidation validator for the request
   - Note: Shared interfaces (ILlmProvider, ITtsProvider, etc.) are defined separately in Core.Interfaces namespace

2. **Interface Sharing**: Shared interfaces (ILlmProvider, ITtsProvider, IAudioPlayer, ISecretsProvider, IHistoryRepository) are defined in `YetAnotherTranslator.Core.Interfaces` namespace, separate from handler namespaces. This pragmatic choice avoids duplication while keeping handler business logic self-contained. Each handler namespace contains only its models, validators, and handler class—no interface definitions.

3. **Clean Boundaries**:

   - **Core** has zero dependencies on Infrastructure or CLI
   - **Infrastructure** references Core to implement interfaces
   - **CLI** references both Core (handlers) and Infrastructure (DI registration)

4. **Testability**:
   - Integration tests reference all projects
   - Real PostgreSQL via Testcontainers
   - Mocked external APIs via WireMock.Net
   - Verify for snapshot testing of complex results

This structure balances:

- **Modularity**: Each handler is independently understandable
- **Pragmatism**: Shared interfaces avoid duplication
- **Testability**: Clean boundaries enable comprehensive integration testing
- **Simplicity**: No over-engineering with commands, queries, or mediator patterns

## Complexity Tracking

**No violations to track** - Constitution Check passed all gates. The three-project structure (Core + Infrastructure + CLI) is justified as the minimum for clean architecture and testability.

