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

- [ ] T001 Create solution file YetAnotherTranslator.sln in repository root
- [ ] T002 Create Core project src/YetAnotherTranslator.Core/YetAnotherTranslator.Core.csproj targeting .NET 10
- [ ] T003 [P] Create Infrastructure project src/YetAnotherTranslator.Infrastructure/YetAnotherTranslator.Infrastructure.csproj targeting .NET 10
- [ ] T004 [P] Create CLI project src/YetAnotherTranslator.Cli/YetAnotherTranslator.Cli.csproj targeting .NET 10
- [ ] T005 [P] Create Integration test project tests/YetAnotherTranslator.Tests.Integration/YetAnotherTranslator.Tests.Integration.csproj
- [ ] T006 Configure project references: Infrastructure references Core, CLI references Core and Infrastructure, Tests reference all
- [ ] T007 [P] Add Core dependencies: FluentValidation, FluentValidation.DependencyInjectionExtensions
- [ ] T008 [P] Add Infrastructure dependencies: Anthropic SDK, ElevenLabs-DotNet, Azure.Security.KeyVault.Secrets, Azure.Identity, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, PortAudioSharp
- [ ] T009 [P] Add CLI dependencies: PrettyPrompt, Spectre.Console
- [ ] T010 [P] Add Test dependencies: xUnit, xUnit.runner.visualstudio, Microsoft.NET.Test.Sdk, FluentAssertions, NSubstitute, Testcontainers.PostgreSql, WireMock.Net, Verify.Xunit

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T011 Create directory structure: Core/{Handlers,Interfaces,Models,Validation}, Infrastructure/{Llm,Tts,Secrets,Persistence,Configuration}, CLI/{Repl,Display}
- [ ] T012 [P] Create ILlmProvider interface in src/YetAnotherTranslator.Core/Interfaces/ILlmProvider.cs
- [ ] T013 [P] Create ITtsProvider interface in src/YetAnotherTranslator.Core/Interfaces/ITtsProvider.cs
- [ ] T014 [P] Create ISecretsProvider interface in src/YetAnotherTranslator.Core/Interfaces/ISecretsProvider.cs
- [ ] T015 [P] Create IHistoryRepository interface in src/YetAnotherTranslator.Core/Interfaces/IHistoryRepository.cs
- [ ] T016 [P] Create CommandType enum in src/YetAnotherTranslator.Core/Models/CommandType.cs
- [ ] T017 Create TranslatorDbContext in src/YetAnotherTranslator.Infrastructure/Persistence/TranslatorDbContext.cs with DbSets for HistoryEntry, TranslationCache, TextTranslationCache, PronunciationCache
- [ ] T018 Create initial EF Core migration InitialSchema using dotnet ef migrations add
  - Entities included: HistoryEntryEntity, TranslationCacheEntity, TextTranslationCacheEntity, PronunciationCacheEntity
  - Indexes: Primary keys, unique indexes on cache_key columns, timestamp indexes for history queries
  - Command: `dotnet ef migrations add InitialSchema --project src/YetAnotherTranslator.Infrastructure --startup-project src/YetAnotherTranslator.Cli`
  - Verify migration creates all 4 tables with correct column types (uuid, timestamp with time zone, jsonb, bytea, varchar)
- [ ] T019 [P] Create TestBase class in tests/YetAnotherTranslator.Tests.Integration/Infrastructure/TestBase.cs with Testcontainers PostgreSQL setup and WireMock server initialization
- [ ] T020 [P] Create WireMock fixtures directory tests/YetAnotherTranslator.Tests.Integration/Infrastructure/WireMockFixtures/
- [ ] T021 [P] Setup dependency injection container configuration in src/YetAnotherTranslator.Cli/Program.cs
- [ ] T022 Create error handling infrastructure and base exception types in src/YetAnotherTranslator.Core/Exceptions/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 6 - Configuration Validation (Priority: P0) 🎯 PREREQUISITE

**Goal**: Validate configuration at startup before any translation features can function

**Independent Test**: Run application with missing/incomplete/invalid config and verify error messages

### Integration Tests for User Story 6

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T023 [P] [US6] Integration test for missing config file in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [ ] T024 [P] [US6] Integration test for malformed JSON config in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [ ] T025 [P] [US6] Integration test for incomplete config (missing LLM provider) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [ ] T026 [P] [US6] Integration test for invalid Azure Key Vault URL in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [ ] T027 [P] [US6] Integration test for valid config successfully launches REPL in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [ ] T027a [P] [US6] Integration test for Azure Key Vault network failure (timeout) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [ ] T027b [P] [US6] Integration test for Azure Key Vault permission denied (403) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs
- [ ] T027c [P] [US6] Integration test for secret not found (404) in tests/YetAnotherTranslator.Tests.Integration/Features/ConfigurationValidationTests.cs

### Implementation for User Story 6

- [ ] T028 [P] [US6] Create ApplicationConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/ApplicationConfiguration.cs
- [ ] T029 [P] [US6] Create SecretManagerConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/SecretManagerConfiguration.cs
- [ ] T030 [P] [US6] Create LlmProviderConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/LlmProviderConfiguration.cs
- [ ] T031 [P] [US6] Create TtsProviderConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/TtsProviderConfiguration.cs
- [ ] T032 [P] [US6] Create DatabaseConfiguration model in src/YetAnotherTranslator.Infrastructure/Configuration/DatabaseConfiguration.cs
- [ ] T033 [US6] Create ConfigurationValidator with FluentValidation in src/YetAnotherTranslator.Infrastructure/Configuration/ConfigurationValidator.cs
- [ ] T034 [US6] Implement configuration loading from standard user config directory in src/YetAnotherTranslator.Cli/Program.cs
- [ ] T035 [US6] Implement startup validation that fails fast with clear error messages and exits with non-zero status code on failures in src/YetAnotherTranslator.Cli/Program.cs

**Checkpoint**: Configuration validation complete - application can safely start with valid config

---

## Phase 4: User Story 1 - Word Translation with Detailed Linguistic Information (Priority: P1) 🎯 MVP

**Goal**: Translate individual words between Polish and English with linguistic metadata (parts of speech, countability, CMU Arpabet for English translations, example sentences)

**Independent Test**: Translate a Polish word and verify output shows multiple translations ranked by popularity with all linguistic details

### Integration Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T036 [P] [US1] Integration test for Polish word translation with CMU Arpabet in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T037 [P] [US1] Integration test for English word translation (no CMU Arpabet) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T038 [P] [US1] Integration test for word with multiple meanings in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T039 [P] [US1] Integration test for word with pronunciation variants (part-of-speech-specific CMU Arpabet) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T040 [P] [US1] Integration test for cache hit scenario in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T041 [P] [US1] Integration test for --no-cache option in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T042 [P] [US1] Integration test for invalid word (empty, too long) in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T042a [P] [US1] Integration test verifying CMU Arpabet is saved to TranslationCache and retrieved from cache on subsequent call in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs
- [ ] T042b [P] [US1] Integration test verifying CMU Arpabet failure (null) is saved to cache and subsequent calls display "N/A" from cache in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateWordTests.cs

### Implementation for User Story 1

- [ ] T043 [P] [US1] Create Translation nested model in src/YetAnotherTranslator.Core/Models/Translation.cs with Rank, Word, PartOfSpeech, Countability, CmuArpabet, Examples fields
- [ ] T044 [P] [US1] Create TranslationResult model in src/YetAnotherTranslator.Core/Models/TranslationResult.cs
- [ ] T045 [P] [US1] Create TranslateWordRequest record in src/YetAnotherTranslator.Core/Models/TranslateWordRequest.cs
- [ ] T046 [P] [US1] Create LlmResponseMetadata model in src/YetAnotherTranslator.Core/Models/LlmResponseMetadata.cs
- [ ] T047 [US1] Create TranslateWordRequestValidator in src/YetAnotherTranslator.Core/Validation/TranslateWordRequestValidator.cs
- [ ] T048 [P] [US1] Create HistoryEntryEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/HistoryEntryEntity.cs
- [ ] T049 [P] [US1] Create TranslationCacheEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/TranslationCacheEntity.cs
- [ ] T050 [US1] Update TranslatorDbContext with entity configurations and indexes in src/YetAnotherTranslator.Infrastructure/Persistence/TranslatorDbContext.cs
- [ ] T051 [US1] Implement AnthropicLlmProvider with TranslateWordAsync method that requests CMU Arpabet from LLM in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs
  - Use structured JSON response format (see research.md section 9 for prompt template)
  - Temperature: 0.3 for consistent translations
  - MaxTokens: 2048 for word translations
  - Parse JSON response into TranslationResult model
  - Handle null cmuArpabet field gracefully (set to null in model, display as "N/A" in UI)
  - Include error handling for malformed JSON responses (retry once, then fail with clear message)
  - Log LLM response metadata (tokens, model, response ID) to LlmResponseMetadata
- [ ] T052 [US1] Implement AzureKeyVaultSecretsProvider in src/YetAnotherTranslator.Infrastructure/Secrets/AzureKeyVaultSecretsProvider.cs
- [ ] T053 [US1] Implement HistoryRepository with cache-aside pattern (check cache, call API on miss, save to cache) in src/YetAnotherTranslator.Infrastructure/Persistence/HistoryRepository.cs
- [ ] T054 [US1] Implement cache key generation using SHA256 hash in src/YetAnotherTranslator.Infrastructure/Persistence/CacheKeyGenerator.cs
- [ ] T055 [US1] Implement TranslateWordHandler with validation, caching, LLM call, history save in src/YetAnotherTranslator.Core/Handlers/TranslateWordHandler.cs
- [ ] T056 [US1] Create TranslationTableFormatter for Spectre.Console table output with CMU Arpabet column (Polish→English only) in src/YetAnotherTranslator.Cli/Display/TranslationTableFormatter.cs
- [ ] T057 [US1] Create CommandParser with support for /t, /tp, /te commands and --no-cache option in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T058 [US1] Create ReplEngine with PrettyPrompt integration in src/YetAnotherTranslator.Cli/Repl/ReplEngine.cs
- [ ] T059 [US1] Wire up dependency injection for US1 components in src/YetAnotherTranslator.Cli/Program.cs
- [ ] T060 [US1] Implement graceful CMU Arpabet failure handling (display translation with warning in Arpabet column) in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs

**Checkpoint**: Word translation fully functional - users can translate words with all linguistic metadata including CMU Arpabet

---

## Phase 5: User Story 2 - Text Snippet Translation (Priority: P2)

**Goal**: Translate text snippets between Polish and English (up to 5000 characters)

**Independent Test**: Translate a multi-sentence text snippet and verify accurate translation output

### Integration Tests for User Story 2

- [ ] T061 [P] [US2] Integration test for Polish text translation in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs
- [ ] T062 [P] [US2] Integration test for English text translation in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs
- [ ] T063 [P] [US2] Integration test for text exceeding 5000 characters in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs
- [ ] T064 [P] [US2] Integration test for multi-line text with escaped newlines in tests/YetAnotherTranslator.Tests.Integration/Features/TranslateTextTests.cs

### Implementation for User Story 2

- [ ] T065 [P] [US2] Create TextTranslationResult model in src/YetAnotherTranslator.Core/Models/TextTranslationResult.cs
- [ ] T066 [P] [US2] Create TranslateTextRequest record in src/YetAnotherTranslator.Core/Models/TranslateTextRequest.cs
- [ ] T067 [P] [US2] Create TextTranslationCacheEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/TextTranslationCacheEntity.cs
- [ ] T068 [US2] Create TranslateTextRequestValidator with 5000 character limit in src/YetAnotherTranslator.Core/Validation/TranslateTextRequestValidator.cs
- [ ] T069 [US2] Implement TranslateTextAsync method in AnthropicLlmProvider in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs
  - Temperature: 0.3 for consistent translations
  - MaxTokens: 4096 for text snippets (larger than word translations)
  - First call DetectLanguageAsync if source language is Auto
  - Preserve paragraph structure and formatting in translation
  - Simple plain text response (no JSON parsing needed for text translation)
  - Handle 5000 character limit with clear error if exceeded (validation should catch this, but double-check)
- [ ] T070 [US2] Implement TranslateTextHandler with caching in src/YetAnotherTranslator.Core/Handlers/TranslateTextHandler.cs
- [ ] T071 [US2] Add /tt, /ttp, /tte command support to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T072 [US2] Create TextTranslationFormatter for plain text output in src/YetAnotherTranslator.Cli/Display/TextTranslationFormatter.cs
- [ ] T073 [US2] Wire up dependency injection for US2 components in src/YetAnotherTranslator.Cli/Program.cs

**Checkpoint**: Text translation fully functional - users can translate text snippets up to 5000 characters

---

## Phase 6: User Story 3 - English Grammar and Vocabulary Review (Priority: P3)

**Goal**: Review English text and identify grammar errors with corrections and vocabulary suggestions

**Independent Test**: Submit English text with known grammar/vocabulary issues and verify system identifies and reports them

### Integration Tests for User Story 3

- [ ] T074 [P] [US3] Integration test for text with grammar errors in tests/YetAnotherTranslator.Tests.Integration/Features/ReviewGrammarTests.cs
- [ ] T075 [P] [US3] Integration test for grammatically correct text with vocabulary suggestions in tests/YetAnotherTranslator.Tests.Integration/Features/ReviewGrammarTests.cs
- [ ] T076 [P] [US3] Integration test for non-English text detection in tests/YetAnotherTranslator.Tests.Integration/Features/ReviewGrammarTests.cs

### Implementation for User Story 3

- [ ] T077 [P] [US3] Create GrammarReviewResult model with nested GrammarIssue and VocabularySuggestion types in src/YetAnotherTranslator.Core/Models/GrammarReviewResult.cs
- [ ] T078 [P] [US3] Create ReviewGrammarRequest record in src/YetAnotherTranslator.Core/Models/ReviewGrammarRequest.cs
- [ ] T079 [US3] Create ReviewGrammarRequestValidator in src/YetAnotherTranslator.Core/Validation/ReviewGrammarRequestValidator.cs
- [ ] T080 [US3] Implement ReviewGrammarAsync method with language detection in AnthropicLlmProvider in src/YetAnotherTranslator.Infrastructure/Llm/AnthropicLlmProvider.cs
  - First call DetectLanguageAsync to verify English text
  - Temperature: 0.5 for balanced grammar analysis (higher than translation for nuanced suggestions)
  - MaxTokens: 4096 for detailed grammar feedback
  - Prompt structure: System prompt establishes grammar expert role, user prompt provides text with clear instructions to categorize issues (grammar vs vocabulary) and provide explanations
  - Parse structured JSON response into GrammarReviewResult with nested GrammarIssue and VocabularySuggestion types
  - Handle edge case: Text is grammatically correct but has vocabulary suggestions
  - Include retry logic for API failures with exponential backoff (3 attempts)
- [ ] T081 [US3] Implement ReviewGrammarHandler in src/YetAnotherTranslator.Core/Handlers/ReviewGrammarHandler.cs
- [ ] T082 [US3] Add /r, /review command support to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T083 [US3] Create GrammarReviewFormatter for formatted output in src/YetAnotherTranslator.Cli/Display/GrammarReviewFormatter.cs
- [ ] T084 [US3] Wire up dependency injection for US3 components in src/YetAnotherTranslator.Cli/Program.cs

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

- [ ] T089 [P] [US4] Create PronunciationResult model in src/YetAnotherTranslator.Core/Models/PronunciationResult.cs
- [ ] T090 [P] [US4] Create PlayPronunciationRequest record in src/YetAnotherTranslator.Core/Models/PlayPronunciationRequest.cs
- [ ] T091 [P] [US4] Create PronunciationCacheEntity in src/YetAnotherTranslator.Infrastructure/Persistence/Entities/PronunciationCacheEntity.cs
- [ ] T092 [US4] Create PlayPronunciationRequestValidator in src/YetAnotherTranslator.Core/Validation/PlayPronunciationRequestValidator.cs
- [ ] T093 [US4] Implement ElevenLabsTtsProvider with GenerateSpeechAsync in src/YetAnotherTranslator.Infrastructure/Tts/ElevenLabsTtsProvider.cs
- [ ] T094 [US4] Implement audio playback using PortAudioSharp in src/YetAnotherTranslator.Infrastructure/Tts/AudioPlaybackService.cs
- [ ] T095 [US4] Implement PlayPronunciationHandler with caching and error handling in src/YetAnotherTranslator.Core/Handlers/PlayPronunciationHandler.cs
- [ ] T096 [US4] Add /p, /playback command support with optional part-of-speech to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T097 [US4] Wire up dependency injection for US4 components in src/YetAnotherTranslator.Cli/Program.cs

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
- [ ] T101 [US5] Implement GetHistoryHandler in src/YetAnotherTranslator.Core/Handlers/GetHistoryHandler.cs
- [ ] T102 [US5] Add /history, /hist command support to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T103 [US5] Create HistoryFormatter for table output in src/YetAnotherTranslator.Cli/Display/HistoryFormatter.cs
- [ ] T104 [US5] Wire up dependency injection for US5 components in src/YetAnotherTranslator.Cli/Program.cs

**Checkpoint**: History tracking fully functional - users can view all past operations

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T105 [P] Add /help command with full command reference to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T106 [P] Add /clear and /quit commands to CommandParser in src/YetAnotherTranslator.Cli/Repl/CommandParser.cs
- [ ] T107 Validate quickstart.md instructions by following setup steps end-to-end

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

