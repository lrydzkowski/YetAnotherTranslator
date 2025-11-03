# Implementation Plan: Polish-English Translation CLI Tool

**Branch**: `001-polish-english-translator` | **Date**: 2025-11-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-polish-english-translator/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

A REPL-style command-line interface for Polish-English translation featuring word translation with detailed linguistic metadata (parts of speech, countability, CMU Arpabet phonetic transcriptions), text translation, English grammar review, pronunciation playback via text-to-speech, and persistent operation history. Built with .NET 10, using Anthropic Claude for LLM operations, ElevenLabs for TTS, PostgreSQL for history/cache storage, and Azure Key Vault for secure credential management.

## Technical Context

**Language/Version**: .NET 10 (C#)
**Primary Dependencies**: Official Anthropic SDK, PrettyPrompt, Spectre.Console, FluentValidation, EF Core, Azure SDK (Key Vault), ElevenLabs SDK, PortAudioSharp, xUnit, NSubstitute, Testcontainers, WireMock.Net, Verify
**Storage**: PostgreSQL with EF Core for operation history and pronunciation cache
**Testing**: xUnit with integration tests only (Testcontainers for PostgreSQL, WireMock.Net for external API mocking, NSubstitute for interface mocking, Verify for snapshot testing)
**Target Platform**: Cross-platform CLI (Windows, macOS, Linux)
**Project Type**: Multi-project solution (Core + Infrastructure + CLI)
**Performance Goals**: Word translation <3s (SC-001), Pronunciation playback <2s (SC-006), History retrieval <1s (SC-008), Configuration validation <1s (SC-011)
**Constraints**: Text snippet translation max 5000 characters (FR-024/SC-004), Offline cache fallback for translations/pronunciations, UTF-8 support for Polish diacritics
**Scale/Scope**: Single-user CLI tool, unlimited operation history storage, support for 4 main operations (word translation, text translation, grammar review, pronunciation playback)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Single Responsibility ✅
- **TranslateWordHandler**: Word translation logic only
- **TranslateTextHandler**: Text translation logic only
- **ReviewGrammarHandler**: Grammar review logic only
- **PlayPronunciationHandler**: Pronunciation playback logic only
- **Handlers**: One clear purpose per handler class

### Testability ✅
- **Integration tests only**: Real PostgreSQL via Testcontainers, mocked external APIs via WireMock.Net
- **Dependency injection**: All dependencies injected via .NET DI container
- **Interfaces**: Core defines interfaces, Infrastructure implements (ILlmProvider, ITtsProvider, ISecretsProvider, IRepository)
- **No unit tests**: Focus on end-to-end feature testing with real database behavior

### CLI Standards ✅
- **Arguments**: REPL commands via PrettyPrompt input
- **Output**: Results to stdout via Spectre.Console
- **Errors**: Error messages to stderr
- **Exit codes**: 0 for success, non-zero for configuration/startup errors

### Error Handling ✅
- **Fail fast**: Configuration validation at startup before entering REPL
- **Descriptive messages**: Clear error messages with context for all failure scenarios (FR-016, FR-030, FR-036, FR-041, FR-042, FR-043)
- **No silent failures**: All errors displayed to user with actionable guidance
- **Appropriate level**: Validation in handlers via FluentValidation, business errors in handlers, infrastructure errors in providers

**Offline Detection and Cache Fallback**:

The system detects offline scenarios through exception handling rather than proactive connectivity checks:

1. **LLM Provider Unavailable**:
   - Catch `HttpRequestException` with inner `SocketException` (no network)
   - Catch `TaskCanceledException` from timeout (5 second timeout for API calls)
   - Catch provider-specific exceptions (e.g., `AnthropicException` for API errors)

2. **Cache Fallback Behavior**:
   - On network exception → Check cache for matching entry
   - Cache hit → Return cached result with "(cached)" indicator in UI
   - Cache miss → Display error: "Unable to connect to translation service. No cached result available. Please check your internet connection."

3. **Azure Key Vault Unavailable** (startup only):
   - Catch `RequestFailedException` from Azure SDK
   - Fail fast at startup (no cache fallback for credentials)
   - Display clear error: "Failed to retrieve credentials from Azure Key Vault: {error}. Run 'az login' and verify Key Vault access."

**Implementation Notes**:
- Do not implement proactive connectivity checks (adds complexity, violates Simplicity principle)
- Rely on exception handling at operation level
- Cache keys based on SHA256 hash ensure exact match for offline retrieval
- Offline mode is automatic and transparent to user (except error messages when no cache available)

### Simplicity ✅
- **Handler pattern**: Simple handler classes with business logic methods, no MediatR complexity
- **Direct invocation**: Handlers called directly from CLI layer
- **Cache-Aside pattern**: History table serves as cache with bypass option
- **No clever tricks**: Explicit dependencies, clear data flow

### Technical Standards ✅
- **.NET 10**: As specified
- **FluentValidation**: For input validation
- **Integration tests**: xUnit, Testcontainers, Verify, WireMock.Net, NSubstitute
- **Return early pattern**: To be followed in all implementations

### Complexity Justification

**Multi-project structure (3 projects)**: Required to maintain Clean Architecture boundaries
- **Core**: Business logic and interfaces (handler classes, domain models, validators)
- **Infrastructure**: External integrations (Anthropic SDK, ElevenLabs SDK, Azure Key Vault SDK, EF Core DbContext)
- **CLI**: User interface (PrettyPrompt REPL, Spectre.Console display)

**Rationale**: Prevents accidental dependencies from Core → Infrastructure. Core defines interfaces, Infrastructure implements them. CLI depends on both. Standard Clean Architecture pattern for maintainability and testability.

**Alternative rejected**: Single project would allow business logic to directly reference external SDKs, violating dependency inversion and making testing harder.

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
├── YetAnotherTranslator.Core/          # Business logic
│   ├── Handlers/                       # Business logic handlers
│   │   ├── TranslateWordHandler.cs
│   │   ├── TranslateTextHandler.cs
│   │   ├── ReviewGrammarHandler.cs
│   │   └── PlayPronunciationHandler.cs
│   ├── Interfaces/                     # Abstractions for Infrastructure
│   │   ├── ILlmProvider.cs
│   │   ├── ITtsProvider.cs
│   │   ├── ISecretsProvider.cs
│   │   └── IHistoryRepository.cs
│   ├── Models/                         # Domain models
│   │   ├── TranslationRequest.cs
│   │   ├── TranslationResult.cs
│   │   ├── GrammarReviewRequest.cs
│   │   ├── GrammarReviewResult.cs
│   │   ├── PronunciationRequest.cs
│   │   └── HistoryEntry.cs
│   └── Validation/                     # FluentValidation validators
│       ├── TranslateWordRequestValidator.cs
│       ├── TranslateTextRequestValidator.cs
│       ├── ReviewGrammarRequestValidator.cs
│       └── PlayPronunciationRequestValidator.cs
│
├── YetAnotherTranslator.Infrastructure/  # External integrations
│   ├── Llm/                            # Official Anthropic SDK provider
│   │   └── AnthropicLlmProvider.cs
│   ├── Tts/                            # ElevenLabs provider
│   │   └── ElevenLabsTtsProvider.cs
│   ├── Secrets/                        # Azure Key Vault provider
│   │   └── AzureKeyVaultSecretsProvider.cs
│   ├── Persistence/                    # EF Core DbContext, repositories
│   │   ├── TranslatorDbContext.cs
│   │   ├── HistoryRepository.cs
│   │   └── Migrations/
│   └── Configuration/                  # Config validation
│       ├── AppConfiguration.cs
│       └── ConfigurationValidator.cs
│
└── YetAnotherTranslator.Cli/            # User interface
    ├── Repl/                           # PrettyPrompt integration
    │   ├── ReplEngine.cs
    │   └── CommandParser.cs
    ├── Display/                        # Spectre.Console formatting
    │   ├── TranslationTableFormatter.cs
    │   ├── GrammarReviewFormatter.cs
    │   └── HistoryFormatter.cs
    └── Program.cs                      # Entry point

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
```

**Structure Decision**: Multi-project solution following Clean Architecture principles. Core project contains business logic and defines interfaces. Infrastructure project implements external integrations (Anthropic SDK, ElevenLabs SDK, Azure Key Vault, EF Core). CLI project handles user interaction via PrettyPrompt REPL and Spectre.Console display. Integration tests only, using Testcontainers for real PostgreSQL and WireMock.Net for external API mocking.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

*No violations - all complexity justified in Constitution Check section above.*
