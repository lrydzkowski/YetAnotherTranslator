# Tasks: Polish-English Translation CLI Tool

**Input**: Design documents from `/specs/001-polish-english-translator/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Integration tests are included as per testing strategy (integration tests only with Testcontainers, WireMock.Net, Verify).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Multi-project solution structure:

- **Core**: `src/YetAnotherTranslator.Core/`
- **Infrastructure**: `src/YetAnotherTranslator.Infrastructure/`
- **CLI**: `src/YetAnotherTranslator.Cli/`
- **Tests**: `tests/YetAnotherTranslator.Tests.Integration/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution file YetAnotherTranslator.sln in repository root
- [X] T002 Create Core project src/YetAnotherTranslator.Core/YetAnotherTranslator.Core.csproj targeting .NET 10
- [X] T003 [P] Create Infrastructure project src/YetAnotherTranslator.Infrastructure/YetAnotherTranslator.Infrastructure.csproj targeting .NET 10
- [X] T004 [P] Create CLI project src/YetAnotherTranslator.Cli/YetAnotherTranslator.Cli.csproj targeting .NET 10
- [X] T005 [P] Create Integration test project tests/YetAnotherTranslator.Tests.Integration/YetAnotherTranslator.Tests.Integration.csproj
- [X] T006 Configure project references: Infrastructure references Core, CLI references Core and Infrastructure, Tests reference all
- [X] T007 [P] Add Core dependencies: FluentValidation, FluentValidation.DependencyInjectionExtensions
- [X] T008 [P] Add Infrastructure dependencies: Anthropic SDK, ElevenLabs-DotNet, Azure.Security.KeyVault.Secrets, Azure.Identity, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, PortAudioSharp
- [X] T009 [P] Add CLI dependencies: PrettyPrompt, Spectre.Console
- [X] T010 [P] Add Test dependencies: xUnit, xUnit.runner.visualstudio, Microsoft.NET.Test.Sdk, FluentAssertions, NSubstitute, Testcontainers.PostgreSql, WireMock.Net, Verify.Xunit

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T011 Create directory structure: Core/{Handlers,Interfaces}, Infrastructure/{Llm,Tts,Secrets,Persistence,Configuration}, CLI/{Repl,Display}
  - Note: Core/Models and Core/Validation directories NOT created; handlers are self-contained with models/validators in their own namespaces
  - Shared interfaces live in Core/Interfaces separate from handler namespaces
- [X] T012 [P] Create ILlmProvider interface in src/YetAnotherTranslator.Core/Interfaces/ILlmProvider.cs
- [X] T013 [P] Create ITtsProvider interface in src/YetAnotherTranslator.Core/Interfaces/ITtsProvider.cs
- [X] T014 [P] Create ISecretsProvider interface in src/YetAnotherTranslator.Core/Interfaces/ISecretsProvider.cs
- [X] T015 [P] Create IHistoryRepository interface in src/YetAnotherTranslator.Core/Interfaces/IHistoryRepository.cs
- [X] T016 [P] Create CommandType enum in src/YetAnotherTranslator.Core/Handlers/GetHistory/CommandType.cs
- [X] T017 Create TranslatorDbContext in src/YetAnotherTranslator.Infrastructure/Persistence/TranslatorDbContext.cs with DbSets for HistoryEntry, TranslationCache, TextTranslationCache, PronunciationCache
- [X] T018 Create initial EF Core migration InitialSchema using dotnet ef migrations add
  - Entities included: HistoryEntryEntity, TranslationCacheEntity, TextTranslationCacheEntity, PronunciationCacheEntity, LlmResponseCacheEntity
  - TranslationCacheEntity fields: Id (uuid PK), CacheKey (varchar unique index, SHA256 hash), SourceLanguage (varchar), TargetLanguage (varchar), InputText (text), ResultJson (jsonb with translations array including cmuArpabet field), CreatedAt (timestamp with time zone)
  - TextTranslationCacheEntity fields: Id (uuid PK), CacheKey (varchar unique index), SourceLanguage (varchar), TargetLanguage (varchar), InputText (text), TranslatedText (text), CreatedAt (timestamp with time zone)
  - PronunciationCacheEntity fields: Id (uuid PK), CacheKey (varchar unique index, SHA256 of text+partOfSpeech), Text (text), PartOfSpeech (varchar nullable), AudioData (bytea), VoiceId (varchar), CreatedAt (timestamp with time zone)
  - LlmResponseCacheEntity fields: Id (uuid PK), CacheKey (varchar unique index), OperationType (varchar), RequestHash (varchar), ResponseJson (jsonb), CreatedAt (timestamp with time zone), ExpiresAt (timestamp with time zone nullable)
  - Indexes: Primary keys, unique indexes on all cache_key columns, timestamp indexes for history queries, index on LlmResponseCacheEntity.ExpiresAt for cleanup queries
  - Cache expiration: All cache entities have 30-day expiration per FR-046; cache retrieval MUST check CreatedAt/ExpiresAt and treat expired entries as cache misses
  - Cache cleanup strategy: Manual cleanup via SQL query (DELETE FROM llm_response_cache WHERE expires_at < NOW()) or scheduled background task; cleanup is optional (caches grow unbounded until manually cleaned)
  - Command: `dotnet ef migrations add InitialSchema --project src/YetAnotherTranslator.Infrastructure --startup-project src/YetAnotherTranslator.Cli`
  - Verification checklist:
    * Run migration and verify all 5 tables created (HistoryEntry, TranslationCache, TextTranslationCache, PronunciationCache, LlmResponseCache)
    * Verify column types match specification (uuid PK, jsonb for structured data, bytea for audio, timestamp with time zone)
    * Verify unique indexes exist on all cache_key columns
    * Test migration rollback: `dotnet ef database update 0` succeeds and drops all tables
    * Re-apply migration to verify idempotency
- [X] T019 [P] Create TestBase class in tests/YetAnotherTranslator.Tests.Integration/Infrastructure/TestBase.cs with Testcontainers PostgreSQL setup and WireMock server initialization
  - Implement IAsyncLifetime or IDisposable for proper Testcontainers cleanup (stop and dispose PostgreSQL container, stop WireMock server)
  - Ensure each test gets isolated database state (either fresh container or database reset between tests)
- [X] T020 [P] Create WireMock fixtures directory tests/YetAnotherTranslator.Tests.Integration/Infrastructure/WireMockFixtures/
- [X] T021 [P] Setup dependency injection container configuration in src/YetAnotherTranslator.Cli/Program.cs
- [X] T022 Create error handling infrastructure and base exception types in src/YetAnotherTranslator.Core/Exceptions/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 6 - Configuration Validation (Priority: P0) 🎯 PREREQUISITE

**Goal**: Validate configuration at startup before any translation features can function

**Independent Test**: Run application with missing/incomplete/invalid config and verify error messages

### Integration Tests for User Story 6

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T023 [P] [US6] Integration test for missing config file in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T024 [P] [US6] Integration test for malformed JSON config in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T024a [P] [US6] Integration test verifying malformed JSON error includes line and column numbers in output (validates FR-029 requirement) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T025 [P] [US6] Integration test for incomplete config (missing LLM provider) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T026 [P] [US6] Integration test for invalid Azure Key Vault URL in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T027 [P] [US6] Integration test for valid config successfully launches REPL in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T027a [P] [US6] Integration test for Azure Key Vault network failure (timeout) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T027b [P] [US6] Integration test for Azure Key Vault permission denied (403) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T027c [P] [US6] Integration test for secret not found (404) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [x] T027d [P] [US6] Integration test for LLM provider connection failure at startup (validates User Story 6 Acceptance Scenario 8) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs

### Implementation for User Story 6

- [x] T028 [P] [US6] Create ApplicationConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/ApplicationConfiguration.cs
- [x] T029 [P] [US6] Create SecretManagerConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/SecretManagerConfiguration.cs
- [x] T030 [P] [US6] Create LlmProviderConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/LlmProviderConfiguration.cs
- [x] T031 [P] [US6] Create TtsProviderConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/TtsProviderConfiguration.cs
- [x] T032 [P] [US6] Create DatabaseConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/DatabaseConfiguration.cs
- [x] T033 [US6] Create ConfigurationValidator with FluentValidation in src/YetAnotherTranslator.Infrastructure/Configuration/ConfigurationValidator.cs
- [x] T034 [US6] Implement configuration loading from standard user config directory in src/YetAnotherTranslator.Cli/Program.cs
- [x] T035 [US6] Implement startup validation that fails fast with clear error messages and exits with non-zero status code on failures in src/YetAnotherTranslator.Cli/Program.cs

**Checkpoint**: Configuration validation complete - application can safely start with valid config

---

## Phase 4: User Story 1 - Word Translation with Detailed Linguistic Information (Priority: P1) 🎯 MVP

**Goal**: Translate individual words between Polish and English with linguistic metadata (parts of speech, countability, CMU Arpabet for English translations, example sentences)

**Independent Test**: Translate a Polish word and verify output shows multiple translations ranked by popularity with all linguistic details

### Integration Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T036 [P] [US1] Integration test for Polish word translation with CMU Arpabet in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T037 [P] [US1] Integration test for English word translation (no CMU Arpabet) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T038 [P] [US1] Integration test for word with multiple meanings in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T039 [P] [US1] Integration test for word with pronunciation variants (part-of-speech-specific CMU Arpabet) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T040 [P] [US1] Integration test for cache hit scenario in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T041 [P] [US1] Integration test for --no-cache option in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T042 [P] [US1] Integration test for invalid word (empty, too long) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T042a [P] [US1] Integration test verifying CMU Arpabet is saved to TranslationCache and retrieved from cache on subsequent call in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T042b [P] [US1] Integration test verifying CMU Arpabet failure (null) is saved to cache and subsequent calls display "N/A" from cache in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T042c [P] [US1] Integration test for offline CMU Arpabet cache retrieval: Perform translation with network available (saves to cache), then simulate offline by stopping WireMock server, verify cached translation with CMU Arpabet displays correctly (validates FR-006b offline retrieval requirement) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T042d [P] [US1] Integration test for UTF-8 rendering of Polish diacritics in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs

### Implementation for User Story 1

- [x] T043 [P] [US1] Create Translation nested model in src/YetAnotherTranslator.Core/Handlers/TranslateWord/Translation.cs with Rank, Word, PartOfSpeech, Countability, CmuArpabet, Examples fields
- [x] T044 [P] [US1] Create TranslationResult model in src/YetAnotherTranslator.Core/Handlers/TranslateWord/TranslationResult.cs
- [x] T045 [P] [US1] Create TranslateWordRequest record in src/YetAnotherTranslator.Core/Handlers/TranslateWord/TranslateWordRequest.cs
- [x] T046 [P] [US1] Create LlmResponseMetadata model in src/YetAnotherTranslator.Core/Models/LlmResponseMetadata.cs
- [x] T047 [US1] Create TranslateWordRequestValidator in src/YetAnotherTranslator.Core/Handlers/TranslateWord/TranslateWordValidator.cs
- [x] T048 [P] [US1] Create HistoryEntryEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/HistoryEntryEntity.cs (already existed from Phase 2)
- [x] T049 [P] [US1] Create TranslationCacheEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/TranslationCacheEntity.cs (already existed from Phase 2)
- [x] T050 [US1] Update TranslatorDbContext with entity configurations and indexes in src/YetAnotherTranslator.Infrastructure/Persistence/TranslatorDbContext.cs (already existed from Phase 2)
- [x] T050a [US1] Implement DetectLanguageAsync method in AnthropicLlmProvider in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs
- [x] T051 [US1] Implement AnthropicLlmProvider with TranslateWordAsync, TranslateTextAsync, ReviewGrammarAsync methods in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs
- [ ] T051a [US1] Implement validation task for AnthropicProvider in tests/YetAnotherTranslator.Tests.Integration/Features/AnthropicProviderValidationTests.cs (deferred - covered by TranslateWordTests)
- [x] T052 [US1] Implement AzureKeyVaultSecretsProvider in src/YetAnotherTranslator.Infrastructure/Secrets/AzureKeyVaultSecretsProvider.cs
- [x] T053 [US1] Implement HistoryRepository with cache-aside pattern in src/YetAnotherTranslator.Infrastructure/Persistence/HistoryRepository.cs
- [x] T054 [US1] Implement cache key generation using SHA256 hash in src/YetAnotherTranslator.Infrastructure/Persistence/CacheKeyGenerator.cs
- [x] T055 [US1] Implement TranslateWordHandler with validation, caching, LLM call, history save in src/YetAnotherTranslator.Core/Handlers/TranslateWord/TranslateWordHandler.cs
- [x] T056 [US1] Create TranslationTableFormatter for Spectre.Console table output with CMU Arpabet column (Polish→English only) in src/YetAnotherTranslator.Cli/Display/TranslationTableFormatter.cs
- [x] T056a [P] [US1] Integration test verifying UTF-8 rendering of Polish diacritics (covered by T042d) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [x] T057 [US1] Create CommandParser with support for /t, /tp, /te commands and --no-cache option in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [x] T058 [US1] Create ReplEngine with PrettyPrompt integration in src/YetAnotherTranslator.Cli/Repl/ReplEngine.cs
- [x] T059 [US1] Wire up dependency injection for US1 components in src/YetAnotherTranslator.Cli/Program.cs
- [x] T060 [US1] Verify graceful CMU Arpabet failure handling integration (covered by T042b) - TranslateWordHandler → AnthropicProvider (null Arpabet) → cache save → display "N/A"

**Checkpoint**: ✅ Word translation fully functional - users can translate words with all linguistic metadata including CMU Arpabet

**Test Results**: All 12 integration tests passing (T036-T042d)

---

## Phase 5: User Story 2 - Text Snippet Translation (Priority: P2)

**Goal**: Translate text snippets between Polish and English (up to 5000 characters)

**Independent Test**: Translate a multi-sentence text snippet and verify accurate translation output

### Integration Tests for User Story 2

- [x] T061 [P] [US2] Integration test for Polish text translation in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs
- [x] T062 [P] [US2] Integration test for English text translation in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs
- [x] T063 [P] [US2] Integration test for text exceeding 5000 characters in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs
- [x] T064 [P] [US2] Integration test for multi-line text with escaped newlines in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs

### Implementation for User Story 2

- [x] T065 [P] [US2] Create TextTranslationResult model in src/YetAnotherTranslator.Core/Handlers/TranslateText/TextTranslationResult.cs
- [x] T066 [P] [US2] Create TranslateTextRequest record in src/YetAnotherTranslator.Core/Handlers/TranslateText/TranslateTextRequest.cs
- [x] T066a [P] [US2] Create SourceLanguage enum (Polish, English, Auto) in src/YetAnotherTranslator.Core/Models/SourceLanguage.cs for use in TranslateTextRequest and word translation; CommandParser sets Auto for /tt and /t commands (auto-detect), Polish for /ttp and /tp, English for /tte and /te
- [x] T067 [P] [US2] Create TextTranslationCacheEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/TextTranslationCacheEntity.cs (already existed from Phase 2)
- [x] T068 [US2] Create TranslateTextRequestValidator with 5000 character limit in src/YetAnotherTranslator.Core/Handlers/TranslateText/TranslateTextValidator.cs
- [x] T069 [US2] Implement TranslateTextAsync method in AnthropicLlmProvider in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs (already existed from Phase 2)
  - Temperature: 0.3 for consistent translations (rationale: maintain consistency with word translation temperature)
  - MaxTokens: 4096 for text snippets (larger than word translations to accommodate 5000 char input + translation output)
  - First call DetectLanguageAsync (implemented in T050a) if source language is Auto; if confidence < 80%, return error per FR-020/FR-041
  - Preserve paragraph structure and formatting in translation
  - Simple plain text response (no JSON parsing needed for text translation)
  - Handle 5000 character limit with clear error if exceeded (validation should catch this, but double-check as defense in depth)
- [x] T070 [US2] Implement TranslateTextHandler with caching in src/YetAnotherTranslator.Core/Handlers/TranslateTextHandler.cs
- [x] T071 [US2] Add /tt, /ttp, /tte command support to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs (already existed)
- [x] T072 [US2] Create TextTranslationFormatter for plain text output in src/YetAnotherTranslator.Cli/Display/TextTranslationFormatter.cs
- [x] T073 [US2] Wire up dependency injection for US2 components in src/YetAnotherTranslator.Cli/Program.cs
  - Verification: Call serviceProvider.GetRequiredService<TranslateTextHandler>() to ensure dependencies resolve correctly

**Checkpoint**: ✅ Text translation fully functional - users can translate text snippets up to 5000 characters

---

## Phase 6: User Story 3 - English Grammar and Vocabulary Review (Priority: P3)

**Goal**: Review English text and identify grammar errors with corrections and vocabulary suggestions

**Independent Test**: Submit English text with known grammar/vocabulary issues and verify system identifies and reports them

### Integration Tests for User Story 3

- [ ] T074 [P] [US3] Integration test for text with grammar errors in tests/YetAnotherTranslator.Tests.Integration/Features/ReviewGrammarTests.cs
- [ ] T075 [P] [US3] Integration test for grammatically correct text with vocabulary suggestions in tests/YetAnotherTranslator.Tests.Integration/Features/ReviewGrammarTests.cs
- [ ] T076 [P] [US3] Integration test for non-English text detection in tests/YetAnotherTranslator.Tests.Integration/Features/ReviewGrammarTests.cs

### Implementation for User Story 3

- [ ] T077 [P] [US3] Create GrammarReviewResult model with nested GrammarIssue and VocabularySuggestion types in src/YetAnotherTranslator.Core/Handlers/ReviewGrammar/GrammarReviewResult.cs
- [ ] T078 [P] [US3] Create ReviewGrammarRequest record in src/YetAnotherTranslator.Core/Handlers/ReviewGrammar/ReviewGrammarRequest.cs
- [ ] T079 [US3] Create ReviewGrammarRequestValidator in src/YetAnotherTranslator.Core/Handlers/ReviewGrammar/ReviewGrammarValidator.cs
- [ ] T080 [US3] Implement ReviewGrammarAsync method with language detection in AnthropicLlmProvider in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs
  - First call DetectLanguageAsync to verify English text; if not English, return error per FR-042
  - Temperature: 0.5 for balanced grammar analysis (rationale: higher than translation 0.3 to allow nuanced vocabulary suggestions while maintaining consistency for grammar rules)
  - MaxTokens: 4096 for detailed grammar feedback
  - Prompt structure: System prompt establishes grammar expert role with instruction to detect specific patterns (subject-verb agreement, article usage, tense errors, double negatives, plural forms per SC-005); user prompt provides text with clear instructions to categorize issues (grammar vs vocabulary) and provide explanations
  - Parse structured JSON response into GrammarReviewResult with nested GrammarIssue and VocabularySuggestion types
  - Handle edge case: Text is grammatically correct but has vocabulary suggestions
  - Include retry logic for API failures with exponential backoff (3 attempts: immediate, wait 1s, wait 2s)
- [ ] T081 [US3] Implement ReviewGrammarHandler in src/YetAnotherTranslator.Core/Handlers/ReviewGrammarHandler.cs
- [ ] T082 [US3] Add /r, /review command support to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T083 [US3] Create GrammarReviewFormatter for formatted output in src/YetAnotherTranslator.Cli/Display/GrammarReviewFormatter.cs
- [ ] T084 [US3] Wire up dependency injection for US3 components in src/YetAnotherTranslator.Cli/Program.cs
  - Verification: Call serviceProvider.GetRequiredService<ReviewGrammarHandler>() to ensure dependencies resolve correctly

**Checkpoint**: Grammar review fully functional - users can review English text for grammar and vocabulary

---

## Phase 7: User Story 4 - Audio Pronunciation Playback (Priority: P4)

**Goal**: Play audio pronunciation for English words and phrases using text-to-speech

**Independent Test**: Request pronunciation of an English word and verify audio playback occurs

### Integration Tests for User Story 4

- [ ] T085 [P] [US4] Integration test for word pronunciation in tests/YetAnotherTranslator.Tests.Integration/Features/PlayPronunciationTests.cs
- [ ] T086 [P] [US4] Integration test for phrase pronunciation in tests/YetAnotherTranslator.Tests.Integration/Features/PlayPronunciationTests.cs
- [ ] T087 [P] [US4] Integration test for pronunciation with part-of-speech parameter in tests/YetAnotherTranslator.Tests.Integration/Features/PlayPronunciationTests.cs
- [ ] T088 [P] [US4] Integration test for pronunciation cache hit in tests/YetAnotherTranslator.Tests.Integration/Features/PlayPronunciationTests.cs

### Implementation for User Story 4

- [ ] T089 [P] [US4] Create PronunciationResult model in src/YetAnotherTranslator.Core/Handlers/PlayPronunciation/PronunciationResult.cs
- [ ] T090 [P] [US4] Create PlayPronunciationRequest record in src/YetAnotherTranslator.Core/Handlers/PlayPronunciation/PlayPronunciationRequest.cs
- [ ] T091 [P] [US4] Create PronunciationCacheEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/PronunciationCacheEntity.cs
  - Cache key includes both text and part-of-speech: SHA256("text:partOfSpeech") where partOfSpeech is empty string if not provided
  - Allows different pronunciations for same word with different POS (e.g., "record" as noun vs verb)
- [ ] T092 [US4] Create PlayPronunciationRequestValidator in src/YetAnotherTranslator.Core/Handlers/PlayPronunciation/PlayPronunciationValidator.cs
- [ ] T093 [US4] Implement ElevenLabsTtsProvider with GenerateSpeechAsync in src/YetAnotherTranslator.Infrastructure/Tts/ElevenLabsTtsProvider.cs
  - ITtsProvider interface signature: Task<byte[]> GenerateSpeechAsync(string text, string? partOfSpeech = null, CancellationToken ct = default)
  - partOfSpeech parameter optional; when provided, may be included in TTS request metadata or used to adjust pronunciation hint
- [ ] T094 [US4] Implement audio playback using PortAudioSharp in src/YetAnotherTranslator.Infrastructure/Tts/PortAudioPlayer.cs
  - Corrected filename: PortAudioPlayer.cs (not AudioPlaybackService.cs) per plan.md
  - Implements IAudioPlayer interface for cross-platform audio playback
- [ ] T095 [US4] Implement PlayPronunciationHandler with caching and error handling in src/YetAnotherTranslator.Core/Handlers/PlayPronunciationHandler.cs
- [ ] T096 [US4] Add /p, /playback command support with optional part-of-speech to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T097 [US4] Wire up dependency injection for US4 components in src/YetAnotherTranslator.Cli/Program.cs
  - Verification: Call serviceProvider.GetRequiredService<PlayPronunciationHandler>() to ensure dependencies resolve correctly

**Checkpoint**: Pronunciation playback fully functional - users can hear English pronunciation

---

## Phase 8: User Story 5 - Operation History Tracking (Priority: P5)

**Goal**: Track and display history of all translation, review, and pronunciation operations

**Independent Test**: Perform several operations, request history, and verify all operations are recorded

### Integration Tests for User Story 5

- [ ] T098 [P] [US5] Integration test for history display after multiple operations in tests/YetAnotherTranslator.Tests.Integration/Features/HistoryTests.cs
- [ ] T099 [P] [US5] Integration test for empty history in tests/YetAnotherTranslator.Tests.Integration/Features/HistoryTests.cs

### Implementation for User Story 5

- [ ] T100 [US5] Implement GetHistoryAsync method in HistoryRepository in src/YetAnotherTranslator.Infrastructure/Persistence/HistoryRepository.cs
- [ ] T101 [US5] Implement GetHistoryHandler in src/YetAnotherTranslator.Core/Handlers/GetHistory/GetHistoryHandler.cs
- [ ] T102 [US5] Add /history, /hist command support to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T103 [US5] Create HistoryFormatter for table output in src/YetAnotherTranslator.Cli/Display/HistoryFormatter.cs
- [ ] T104 [US5] Wire up dependency injection for US5 components in src/YetAnotherTranslator.Cli/Program.cs
  - Verification: Call serviceProvider.GetRequiredService<GetHistoryHandler>() to ensure dependencies resolve correctly

**Checkpoint**: History tracking fully functional - users can view all past operations

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T105 [P] Add /help command with full command reference to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T105a [P] Integration test for /quit command exit code validation in tests/YetAnotherTranslator.Tests.Integration/Features/ReplCommandTests.cs
  - Verify /quit or /q command exits REPL with exit code 0 (success)
  - Verify application terminates gracefully (disposes resources, closes DB connections)
- [ ] T105b [P] Integration test for /clear screen clearing behavior in tests/YetAnotherTranslator.Tests.Integration/Features/ReplCommandTests.cs
  - Verify /clear or /c command clears console output (implementation may vary by platform)
  - Verify REPL continues accepting commands after clear
- [ ] T106a [P] Add /clear command to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T106b [P] Add /quit command to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T107 Validate quickstart.md instructions by following setup steps end-to-end
- [ ] T107a [P] Integration test for UTF-8 encoding validation: Display Polish word translation with diacritics on console and verify characters render correctly (validates FR-038) in tests/YetAnotherTranslator.Tests.Integration/Features/EncodingTests.cs
  - Test words: "ą", "ć", "ę", "ł", "ń", "ó", "ś", "ź", "ż"
  - Verify output bytes match UTF-8 encoding of expected characters
  - Note: Visual rendering depends on terminal font support, but byte-level validation ensures application outputs correct UTF-8

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 6 - Configuration (Phase 3, P0)**: Depends on Foundational - PREREQUISITE for all other stories
- **User Story 1 - Word Translation (Phase 4, P1)**: Depends on Configuration completion - MVP
- **User Story 2 - Text Translation (Phase 5, P2)**: Depends on Configuration and Foundational, can start after US1 MVP validated
- **User Story 3 - Grammar Review (Phase 6, P3)**: Depends on Configuration and Foundational, independent of US1/US2
- **User Story 4 - Pronunciation (Phase 7, P4)**: Depends on Configuration and Foundational, independent of US1/US2/US3
- **User Story 5 - History (Phase 8, P5)**: Depends on Configuration and Foundational, independent of other stories
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 6 (P0)**: PREREQUISITE - Must complete before any other story
- **User Story 1 (P1)**: Can start after US6 - No dependencies on other stories - MVP
- **User Story 2 (P2)**: Can start after US6 - Independent of US1 but builds on same LLM provider infrastructure
- **User Story 3 (P3)**: Can start after US6 - Independent of all other stories
- **User Story 4 (P4)**: Can start after US6 - Independent of all other stories (uses TTS provider, not LLM)
- **User Story 5 (P5)**: Can start after US6 - Independent of all other stories but integrates with all

### Within Each User Story

- Integration tests MUST be written and FAIL before implementation
- Models before handlers
- Handlers before CLI integration
- Core implementation before formatting/display
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T003-T010)
- All Foundational interface tasks marked [P] can run in parallel (T012-T015)
- All Configuration model tasks marked [P] can run in parallel (T028-T032)
- All US1 test tasks marked [P] can run in parallel (T036-T042)
- All US1 model tasks marked [P] can run in parallel (T043-T046, T048-T049)
- Once Configuration (US6) completes, US1, US2, US3, US4, US5 can all start in parallel (if team capacity allows)
- Different user stories can be worked on in parallel by different team members after Configuration phase

**Total Parallelizable Tasks**: 47 tasks marked with [P]

---

## Parallel Example: User Story 1

```bash
# Launch all integration tests for User Story 1 together:
Task: "Integration test for Polish word translation with CMU Arpabet"
Task: "Integration test for English word translation (no CMU Arpabet)"
Task: "Integration test for word with multiple meanings"
Task: "Integration test for word with pronunciation variants"
Task: "Integration test for cache hit scenario"
Task: "Integration test for --no-cache option"
Task: "Integration test for invalid word (empty, too long)"

# Launch all models for User Story 1 together:
Task: "Create Translation nested model"
Task: "Create TranslationResult model"
Task: "Create TranslateWordRequest record"
Task: "Create LlmResponseMetadata model"
Task: "Create HistoryEntryEntity"
Task: "Create TranslationCacheEntity"
```

---

## Implementation Strategy

### MVP First (Configuration + Word Translation Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 6 - Configuration (PREREQUISITE)
4. Complete Phase 4: User Story 1 - Word Translation
5. **STOP and VALIDATE**: Test word translation independently with all linguistic metadata including CMU Arpabet
6. Deploy/demo if ready

**MVP Delivers**:

- Configuration validation and secure credential management
- Polish→English and English→Polish word translation
- Multiple translations ranked by popularity
- Parts of speech, countability, CMU Arpabet (Polish→English only), example sentences
- Table-formatted REPL output
- Caching for performance
- History tracking
- Offline cache fallback

### Incremental Delivery

1. Complete Setup + Foundational + Configuration → Foundation ready
2. Add User Story 1 (Word Translation) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (Text Translation) → Test independently → Deploy/Demo
4. Add User Story 3 (Grammar Review) → Test independently → Deploy/Demo
5. Add User Story 4 (Pronunciation) → Test independently → Deploy/Demo
6. Add User Story 5 (History Display) → Test independently → Deploy/Demo
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational + Configuration together
2. Once Configuration (US6) is done:
   - Developer A: User Story 1 (Word Translation) - MVP priority
   - Developer B: User Story 2 (Text Translation)
   - Developer C: User Story 3 (Grammar Review)
   - Developer D: User Story 4 (Pronunciation)
   - Developer E: User Story 5 (History)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Integration tests MUST fail before implementing features
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- CMU Arpabet only included for Polish→English translations, not English→Polish
- Graceful handling of CMU Arpabet generation failures (display warning instead of failing entire translation)
- All tests use Testcontainers for real PostgreSQL and WireMock.Net for external API mocking
- Use Verify for snapshot testing complex LLM responses
- Follow "return early" pattern per constitution
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

