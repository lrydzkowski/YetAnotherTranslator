# Data Model: Polish-English Translation CLI Tool

**Feature**: `001-polish-english-translator`
**Date**: 2025-11-02
**Phase**: Phase 1 - Design & Contracts

## Overview

This document defines all domain entities, database schema, validation rules, and state transitions for the Polish-English Translation CLI tool. The model supports CQRS pattern with separate command and query concerns.

## Domain Models (YetAnotherTranslator.Core)

### 1. TranslationResult

Represents the result of translating a word with linguistic metadata.

**Fields**:

- `SourceWord` (string, required): Original word to translate
- `SourceLanguage` (string, required): Language of source word ("Polish" | "English")
- `TargetLanguage` (string, required): Language of translations ("Polish" | "English")
- `Translations` (List<Translation>, required): Ordered list of translations by popularity
- `DetectedLanguage` (string, nullable): Auto-detected language if auto-detect was used

**Nested Type: Translation**:

- `Rank` (int, required): Popularity ranking (1 = most popular)
- `Word` (string, required): Translated word
- `PartOfSpeech` (string, required): Grammatical category (noun, verb, adjective, etc.)
- `Countability` (string, nullable): "countable" | "uncountable" | null (only for nouns)
- `CmuArpabet` (string, nullable): CMU Arpabet phonetic transcription (only for English translations when translating Polish→English, not for English→Polish)
- `Examples` (List<string>, required): Example sentences demonstrating usage

**Validation Rules**:

- `SourceWord`: Not empty, max 100 characters
- `SourceLanguage`: Must be "Polish" or "English"
- `TargetLanguage`: Must be "Polish" or "English", different from SourceLanguage
- `Translations`: At least 1 translation, max 10 translations
- `Translation.Rank`: Unique within list, sequential starting from 1
- `Translation.Word`: Not empty, max 100 characters
- `Translation.Example`: Not empty, max 500 characters

**State Transitions**: Immutable value object (no state transitions)

### 2. TextTranslationResult

Represents the result of translating a text snippet.

**Fields**:

- `SourceText` (string, required): Original text to translate
- `TranslatedText` (string, required): Translated text
- `SourceLanguage` (string, required): Language of source text ("Polish" | "English")
- `TargetLanguage` (string, required): Language of translation ("Polish" | "English")
- `DetectedLanguage` (string, nullable): Auto-detected language if auto-detect was used
- `CharacterCount` (int, required): Number of characters in source text

**Validation Rules**:

- `SourceText`: Not empty, max 5000 characters
- `TranslatedText`: Not empty
- `SourceLanguage`: Must be "Polish" or "English"
- `TargetLanguage`: Must be "Polish" or "English", different from SourceLanguage

**State Transitions**: Immutable value object (no state transitions)

### 3. GrammarReviewResult

Represents the result of reviewing English text for grammar and vocabulary.

**Fields**:

- `OriginalText` (string, required): Text that was reviewed
- `IsCorrect` (bool, required): Whether text is grammatically correct
- `GrammarIssues` (List<GrammarIssue>, required): Grammar errors found (empty if none)
- `VocabularySuggestions` (List<VocabularySuggestion>, required): Vocabulary improvements (empty if none)

**Nested Type: GrammarIssue**:

- `Type` (string, required): Error category (e.g., "subject-verb agreement", "tense", "article")
- `OriginalText` (string, required): Problematic text fragment
- `CorrectedText` (string, required): Suggested correction
- `Explanation` (string, required): Why the correction is needed

**Nested Type: VocabularySuggestion**:

- `OriginalWord` (string, required): Word to replace
- `SuggestedWord` (string, required): Better alternative
- `Reason` (string, required): Why the suggestion is better

**Validation Rules**:

- `OriginalText`: Not empty, max 5000 characters
- `GrammarIssues`: Max 50 issues
- `VocabularySuggestions`: Max 20 suggestions

**State Transitions**: Immutable value object (no state transitions)

### 4. PronunciationResult

Represents the result of generating pronunciation audio.

**Fields**:

- `Text` (string, required): Text that was pronounced
- `PartOfSpeech` (string, nullable): Optional part of speech for disambiguation
- `AudioData` (byte[], required): MP3 audio data
- `AudioFormat` (string, required): Audio file format ("mp3")
- `VoiceId` (string, required): ElevenLabs voice ID used
- `AudioSizeBytes` (int, required): Size of audio data

**Validation Rules**:

- `Text`: Not empty, max 500 characters
- `AudioData`: Not empty
- `PartOfSpeech`: If specified, must be valid POS (noun, verb, adjective, etc.)

**State Transitions**: Immutable value object (no state transitions)

### 5. CommandType (Enum)

Enumeration of all command types for history tracking.

**Values**:

- `TranslateWord` = 0
- `TranslateText` = 1
- `ReviewGrammar` = 2
- `PlayPronunciation` = 3

### 6. LlmResponseMetadata

Metadata from LLM API responses for tracking and debugging.

**Fields**:

- `Model` (string, required): Model used (e.g., "claude-3-5-sonnet-20241022")
- `InputTokens` (int, required): Number of tokens in request
- `OutputTokens` (int, required): Number of tokens in response
- `StopReason` (string, required): Why generation stopped ("end_turn", "max_tokens", etc.)
- `ResponseId` (string, required): Unique response ID from provider

**Validation Rules**:

- All fields required
- Token counts >= 0

**State Transitions**: Immutable value object (no state transitions)

## Database Entities (YetAnotherTranslator.Infrastructure.Persistence)

### 1. HistoryEntryEntity

Persistent record of all operations for history and caching.

**Table**: `history_entries`

**Columns**:

- `id` (uuid, PK): Unique identifier
- `timestamp` (timestamp with time zone, required, indexed): When operation occurred
- `command_type` (integer, required): CommandType enum value
- `input_text` (text, required): User input
- `output_text` (jsonb, required): Serialized result object
- `llm_metadata` (jsonb, nullable): Serialized LlmResponseMetadata
- `created_at` (timestamp with time zone, required): Record creation time

**Indexes**:

- Primary key on `id`
- Index on `timestamp DESC` for recent history queries
- Index on `(command_type, input_text)` for cache lookups

**Validation Rules**:

- `timestamp`: Must be <= current time
- `command_type`: Must be valid CommandType value (0-3)
- `input_text`: Not empty
- `output_text`: Valid JSON

**Relationships**: None (independent entity)

**Retention Policy**: Keep last 100 entries per command type, delete older

### 2. TranslationCacheEntity

Cache for word translations to avoid repeated LLM calls.

**Table**: `translation_cache`

**Columns**:

- `id` (uuid, PK): Unique identifier
- `cache_key` (varchar(64), required, unique indexed): SHA256 hash of (word, source_lang, target_lang)
- `source_word` (varchar(100), required): Original word
- `source_language` (varchar(20), required): Source language
- `target_language` (varchar(20), required): Target language
- `result_json` (jsonb, required): Serialized TranslationResult
- `created_at` (timestamp with time zone, required): Cache entry creation time
- `accessed_at` (timestamp with time zone, required): Last access time
- `access_count` (integer, required, default: 1): Number of times accessed

**Indexes**:

- Primary key on `id`
- Unique index on `cache_key`
- Index on `created_at` for cache expiration cleanup

**Validation Rules**:

- `cache_key`: Exactly 64 characters (SHA256 hex)
- `source_word`: Not empty, max 100 characters
- `result_json`: Valid JSON matching TranslationResult schema
- `access_count`: >= 1

**Relationships**: None (independent entity)

**Retention Policy**:
- **v1 Scope**: No automatic expiration or cleanup (unlimited storage per FR-045)
- **Future Enhancement**: 30-day expiration and LRU eviction deferred to post-MVP release
- **User Responsibility**: Users manage their own database storage and can manually delete old cache entries if needed
- **Rationale**: Simplicity principle - avoid complexity of background cleanup jobs in v1; users have `--no-cache` option to force fresh results

### 3. TextTranslationCacheEntity

Cache for text translations to avoid repeated LLM calls.

**Table**: `text_translation_cache`

**Columns**:

- `id` (uuid, PK): Unique identifier
- `cache_key` (varchar(64), required, unique indexed): SHA256 hash of (text, source_lang, target_lang)
- `source_text_hash` (varchar(64), required): SHA256 hash of source text only
- `source_language` (varchar(20), required): Source language
- `target_language` (varchar(20), required): Target language
- `result_json` (jsonb, required): Serialized TextTranslationResult
- `created_at` (timestamp with time zone, required): Cache entry creation time
- `accessed_at` (timestamp with time zone, required): Last access time
- `access_count` (integer, required, default: 1): Number of times accessed

**Indexes**:

- Primary key on `id`
- Unique index on `cache_key`
- Index on `created_at` for cache expiration cleanup

**Validation Rules**:

- `cache_key`: Exactly 64 characters (SHA256 hex)
- `result_json`: Valid JSON matching TextTranslationResult schema
- `access_count`: >= 1

**Relationships**: None (independent entity)

**Retention Policy**:
- **v1 Scope**: No automatic expiration or cleanup (unlimited storage per FR-045)
- **Future Enhancement**: 30-day expiration and LRU eviction deferred to post-MVP release
- **User Responsibility**: Users manage their own database storage and can manually delete old cache entries if needed
- **Rationale**: Simplicity principle - avoid complexity of background cleanup jobs in v1; users have `--no-cache` option to force fresh results

### 4. PronunciationCacheEntity

Cache for pronunciation audio to avoid repeated TTS API calls.

**Table**: `pronunciation_cache`

**Columns**:

- `id` (uuid, PK): Unique identifier
- `cache_key` (varchar(64), required, unique indexed): SHA256 hash of (text, part_of_speech, voice_id)
- `text` (varchar(500), required): Text that was pronounced
- `part_of_speech` (varchar(50), nullable): Optional POS for disambiguation
- `voice_id` (varchar(100), required): ElevenLabs voice ID
- `audio_data` (bytea, required): MP3 audio bytes
- `audio_format` (varchar(10), required): Audio format ("mp3")
- `audio_size_bytes` (integer, required): Size of audio data
- `created_at` (timestamp with time zone, required): Cache entry creation time
- `accessed_at` (timestamp with time zone, required): Last access time
- `access_count` (integer, required, default: 1): Number of times accessed

**Indexes**:

- Primary key on `id`
- Unique index on `cache_key`
- Index on `created_at` for cache expiration cleanup

**Validation Rules**:

- `cache_key`: Exactly 64 characters (SHA256 hex)
- `text`: Not empty, max 500 characters
- `audio_data`: Not empty
- `audio_size_bytes`: > 0, matches actual audio_data size
- `access_count`: >= 1

**Relationships**: None (independent entity)

**Retention Policy**:
- **v1 Scope**: No automatic expiration or cleanup (unlimited storage per FR-045)
- **Future Enhancement**: 30-day expiration and LRU eviction deferred to post-MVP release
- **User Responsibility**: Users manage their own database storage and can manually delete old cache entries if needed
- **Rationale**: Simplicity principle - avoid complexity of background cleanup jobs in v1; users have `--no-cache` option to force fresh results

## Configuration Models (YetAnotherTranslator.Infrastructure.Configuration)

### 1. ApplicationConfiguration

Root configuration loaded from config.json.

**Fields**:

- `SecretManager` (SecretManagerConfiguration, required): Secret manager settings
- `LlmProvider` (LlmProviderConfiguration, required): LLM provider settings
- `TtsProvider` (TtsProviderConfiguration, required): TTS provider settings
- `Database` (DatabaseConfiguration, required): Database connection settings

**Validation Rules** (via FluentValidation):

- All nested configurations required
- Validates recursively

### 2. SecretManagerConfiguration

Configuration for Azure Key Vault.

**Fields**:

- `Type` (string, required): Backend type ("azure-keyvault")
- `KeyVaultUrl` (string, required): Key Vault URL (e.g., "https://vault.vault.azure.net")

**Validation Rules**:

- `Type`: Must equal "azure-keyvault"
- `KeyVaultUrl`: Must be valid HTTPS URL matching pattern `https://*.vault.azure.net`

### 3. LlmProviderConfiguration

Configuration for LLM provider (Anthropic).

**Fields**:

- `Type` (string, required): Provider type ("anthropic")
- `SecretReference` (string, required): Key Vault secret name for API key
- `Model` (string, required): Model to use (e.g., "claude-3-5-sonnet-20241022")
- `MaxTokens` (int, required): Default max tokens for responses
- `Temperature` (decimal, required): Default temperature (0.0-1.0)

**Validation Rules**:

- `Type`: Must equal "anthropic"
- `SecretReference`: Not empty
- `Model`: Not empty
- `MaxTokens`: 1-8192
- `Temperature`: 0.0-1.0

### 4. TtsProviderConfiguration

Configuration for TTS provider (ElevenLabs).

**Fields**:

- `Type` (string, required): Provider type ("elevenlabs")
- `SecretReference` (string, required): Key Vault secret name for API key
- `VoiceId` (string, required): ElevenLabs voice ID to use
- `Model` (string, nullable): Optional model selection

**Validation Rules**:

- `Type`: Must equal "elevenlabs"
- `SecretReference`: Not empty
- `VoiceId`: Not empty

### 5. DatabaseConfiguration

Configuration for PostgreSQL database.

**Fields**:

- `SecretReference` (string, required): Key Vault secret name for connection string
- `MaxRetryCount` (int, required): EF Core retry count (default: 3)
- `CommandTimeout` (int, required): Command timeout in seconds (default: 30)

**Validation Rules**:

- `SecretReference`: Not empty
- `MaxRetryCount`: 0-10
- `CommandTimeout`: 1-300

## Cache Key Generation

All cache keys use SHA256 hashing for consistent, collision-resistant keys.

### Word Translation Cache Key

```csharp
string GenerateTranslationCacheKey(string word, string sourceLang, string targetLang)
{
    var input = $"{word.ToLowerInvariant()}|{sourceLang}|{targetLang}";
    using var sha256 = SHA256.Create();
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexString(hashBytes).ToLowerInvariant();
}
```

### Text Translation Cache Key

```csharp
string GenerateTextTranslationCacheKey(string text, string sourceLang, string targetLang)
{
    var input = $"{text}|{sourceLang}|{targetLang}";
    using var sha256 = SHA256.Create();
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexString(hashBytes).ToLowerInvariant();
}
```

### Pronunciation Cache Key

```csharp
string GeneratePronunciationCacheKey(string text, string partOfSpeech, string voiceId)
{
    var pos = partOfSpeech ?? "none";
    var input = $"{text.ToLowerInvariant()}|{pos}|{voiceId}";
    using var sha256 = SHA256.Create();
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexString(hashBytes).ToLowerInvariant();
}
```

## Database Migrations

Migration strategy using EF Core migrations.

**Initial Migration** (`001_InitialSchema`):

- Create `history_entries` table
- Create `translation_cache` table
- Create `text_translation_cache` table
- Create `pronunciation_cache` table
- Create all indexes

**Migration Commands**:

```bash
dotnet ef migrations add InitialSchema --project src/YetAnotherTranslator.Infrastructure
dotnet ef database update --project src/YetAnotherTranslator.Infrastructure
```

## Entity Relationships Diagram

```text
┌─────────────────────────────┐
│   HistoryEntryEntity        │
│  (All operations log)       │
├─────────────────────────────┤
│ id (PK)                     │
│ timestamp (indexed)         │
│ command_type                │
│ input_text                  │
│ output_text (jsonb)         │
│ llm_metadata (jsonb)        │
│ created_at                  │
└─────────────────────────────┘

┌─────────────────────────────┐
│ TranslationCacheEntity      │
│   (Word translation cache)  │
├─────────────────────────────┤
│ id (PK)                     │
│ cache_key (unique indexed)  │
│ source_word                 │
│ source_language             │
│ target_language             │
│ result_json (jsonb)         │
│ created_at                  │
│ accessed_at                 │
│ access_count                │
└─────────────────────────────┘

┌─────────────────────────────┐
│ TextTranslationCacheEntity  │
│   (Text translation cache)  │
├─────────────────────────────┤
│ id (PK)                     │
│ cache_key (unique indexed)  │
│ source_text_hash            │
│ source_language             │
│ target_language             │
│ result_json (jsonb)         │
│ created_at                  │
│ accessed_at                 │
│ access_count                │
└─────────────────────────────┘

┌─────────────────────────────┐
│ PronunciationCacheEntity    │
│   (Pronunciation cache)     │
├─────────────────────────────┤
│ id (PK)                     │
│ cache_key (unique indexed)  │
│ text                        │
│ part_of_speech              │
│ voice_id                    │
│ audio_data (bytea)          │
│ audio_format                │
│ audio_size_bytes            │
│ created_at                  │
│ accessed_at                 │
│ access_count                │
└─────────────────────────────┘

Note: No foreign key relationships - all entities are independent
```

## Data Flow

### Command Flow (Write Operations)

1. User issues command in REPL
2. CommandParser parses command to extract operation and arguments
3. CLI directly invokes appropriate handler (e.g., `TranslateWordHandler`)
4. Handler validates input with FluentValidation
5. Handler checks cache (if `--no-cache` not specified)
6. If cache miss, handler calls external provider (LLM/TTS)
7. Handler saves to cache
8. Handler saves to history
9. Result returned to user

### Query Flow (Read Operations)

1. User requests history
2. CLI directly invokes `GetHistoryHandler`
3. Handler queries `history_entries` table
4. Results ordered by timestamp DESC
5. Results limited to last 100 entries
6. Results formatted and returned

## Summary

The data model supports:

- ✓ All functional requirements (FR-001 through FR-038)
- ✓ Handler pattern with clear separation of concerns (no MediatR)
- ✓ Efficient caching with SHA256 keys
- ✓ History tracking with LLM metadata
- ✓ Configuration validation with FluentValidation
- ✓ PostgreSQL with EF Core for persistence
- ✓ Immutable domain models for thread safety
- ✓ Performance goals (caching enables <1s lookups)

