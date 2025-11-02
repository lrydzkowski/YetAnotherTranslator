# Implementation Tasks: Polish-English Translation CLI Tool

**Feature**: `001-polish-english-translator`
**Branch**: `001-polish-english-translator`
**Generated**: 2025-11-02

## Overview

This document provides a complete, dependency-ordered task list for implementing the Polish-English Translation CLI tool. Tasks are organized by user story to enable independent implementation and testing.

## Implementation Strategy

**MVP Scope**: User Story 6 (P0 - Configuration) + User Story 1 (P1 - Word Translation)

This minimal viable product provides:

- Configuration validation and startup
- Core word translation with linguistic details
- Table-formatted output in REPL
- Caching for performance
- History tracking

**Incremental Delivery**: After MVP, implement each user story independently in priority order (P2 → P3 → P4 → P5).

## Task Summary

**Total Tasks**: 90

- **Phase 1 (Setup)**: 10 tasks
- **Phase 2 (Foundational)**: 12 tasks
- **Phase 3 (US6 - Configuration P0)**: 8 tasks
- **Phase 4 (US1 - Word Translation P1)**: 18 tasks
- **Phase 5 (US2 - Text Translation P2)**: 11 tasks
- **Phase 6 (US3 - Grammar Review P3)**: 10 tasks
- **Phase 7 (US4 - Pronunciation P4)**: 13 tasks
- **Phase 8 (US5 - History P5)**: 5 tasks
- **Phase 9 (Polish)**: 3 tasks

**Parallel Opportunities**: 47 parallelizable tasks marked with [P]

## Dependencies

```text
User Story Completion Order:
┌────────────────────────────────────────────┐
│  Phase 1: Setup (project initialization)  │
└─────────────────┬──────────────────────────┘
                  │
                  ▼
┌────────────────────────────────────────────┐
│  Phase 2: Foundational (blocking prereqs) │
└─────────────────┬──────────────────────────┘
                  │
                  ▼
┌────────────────────────────────────────────┐
│  Phase 3: US6 - Configuration (P0)        │ ← MUST complete first
└─────────────────┬──────────────────────────┘
                  │
                  ▼
┌────────────────────────────────────────────┐
│  Phase 4: US1 - Word Translation (P1)     │ ← MVP with Phase 3
└─────────────────┬──────────────────────────┘
                  │
                  ├─► Phase 5: US2 - Text Translation (P2)      [Independent]
                  ├─► Phase 6: US3 - Grammar Review (P3)        [Independent]
                  ├─► Phase 7: US4 - Pronunciation (P4)         [Independent]
                  └─► Phase 8: US5 - History (P5)               [Independent]
                        │
                        ▼
                  ┌──────────────────────────────┐
                  │  Phase 9: Polish             │
                  └──────────────────────────────┘

Note: US2-US5 can be implemented in parallel after US1 is complete
```

## Parallel Execution Examples

### Phase 1 (Setup) - All Sequential

Tasks must run in order to establish project structure.

### Phase 2 (Foundational) - Parallel Groups

**Group 1** (run in parallel after T010):

- T011 [P]: Core interfaces
- T012 [P]: Domain models

**Group 2** (run in parallel after Group 1):

- T013 [P]: FluentValidation validators
- T014 [P]: Database entities
- T015 [P]: DbContext
- T016 [P]: EF Core migrations
- T017 [P]: Repository interfaces
- T018 [P]: Repository implementations

**Group 3** (run in parallel after T018):

- T019 [P]: Anthropic provider
- T020 [P]: ElevenLabs provider
- T021 [P]: Azure Key Vault provider

### Phase 3 (US6 - Configuration) - Parallel After T022

**Group** (run in parallel):

- T023 [P] [US6]: Configuration models
- T024 [P] [US6]: Configuration validators
- T025 [P] [US6]: Configuration loader
- T027 [P] [US6]: ConfigurationValidationHandler

### Phase 4 (US1 - Word Translation) - Multiple Parallel Groups

**Group 1** (run in parallel after T030):

- T031 [P] [US1]: TranslationResult model
- T032 [P] [US1]: Translation cache key generator
- T034 [P] [US1]: TranslateWordRequest validator

**Group 2** (run in parallel after T035):

- T036 [P] [US1]: ILlmProvider.TranslateWordAsync
- T037 [P] [US1]: IHistoryRepository methods

**Group 3** (run in parallel after T040):

- T041 [P] [US1]: CommandParser for word translation
- T042 [P] [US1]: OutputFormatter for tables

---

## Phase 1: Setup

**Goal**: Initialize .NET 10 solution with three projects following clean architecture.

**Independent Test**: Solution builds successfully, all projects reference correct dependencies.

### Tasks

- [ ] T001 Create .gitignore file using `dotnet new gitignore`
- [ ] T002 Create solution file YetAnotherTranslator.sln
- [ ] T003 Create YetAnotherTranslator.Core class library project (.NET 10) in src/YetAnotherTranslator.Core/
- [ ] T004 Create YetAnotherTranslator.Infrastructure class library project (.NET 10) in src/YetAnotherTranslator.Infrastructure/
- [ ] T005 Create YetAnotherTranslator.Cli console application project (.NET 10) in src/YetAnotherTranslator.Cli/
- [ ] T006 Create YetAnotherTranslator.Tests.Integration test project (.NET 10) in tests/YetAnotherTranslator.Tests.Integration/
- [ ] T007 Add project references: Infrastructure → Core, Cli → Core + Infrastructure, Tests → all projects
- [ ] T008 Install Core dependencies: FluentValidation 11.10.0, FluentValidation.DependencyInjectionExtensions 11.10.0
- [ ] T009 Install Infrastructure dependencies: Anthropic SDK, ElevenLabs-DotNet 3.6.0, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0, Microsoft.EntityFrameworkCore.Design 10.0.0, Azure.Security.KeyVault.Secrets 4.8.0, Azure.Identity 1.17.0
- [ ] T010 Install CLI dependencies: PrettyPrompt 4.0.0, Spectre.Console 0.49.1, Install Test dependencies: xUnit 2.9.0, xUnit.runner.visualstudio 2.8.2, Microsoft.NET.Test.Sdk 17.11.0, FluentAssertions 6.12.0, NSubstitute 5.1.0, Testcontainers.PostgreSql 3.10.0, WireMock.Net 1.6.4, Verify.Xunit 26.6.0

---

## Phase 2: Foundational Infrastructure

**Goal**: Implement core abstractions, domain models, database schema, and provider infrastructure that all user stories depend on.

**Independent Test**: All foundational components compile and integrate successfully. Database migrations run. Providers can be instantiated with DI.

### Tasks

- [ ] T011 [P] Create Core interfaces in src/YetAnotherTranslator.Core/Interfaces/: ILlmProvider.cs, ITtsProvider.cs, ISecretsProvider.cs, IHistoryRepository.cs
- [ ] T012 [P] Create domain model enums in src/YetAnotherTranslator.Core/Models/: CommandType.cs, LlmResponseMetadata.cs
- [ ] T013 [P] Create FluentValidation base validators in src/YetAnotherTranslator.Core/Validation/ (validator helper classes if needed)
- [ ] T014 [P] Create database entities in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/: HistoryEntryEntity.cs, TranslationCacheEntity.cs, TextTranslationCacheEntity.cs, PronunciationCacheEntity.cs
- [ ] T015 [P] Create TranslatorDbContext in src/YetAnotherTranslator.Infrastructure/Persistence/TranslatorDbContext.cs with entity configurations and indexes
- [ ] T016 [P] Create initial EF Core migration (001_InitialSchema) for all tables with indexes
- [ ] T017 [P] Create repository interfaces in src/YetAnotherTranslator.Core/Interfaces/ for cache and history operations
- [ ] T018 [P] Implement repository classes in src/YetAnotherTranslator.Infrastructure/Persistence/Repositories/ for all cache and history operations
- [ ] T019 [P] Implement AnthropicProvider in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicProvider.cs (skeleton with constructor, client initialization)
- [ ] T020 [P] Implement ElevenLabsProvider in src/YetAnotherTranslator.Infrastructure/Tts/ElevenLabsProvider.cs (skeleton with constructor, client initialization)
- [ ] T021 [P] Implement AzureKeyVaultProvider in src/YetAnotherTranslator.Infrastructure/Secrets/AzureKeyVaultProvider.cs with DefaultAzureCredential, in-memory caching, and error handling
- [ ] T022 Register all foundational services in src/YetAnotherTranslator.Cli/Program.cs with dependency injection

---

## Phase 3: User Story 6 - Configuration Validation (P0)

**Story Goal**: Validate configuration at startup and fail fast with clear error messages if configuration is missing or invalid.

**Why P0**: Prerequisite for all functionality - no operations can work without valid configuration.

**Independent Test**: Run application with various invalid configurations (missing file, malformed JSON, missing fields) and verify clear error messages. Run with valid config and verify successful startup.

### Tasks

- [ ] T023 [P] [US6] Create configuration models in src/YetAnotherTranslator.Infrastructure/Configuration/: ApplicationConfiguration.cs, SecretManagerConfiguration.cs, LlmProviderConfiguration.cs, TtsProviderConfiguration.cs, DatabaseConfiguration.cs
- [ ] T024 [P] [US6] Create configuration validators in src/YetAnotherTranslator.Infrastructure/Configuration/: ApplicationConfigurationValidator.cs, SecretManagerConfigurationValidator.cs, LlmProviderConfigurationValidator.cs, TtsProviderConfigurationValidator.cs, DatabaseConfigurationValidator.cs using FluentValidation
- [ ] T025 [P] [US6] Create ConfigurationLoader in src/YetAnotherTranslator.Infrastructure/Configuration/ConfigurationLoader.cs to load JSON from standard user config directory with JSON syntax error detection
- [ ] T026 [US6] Implement configuration loading in src/YetAnotherTranslator.Cli/Program.cs Main method with error handling for missing file, JSON syntax errors, and validation failures
- [ ] T027 [P] [US6] Create ConfigurationValidationHandler in src/YetAnotherTranslator.Core/Handlers/ConfigurationValidationHandler.cs to validate configuration and retrieve secrets from Azure Key Vault at startup
- [ ] T028 [US6] Update Program.cs to invoke ConfigurationValidationHandler at startup before entering REPL
- [ ] T029 [US6] Create integration test in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs to test all configuration validation scenarios (missing file, malformed JSON, missing fields, invalid Key Vault, valid config)
- [ ] T030 [US6] Add error message formatting in src/YetAnotherTranslator.Cli/Display/ErrorFormatter.cs using Spectre.Console for clear, user-friendly error display

---

## Phase 4: User Story 1 - Word Translation (P1) [MVP]

**Story Goal**: Translate individual words between Polish and English with detailed linguistic information (part of speech, countability, examples) displayed in table format.

**Why P1**: Core value proposition - most frequent use case for language learners.

**Independent Test**: Translate "kot" (Polish → English) and "dog" (English → Polish), verify table output with multiple translations, parts of speech, countability, and examples. Test with non-existent word and verify error handling.

### Tasks

- [ ] T031 [P] [US1] Create TranslationResult domain model in src/YetAnotherTranslator.Core/Models/TranslationResult.cs with nested Translation type (rank, word, part of speech, countability, examples list)
- [ ] T032 [P] [US1] Implement cache key generation for translations in src/YetAnotherTranslator.Infrastructure/Persistence/CacheKeyGenerator.cs using SHA256 hash
- [ [ ] T033 [US1] Update TranslationCacheEntity mapping in TranslatorDbContext to use cache_key as unique index
- [ ] T034 [P] [US1] Create TranslateWordRequest record and TranslateWordRequestValidator in src/YetAnotherTranslator.Core/Validation/TranslateWordRequestValidator.cs with FluentValidation rules (word length, language validation)
- [ ] T035 [US1] Implement ILlmProvider.TranslateWordAsync signature in src/YetAnotherTranslator.Core/Interfaces/ILlmProvider.cs
- [ ] T036 [P] [US1] Implement AnthropicProvider.TranslateWordAsync in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicProvider.cs with Claude API call, structured prompt for translations with linguistic metadata, and response parsing to TranslationResult
- [ ] T037 [P] [US1] Add cache lookup/save methods to IHistoryRepository in src/YetAnotherTranslator.Core/Interfaces/IHistoryRepository.cs (GetCachedTranslationAsync, SaveTranslationCacheAsync)
- [ ] T038 [US1] Implement cache methods in repository implementation in src/YetAnotherTranslator.Infrastructure/Persistence/Repositories/
- [ ] T039 [US1] Create TranslateWordHandler in src/YetAnotherTranslator.Core/Handlers/TranslateWordHandler.cs with validation, cache check, LLM call, cache save, history save logic
- [ ] T040 [US1] Register TranslateWordHandler in Program.cs DI container
- [ ] T041 [P] [US1] Implement command parsing for word translation commands (/t, /translate, /tp, /te) in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs with --no-cache option support
- [ ] T042 [P] [US1] Implement table formatter for word translations in src/YetAnotherTranslator.Cli/Display/OutputFormatter.cs using Spectre.Console with columns: #, Translation, Part of Speech, Countability, Example
- [ ] T043 [US1] Create ReplEngine in src/YetAnotherTranslator.Cli/Repl/ReplEngine.cs using PrettyPrompt with command parsing and handler invocation for word translation commands
- [ ] T044 [US1] Wire up word translation flow in Program.cs to invoke TranslateWordHandler from REPL
- [ ] T045 [US1] Create integration test in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs with Testcontainers PostgreSQL and WireMock.Net for Anthropic API, using Verify for snapshot assertions
- [ ] T046 [US1] Add test cases for Polish → English translation ("kot" → multiple translations with linguistic data) using await Verify(result)
- [ ] T047 [US1] Add test cases for English → Polish translation ("dog" → multiple translations) using await Verify(result)
- [ ] T048 [US1] Add test case for word with multiple meanings ("zamek" → castle/zipper/lock) using await Verify(result)

---

## Phase 5: User Story 2 - Text Translation (P2)

**Story Goal**: Translate text snippets (up to 5000 characters) between Polish and English with automatic language detection.

**Why P2**: Second most common use case - users need to understand larger chunks of text beyond individual words.

**Independent Test**: Translate Polish sentence to English and English paragraph to Polish. Test character limit (5001 characters) and verify error. Test language auto-detection.

### Tasks

- [ ] T049 [P] [US2] Create TextTranslationResult domain model in src/YetAnotherTranslator.Core/Models/TextTranslationResult.cs with source text, translated text, languages, character count
- [ ] T050 [P] [US2] Create TranslateTextRequest record and TranslateTextRequestValidator in src/YetAnotherTranslator.Core/Validation/TranslateTextRequestValidator.cs with character limit validation (max 5000)
- [ ] T051 [P] [US2] Implement ILlmProvider.TranslateTextAsync signature in src/YetAnotherTranslator.Core/Interfaces/ILlmProvider.cs
- [ ] T052 [P] [US2] Implement AnthropicProvider.TranslateTextAsync in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicProvider.cs with text translation prompt
- [ ] T053 [P] [US2] Add language detection method ILlmProvider.DetectLanguageAsync in src/YetAnotherTranslator.Core/Interfaces/ILlmProvider.cs
- [ ] T054 [P] [US2] Implement AnthropicProvider.DetectLanguageAsync in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicProvider.cs
- [ ] T055 [US2] Add text translation cache methods to IHistoryRepository and implement in repository
- [ ] T056 [US2] Create TranslateTextHandler in src/YetAnotherTranslator.Core/Handlers/TranslateTextHandler.cs with validation, cache, language detection (if auto), LLM call logic
- [ ] T057 [US2] Register TranslateTextHandler in Program.cs DI container
- [ ] T058 [US2] Add text translation command parsing (/tt, /ttp, /tte) to CommandParser with support for escaped newlines (\n)
- [ ] T059 [US2] Add plain text output formatter for text translations in OutputFormatter using Spectre.Console
- [ ] T060 [US2] Wire up text translation flow in ReplEngine
- [ ] T061 [US2] Create integration test in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs with test cases for Polish → English, English → Polish, character limit validation, auto-detection using Verify for snapshot assertions

---

## Phase 6: User Story 3 - Grammar Review (P3)

**Story Goal**: Review English text for grammar errors and vocabulary improvements with categorized feedback.

**Why P3**: Adds educational value beyond translation - helps users improve their English writing.

**Independent Test**: Submit English text with known grammar errors (e.g., "I are going to store") and verify system identifies errors with corrections and explanations. Submit correct text and verify no errors found.

### Tasks

- [ ] T062 [P] [US3] Create GrammarReviewResult domain model in src/YetAnotherTranslator.Core/Models/GrammarReviewResult.cs with IsCorrect flag, GrammarIssue list, VocabularySuggestion list
- [ ] T063 [P] [US3] Create ReviewGrammarRequest record and ReviewGrammarRequestValidator in src/YetAnotherTranslator.Core/Validation/ReviewGrammarRequestValidator.cs with text length validation (max 5000)
- [ ] T064 [P] [US3] Implement ILlmProvider.ReviewGrammarAsync signature in src/YetAnotherTranslator.Core/Interfaces/ILlmProvider.cs
- [ ] T065 [P] [US3] Implement AnthropicProvider.ReviewGrammarAsync in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicProvider.cs with grammar review prompt requesting categorized feedback
- [ ] T066 [US3] Add grammar review cache methods to IHistoryRepository and implement in repository
- [ ] T067 [US3] Create ReviewGrammarHandler in src/YetAnotherTranslator.Core/Handlers/ReviewGrammarHandler.cs with validation, cache, LLM call logic
- [ ] T068 [US3] Register ReviewGrammarHandler in Program.cs DI container
- [ ] T069 [US3] Add grammar review command parsing (/r, /review) to CommandParser
- [ ] T070 [US3] Add grammar review output formatter in OutputFormatter with sections for Grammar Issues and Vocabulary Suggestions using Spectre.Console
- [ ] T071 [US3] Wire up grammar review flow in ReplEngine
- [ ] T072 [US3] Create integration test in tests/YetAnotherTranslator.Tests.Integration/Features/ReviewGrammarTests.cs with test cases for text with errors, correct text, mixed issues

---

## Phase 7: User Story 4 - Pronunciation (P4)

**Story Goal**: Play English pronunciation audio for words and phrases with optional part-of-speech disambiguation.

**Why P4**: Valuable for language learning but requires audio infrastructure - lower priority than core translation.

**Independent Test**: Request pronunciation for "hello" and verify audio plays. Request "record" as noun vs verb and verify different pronunciations. Test phrase pronunciation. Test audio playback failure handling.

### Tasks

- [ ] T073 [P] [US4] Create PronunciationResult domain model in src/YetAnotherTranslator.Core/Models/PronunciationResult.cs with text, part of speech, audio data (byte[]), format, voice ID
- [ ] T074 [P] [US4] Create PlayPronunciationRequest record and PlayPronunciationRequestValidator in src/YetAnotherTranslator.Core/Validation/PlayPronunciationRequestValidator.cs with text length validation (max 500)
- [ ] T074a [US4] Update PlayPronunciationRequest to include optional PartOfSpeech parameter and validate it flows through to TTS prompt context (for pronunciation disambiguation like "record" noun vs verb)
- [ ] T075 [P] [US4] Implement ITtsProvider.GenerateSpeechAsync signature in src/YetAnotherTranslator.Core/Interfaces/ITtsProvider.cs with text and optional partOfSpeech parameters
- [ ] T076 [P] [US4] Implement ElevenLabsProvider.GenerateSpeechAsync in src/YetAnotherTranslator.Infrastructure/Tts/ElevenLabsProvider.cs to generate MP3 audio; if partOfSpeech provided, include it in SSML or text context for pronunciation variation (note: ElevenLabs may have limited support for this; document limitation if needed)
- [ ] T077 [P] [US4] Add pronunciation cache methods to IHistoryRepository (cache key includes text + part of speech + voice ID) and implement in repository
- [ ] T078 [P] [US4] Install PortAudioSharp or Bufdio package for cross-platform audio playback
- [ ] T079 [P] [US4] Create IAudioPlayer interface in src/YetAnotherTranslator.Core/Interfaces/IAudioPlayer.cs
- [ ] T080 [P] [US4] Implement PortAudioPlayer in src/YetAnotherTranslator.Infrastructure/Audio/PortAudioPlayer.cs for cross-platform audio playback
- [ ] T081 [US4] Create PlayPronunciationHandler in src/YetAnotherTranslator.Core/Handlers/PlayPronunciationHandler.cs with validation, cache, TTS call, audio playback logic
- [ ] T082 [US4] Register PlayPronunciationHandler and IAudioPlayer in Program.cs DI container
- [ ] T083 [US4] Add pronunciation command parsing (/p, /playback) to CommandParser with optional part of speech parameter
- [ ] T084 [US4] Add pronunciation playback UI in OutputFormatter showing playback progress and completion using Spectre.Console.Status
- [ ] T085 [US4] Wire up pronunciation flow in ReplEngine
- [ ] T086 [US4] Create integration test in tests/YetAnotherTranslator.Tests.Integration/Features/PlayPronunciationTests.cs (note: mock audio playback in tests) with test cases for word, phrase, part of speech disambiguation

---

## Phase 8: User Story 5 - History (P5)

**Story Goal**: Display operation history in reverse chronological order with timestamps, command types, and summaries.

**Why P5**: Adds convenience and learning reinforcement but not essential for core functionality.

**Independent Test**: Perform multiple operations (word translation, text translation, grammar review), request history, verify all operations appear with correct timestamps and details. Test empty history. Test pagination/limits.

### Tasks

- [ ] T087 [US5] Create GetHistoryHandler in src/YetAnotherTranslator.Core/Handlers/GetHistoryHandler.cs to query history_entries table ordered by timestamp DESC with configurable limit (default 10, max 100)
- [ ] T088 [US5] Register GetHistoryHandler in Program.cs DI container
- [ ] T089 [US5] Add history command parsing (/history, /hist) to CommandParser with optional count parameter
- [ ] T090 [US5] Add history table formatter in OutputFormatter with columns: Timestamp, Command Type, Input (truncated), Summary using Spectre.Console
- [ ] T091 [US5] Wire up history flow in ReplEngine
- [ ] T092 [US5] Create integration test in tests/YetAnotherTranslator.Tests.Integration/Features/HistoryTests.cs with test cases for multiple operations, empty history, pagination

---

## Phase 9: Polish & Cross-Cutting Concerns

**Goal**: Add utility commands, help system, and cross-cutting features that enhance usability.

### Tasks

- [ ] T093 [P] Add help command (/h, /help) to CommandParser with comprehensive command reference displayed using Spectre.Console
- [ ] T094 [P] Add clear command (/c, /clear) to CommandParser to clear terminal screen
- [ ] T095 [P] Add quit command (/q, /quit) to CommandParser to exit REPL with goodbye message
- [ ] T096 Add welcome message and command prompt to ReplEngine using Spectre.Console
- [ ] T097 Add error handling middleware in ReplEngine to catch ValidationException, API errors, database errors with user-friendly error messages via Spectre.Console
- [ ] T098 Add retry logic with exponential backoff for API calls in AnthropicProvider and ElevenLabsProvider (3 retries, 1s → 2s → 4s delays)
- [ ] T099 Create end-to-end integration test in tests/YetAnotherTranslator.Tests.Integration/E2ETests.cs that simulates full REPL workflow: start app → translate word → translate text → review grammar → play pronunciation → view history → quit

---

## Validation Checklist

All tasks follow the required format:

- ✓ Every task has checkbox prefix `- [ ]`
- ✓ Every task has sequential Task ID (T001-T099)
- ✓ Parallelizable tasks marked with [P]
- ✓ User story tasks marked with story label [US1]-[US6]
- ✓ Every task includes specific file path
- ✓ Tasks organized by user story phase
- ✓ Clear action verbs (Create, Implement, Add, Update, etc.)

## Notes for Implementation

### Testing with Testcontainers + WireMock

Each integration test should:

1. Start PostgreSQL container with Testcontainers
2. Start WireMock server for external APIs (Anthropic, ElevenLabs, Azure Key Vault)
3. Build service provider with test configuration pointing to mocked APIs
4. Execute test scenario
5. Clean up containers

Example test structure in `tests/YetAnotherTranslator.Tests.Integration/Infrastructure/TestBase.cs`:

```csharp
public class TestBase : IAsyncLifetime
{
    protected PostgreSqlContainer PostgresContainer;
    protected WireMockServer AnthropicMock;
    protected WireMockServer ElevenLabsMock;
    protected WireMockServer KeyVaultMock;
    protected IServiceProvider ServiceProvider;

    public async Task InitializeAsync()
    {
        // Start containers and mocks
        // Configure DI container with test settings
    }

    public async Task DisposeAsync()
    {
        // Clean up
    }
}
```

### Handler Pattern

All handlers follow this pattern:

1. Inject dependencies (validator, provider, repository)
2. `HandleAsync` method with parameters
3. Validate input with FluentValidation
4. Check cache (if applicable)
5. Call external provider
6. Save to cache
7. Save to history
8. Return result

### Configuration File Location

- Windows: `%APPDATA%\translator\config.json`
- Linux/macOS: `~/.config/translator/config.json`

### Secret References in config.json

Configuration file stores **references** to secrets (not actual values):

```json
{
  "secretManager": {
    "type": "azure-keyvault",
    "keyVaultUrl": "https://vault.vault.azure.net"
  },
  "llmProvider": {
    "type": "anthropic",
    "secretReference": "anthropic-api-key"
  }
}
```

Actual API keys stored in Azure Key Vault.

### Error Handling

All handlers must handle:

- Validation errors (FluentValidation.ValidationException)
- API errors (connection timeout, rate limiting, authentication failures)
- Database errors (connection failures, constraint violations)
- Cache misses (not errors - expected behavior)

Error messages displayed to user via Spectre.Console with context and troubleshooting suggestions.

---

**End of Tasks Document**

