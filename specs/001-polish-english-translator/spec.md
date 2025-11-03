# Feature Specification: Polish-English Translation CLI Tool

**Feature Branch**: `001-polish-english-translator`
**Created**: 2025-11-01
**Status**: Draft
**Input**: User description: "A CLI tool that will allow me to: Translate individual words between Polish and English. It should show the list of possible translations from the most popular to the least popular together with information about their parts of speeches, with information if they are countable or uncountable, and with example sentences. Translate snippets of text. Review grammar and vocabulary of English snippets. Play English pronunciation of words and expressions. Save the history of all operations."

## Clarifications

### Session 2025-11-01

- Q: Should pronunciation playback accept part of speech parameter for words with different pronunciations based on usage? → A: Yes, pronunciation should accept optional part of speech parameter since words can have different pronunciations (e.g., "record" as noun vs verb)
- Q: How should users invoke different operations (CLI command structure)? → A: REPL-style interface with command prefixes: `/t` (translate word auto-detect), `/tp` (translate Polish word), `/te` (translate English word), `/tt` (translate text auto-detect), `/ttp` (translate Polish text), `/tte` (translate English text), `/r` (review English text), `/p` (pronounce English), `/h` (help), `/c` (clear screen), `/q` (quit)
- Q: How should users access their operation history in the REPL? → A: `/history` or `/hist` command
- Q: How should word translation results be displayed in the REPL? → A: Table format with columns for rank, translation, part of speech, countability, CMU Arpabet (for English translations), and examples
- Q: How should users input multi-line text in the REPL for text translation and review? → A: Single-line only with escaped newlines (`\n`) for multi-line content

### Session 2025-11-02

- Q: What should be the implementation approach for core translation, grammar review, and linguistic analysis functions? → A: All main functions (word translation, text translation, grammar review, vocabulary suggestions) should be LLM-based, which inherently handles misspellings without separate spell-checking logic
- Q: Should the system use a single LLM provider for all operations or different providers for different functions? → A: Configurable/pluggable provider system allowing users to choose or switch providers
- Q: How should users configure their LLM provider settings and API credentials? → A: Manual configuration file editing - Users manually create and edit a configuration file; the application validates all necessary configuration exists at startup
- Q: How should audio pronunciation be generated for the pronunciation playback feature? → A: Text-to-speech API - Use dedicated TTS service (Google Cloud TTS, Amazon Polly, Azure Speech) to generate and play audio
- Q: How should API credentials for LLM and TTS providers be stored securely in the configuration file? → A: Integration with external secret storage
- Q: What configuration file format should the application use? → A: JSON
- Q: Where should the configuration file be located? → A: Standard user config directory
- Q: Which approach should be supported for external secret storage? → A: Dedicated secret managers - Support specific services like HashiCorp Vault, AWS Secrets Manager, Azure Key Vault

### Session 2025-11-03

- Q: What happens when a word has no direct translation equivalent between Polish and English? → A: Provide the closest conceptual equivalent with an explanation that no direct translation exists
- Q: Should CMU Arpabet pronunciation be returned for source Polish words, English translations, or both? → A: Return CMU Arpabet only for English translation results (not source Polish word)
- Q: Should CMU Arpabet be included when translating English→Polish (for source English words) or only for Polish→English direction? → A: Only include CMU Arpabet for Polish→English direction; skip it for English→Polish
- Q: How should CMU Arpabet phonetic transcriptions be obtained for English words? → A: Request CMU Arpabet from LLM (Claude) along with translations in the same API call
- Q: Should CMU Arpabet differ by part of speech for words with pronunciation variants (e.g., "record" as noun vs verb)? → A: Generate separate CMU Arpabet for each part of speech when word has pronunciation variants
- Q: What should happen when the LLM fails to generate CMU Arpabet or returns invalid format? → A: Display translation without Arpabet and show warning in that specific cell/column
- Q: How does the system handle Polish special characters and diacritics (ą, ć, ę, ł, ń, ó, ś, ź, ż)? → A: Assume full Unicode UTF-8 support throughout the system; display and accept all Polish characters correctly
- Q: What happens when the user is offline and translation services require internet connectivity? → A: Fall back to cache when available, and display a clear error message when there is no cached result
- Q: What happens when the user is offline and LLM providers require internet connectivity? → A: Attempt to use cached responses for previously seen queries and display a clear error message when there is no cache entry
- Q: What happens when the configuration file has syntax errors (invalid JSON)? → A: Display clear error message with line and column information, exit with non-zero status code
- Q: What happens when the configured LLM provider API is down or rate-limited? → A: Display clear error message indicating the provider is unavailable, suggest trying again later, and exit the current operation
- Q: What happens when automatic language detection fails or is uncertain? → A: Display an error message asking user to use explicit language commands (/tp, /te, /ttp, /tte) instead of auto-detect
- Q: What happens when grammar review is requested for non-English text? → A: Detect the language first and display an error message stating grammar review only supports English text
- Q: How does the system handle very long text snippets that may exceed service limits? → A: Enforce a hard limit (5000 characters per FR-024/SC-004) and display clear error message when exceeded
- Q: What happens when audio playback is unavailable or fails? → A: Display clear error message explaining the audio playback failure and suggest checking audio system configuration
- Q: How is history maintained across multiple sessions and what are storage limits? → A: Store unlimited history with no automatic cleanup; let users manage their own data
- Q: What happens when the user enters an invalid REPL command? → A: Display error message showing the invalid command and suggest using /help to see available commands
- Q: What happens when the secret manager is unavailable or fails to return credentials? → A: Display clear error message at startup indicating secret manager connection failure, exit with non-zero status code

## User Scenarios & Testing

### User Story 1 - Word Translation with Detailed Linguistic Information (Priority: P1)

A language learner encounters an unfamiliar Polish word while reading and wants to understand its English meanings, how it's used grammatically, and see it in context.

**Why this priority**: This is the core value proposition of the tool. Understanding individual words with full context is the foundation of language learning and the most frequently needed capability.

**Independent Test**: Can be fully tested by translating a single Polish word and verifying the output shows multiple translations ranked by popularity, parts of speech, countability information, and example sentences.

**Acceptance Scenarios**:

1. **Given** the user enters a Polish noun "kot", **When** the translation is performed, **Then** the system displays English translations ordered by popularity, each with part of speech (noun), countability status (countable), CMU Arpabet phonetic transcription (e.g., "K AE1 T"), and example sentences
2. **Given** the user enters a Polish word with multiple meanings "zamek", **When** the translation is performed, **Then** the system displays all meanings (castle, zipper, lock) ranked by frequency with distinct examples and CMU Arpabet transcriptions for each meaning
3. **Given** the user enters a Polish word that translates to an English word with pronunciation variants "nagrywać", **When** the translation is performed, **Then** the system displays "record" with different CMU Arpabet transcriptions for each part of speech (noun: "R EH1 K ER0 D", verb: "R IH0 K AO1 R D")
4. **Given** the user enters a word in English "dog", **When** reverse translation is performed, **Then** the system displays Polish equivalents without CMU Arpabet (not applicable to English→Polish direction)
5. **Given** the user enters a word that doesn't exist, **When** the translation is attempted, **Then** the system provides a clear error message with suggestions if available

---

### User Story 2 - Text Snippet Translation (Priority: P2)

A user has a sentence or paragraph in Polish that they want to translate to English, or vice versa, to understand the overall meaning quickly.

**Why this priority**: While word translation is foundational, users often need to understand larger chunks of text. This is the second most common use case but depends on translation service availability.

**Independent Test**: Can be fully tested by translating a multi-sentence text snippet and verifying accurate translation output in the target language.

**Acceptance Scenarios**:

1. **Given** the user provides a Polish sentence, **When** text translation is requested, **Then** the system displays the English translation preserving sentence structure and meaning
2. **Given** the user provides an English paragraph, **When** translation to Polish is requested, **Then** the system displays the Polish translation maintaining paragraph structure
3. **Given** the user provides text exceeding 5000 characters, **When** translation is attempted, **Then** the system provides clear feedback about the character limit

---

### User Story 3 - English Grammar and Vocabulary Review (Priority: P3)

A language learner wants to check if their English writing is grammatically correct and identify vocabulary improvements.

**Why this priority**: This adds educational value beyond translation, helping users improve their English writing skills. It's lower priority as it's supplementary to the core translation function.

**Independent Test**: Can be fully tested by submitting an English text with known grammar/vocabulary issues and verifying the system identifies and reports these issues.

**Acceptance Scenarios**:

1. **Given** the user provides English text with grammar errors, **When** review is requested, **Then** the system identifies grammar mistakes and suggests corrections
2. **Given** the user provides grammatically correct English text, **When** review is requested, **Then** the system confirms correctness and may suggest vocabulary enhancements
3. **Given** the user provides English text with both grammar and vocabulary issues, **When** review is requested, **Then** the system categorizes feedback into grammar corrections and vocabulary suggestions

---

### User Story 4 - Audio Pronunciation Playback (Priority: P4)

A user wants to hear how an English word or expression is correctly pronounced to improve their speaking and listening skills.

**Why this priority**: Pronunciation is valuable for language learning but is supplementary to understanding meaning. It's lower priority as it requires audio infrastructure.

**Independent Test**: Can be fully tested by requesting pronunciation of an English word and verifying audio playback occurs with correct pronunciation.

**Acceptance Scenarios**:

1. **Given** the user requests pronunciation for an English word, **When** the playback command is issued, **Then** the system plays audio of the word pronounced correctly
2. **Given** the user requests pronunciation for an English word with part of speech specified (e.g., "record" as noun), **When** the playback command is issued, **Then** the system plays audio with the pronunciation matching that part of speech
3. **Given** the user requests pronunciation for an English phrase, **When** the playback command is issued, **Then** the system plays audio of the entire phrase with natural intonation
4. **Given** the user's system cannot play audio, **When** pronunciation is requested, **Then** the system provides a clear error message about audio capability requirements

---

### User Story 5 - Operation History Tracking (Priority: P5)

A user wants to review their past translation and review activities to revisit words or texts they looked up previously.

**Why this priority**: History tracking adds convenience and learning reinforcement but is not essential for the core translation functionality. It's valuable for long-term learning but lower priority for initial release.

**Independent Test**: Can be fully tested by performing several operations, then requesting history and verifying all operations are recorded with relevant details.

**Acceptance Scenarios**:

1. **Given** the user has performed multiple translation operations, **When** history is requested, **Then** the system displays all past operations with timestamps and original queries
2. **Given** the user requests history, **When** the display is shown, **Then** the history includes operation type (word translation, text translation, grammar review, pronunciation) and key results
3. **Given** the user has no prior operations, **When** history is requested, **Then** the system displays an appropriate message indicating empty history
4. **Given** the user has extensive history, **When** history is requested, **Then** the system displays history in reverse chronological order with reasonable pagination or limits

---

### User Story 6 - Configuration Validation (Priority: P0)

A user wants to run the CLI tool, which requires proper configuration of LLM and TTS providers before any translation features can function.

**Why this priority**: This is a prerequisite for all other functionality. Without proper LLM and TTS provider configuration, no translation, grammar review, or pronunciation operations can be performed. Must be validated before any P1-P5 features.

**Independent Test**: Can be fully tested by running the application with missing or incomplete configuration and verifying the application detects and reports configuration errors at startup.

**Acceptance Scenarios**:

1. **Given** the user runs the application with no configuration file, **When** the application starts, **Then** the system displays a clear error message indicating the missing configuration file and its expected location in the standard user config directory (e.g., `~/.config/translator/config.json` on Linux, `%APPDATA%\translator\config.json` on Windows)
2. **Given** the user runs the application with a malformed JSON configuration file, **When** the application starts, **Then** the system displays a clear error message indicating the JSON syntax error with line and column information
3. **Given** the user runs the application with an incomplete configuration file (missing LLM provider settings), **When** the application starts, **Then** the system displays a clear error message listing the missing required configuration fields
4. **Given** the user runs the application with an incomplete configuration file (missing TTS provider settings), **When** the application starts, **Then** the system displays a clear error message listing the missing required configuration fields
5. **Given** the user runs the application with an incomplete configuration file (missing secret manager settings), **When** the application starts, **Then** the system displays a clear error message listing the missing required configuration fields
6. **Given** the user runs the application with a valid configuration file, **When** the application starts, **Then** the system retrieves credentials from the configured secret manager and launches directly into the REPL
7. **Given** the user runs the application with invalid secret manager connection settings, **When** the application attempts to retrieve credentials, **Then** the system displays a clear error message about the secret manager connection failure
8. **Given** the user runs the application with valid configuration, **When** the application cannot connect to the configured LLM provider, **Then** the system displays a clear error message about the LLM provider connection failure

---

### Edge Cases

- How does the system handle words with no direct translation equivalent between Polish and English? → **Clarified in Session 2025-11-03**: Provide the closest conceptual equivalent with an explanation that no direct translation exists
- How does the system handle Polish special characters and diacritics (ą, ć, ę, ł, ń, ó, ś, ź, ż)? → **Clarified in Session 2025-11-03**: Assume full Unicode UTF-8 support throughout the system; display and accept all Polish characters correctly
- How does the system handle offline scenarios for translation services? → **Clarified in Session 2025-11-03**: Fall back to cache when available, and display a clear error message when there is no cached result
- How does the system handle offline scenarios for LLM providers? → **Clarified in Session 2025-11-03**: Attempt to use cached responses for previously seen queries and display a clear error message when there is no cache entry
- How does the system handle configuration file syntax errors (invalid JSON)? → **Clarified in Session 2025-11-03**: Display clear error message with line and column information, exit with non-zero status code
- How does the system handle LLM provider API downtime or rate limiting? → **Clarified in Session 2025-11-03**: Display clear error message indicating the provider is unavailable, suggest trying again later, and exit the current operation
- How does the system handle ambiguous input that could be either Polish or English when using auto-detect commands (e.g., proper nouns like "Anna", "Roman"; numbers; single-letter words)? → **Clarified in Session 2025-11-03**: `/t` and `/tt` commands use LLM-based language detection via DetectLanguageAsync which should return confidence score; if confidence < 80%, treat as uncertain and display error message per FR-041 asking user to use explicit commands (/tp, /te, /ttp, /tte) instead of auto-detect
- How does the system handle automatic language detection failures or uncertainty? → **Clarified in Session 2025-11-03**: Display an error message asking user to use explicit language commands (/tp, /te, /ttp, /tte) instead of auto-detect
- How does the system handle grammar review requests for non-English text? → **Clarified in Session 2025-11-03**: Detect the language first and display an error message stating grammar review only supports English text
- How does the system handle very long text snippets that may exceed service limits? → **Clarified in Session 2025-11-03**: Enforce a hard limit (5000 characters per FR-024/SC-004) and display clear error message when exceeded
- How does the system handle audio playback unavailability or failures? → **Clarified in Session 2025-11-03**: Display clear error message explaining the audio playback failure and suggest checking audio system configuration
- How is history maintained across multiple sessions and what are storage limits? → **Clarified in Session 2025-11-03**: Store unlimited history with no automatic cleanup; let users manage their own data
- How does the system handle invalid REPL commands? → **Clarified in Session 2025-11-03**: Display error message showing the invalid command and suggest using /help to see available commands
- How does the system handle secret manager unavailability or credential retrieval failures? → **Clarified in Session 2025-11-03**: Display clear error message at startup indicating secret manager connection failure, exit with non-zero status code
- How does the system handle CMU Arpabet generation failures or invalid format responses from LLM? → **Clarified in Session 2025-11-03**: Display translation without Arpabet and show warning in that specific cell/column
- How should the system authenticate to the secret manager (e.g., tokens for Vault, AWS IAM roles for Secrets Manager, Azure managed identity for Key Vault)? → **Clarified in Session 2025-11-02**: Azure Key Vault authentication via DefaultAzureCredential (supports az login for development, managed identity for production)
- Should the application support multiple secret manager backends simultaneously or only one at a time? → **Clarified in Session 2025-11-02**: v1 supports single backend (Azure Key Vault only); multi-backend support deferred to future releases

## Requirements

### Functional Requirements

- **FR-001**: System MUST translate individual Polish words to English showing multiple translations ranked by popularity
- **FR-002**: System MUST translate individual English words to Polish showing multiple translations ranked by popularity
- **FR-003**: System MUST display part of speech information for each word translation
- **FR-004**: System MUST indicate countability status (countable/uncountable) for noun translations
- **FR-005**: System MUST provide example sentences demonstrating usage for each translation
- **FR-005a**: System MUST provide CMU Arpabet phonetic transcription for English word translations when translating Polish→English (not applicable to English→Polish direction)
- **FR-005a1**: System MUST provide part-of-speech-specific CMU Arpabet transcriptions for words with pronunciation variants (e.g., "record" as noun: "R EH1 K ER0 D", as verb: "R IH0 K AO1 R D")
- **FR-005b**: System MUST save CMU Arpabet pronunciation data in operation history and cache for offline access
- **FR-005c**: System MUST gracefully handle CMU Arpabet generation failures by displaying "N/A" in the Arpabet column (consistent with countability column format) rather than failing the entire translation operation
- **FR-006**: System MUST translate text snippets from Polish to English
- **FR-007**: System MUST translate text snippets from English to Polish
- **FR-008**: System MUST review English text and identify grammar errors with correction suggestions
- **FR-009**: System MUST review English text and suggest vocabulary improvements
- **FR-010**: System MUST play audio pronunciation for English words and phrases using a text-to-speech API service
- **FR-010a**: System MUST accept optional part of speech parameter for word pronunciation to handle words with pronunciation variations (e.g., noun vs verb)
- **FR-010b**: System MUST use ElevenLabs as text-to-speech provider (v1 scope: single TTS provider; configurable multi-provider selection for Google Cloud TTS, Amazon Polly, Azure Speech deferred to future releases)
- **FR-011**: System MUST save history of all translation operations
- **FR-012**: System MUST save history of all grammar review operations
- **FR-013**: System MUST save history of all pronunciation requests
- **FR-014**: System MUST allow users to retrieve and view their operation history
- **FR-015**: System MUST handle Polish diacritical characters correctly in all operations
- **FR-016**: System MUST provide clear error messages when operations fail
- **FR-017**: System MUST provide a REPL (Read-Eval-Print Loop) interactive interface
- **FR-017a**: System MUST support `/t <word>` or `/translate <word>` command for word translation with automatic language detection
- **FR-017b**: System MUST support `/tp <word>` or `/translate-polish <word>` command for Polish-to-English word translation
- **FR-017c**: System MUST support `/te <word>` or `/translate-english <word>` command for English-to-Polish word translation
- **FR-017d**: System MUST support `/tt <text>` or `/translate-text <text>` command for text translation with automatic language detection
- **FR-017e**: System MUST support `/ttp <text>` or `/translate-text-polish <text>` command for Polish-to-English text translation
- **FR-017f**: System MUST support `/tte <text>` or `/translate-text-english <text>` command for English-to-Polish text translation
- **FR-017g**: System MUST support `/r <text>` or `/review <text>` command for English grammar and vocabulary review
- **FR-017h**: System MUST support `/p <text>` or `/playback <text>` command for English pronunciation playback with optional part of speech parameter
- **FR-017i**: System MUST support `/history` or `/hist` command to display operation history
- **FR-017j**: System MUST support `/h` or `/help` command to display help information
- **FR-017k**: System MUST support `/c` or `/clear` command to clear the screen
- **FR-017l**: System MUST support `/q` or `/quit` command to quit the application
- **FR-018**: System MUST output results to standard output
- **FR-019**: System MUST output errors to standard error
- **FR-020**: System MUST automatically detect source language (Polish vs English) when using auto-detect commands (`/t`, `/tt`); explicit commands (`/tp`, `/te`, `/ttp`, `/tte`) specify source language without detection. If detection confidence is low (e.g., proper nouns, numbers, ambiguous words), system MUST return error per FR-041 asking user to use explicit language commands.
- **FR-021**: System MUST display word translation results in table format with columns for rank, translation, part of speech, countability, CMU Arpabet (for English translations), and example sentences
- **FR-022**: System MUST accept text input as single-line commands with support for escaped newlines (`\n`) to represent multi-line content
- **FR-023**: System MUST use LLM-based implementation for word translation (including CMU Arpabet generation), text translation, and grammar review functions
- **FR-024**: System MUST leverage LLM's inherent spelling correction capabilities without implementing separate spell-checking logic
- **FR-025**: System MUST use Anthropic Claude as LLM provider (v1 scope: single LLM provider; pluggable multi-provider architecture deferred to future releases)
- **FR-026**: System MUST allow users to configure Anthropic LLM settings (model, temperature, max tokens) via JSON configuration file (v1 scope: single provider configuration; multi-provider operation mapping deferred to future releases)
- **FR-027**: System MUST read configuration from a JSON configuration file located in the standard user config directory (e.g., `~/.config/translator/config.json` on Linux, `%APPDATA%\translator\config.json` on Windows) on startup
- **FR-028**: System MUST validate that all required configuration fields are present at startup
- **FR-029**: System MUST validate configuration file syntax (valid JSON) at startup and provide error messages with line and column information for JSON syntax errors
- **FR-030**: System MUST display clear error messages indicating missing or invalid configuration fields when validation fails
- **FR-031**: System MUST exit with a non-zero status code when configuration validation fails
- **FR-032**: System MUST integrate with Azure Key Vault for securely storing and retrieving API credentials for LLM and TTS providers (v1 scope: Azure Key Vault only; multi-backend support for HashiCorp Vault and AWS Secrets Manager deferred to future releases)
- **FR-033**: System MUST support Azure Key Vault configuration via configuration file (v1 scope: single backend; configurable multi-backend selection deferred to future releases)
- **FR-034**: System MUST store references to secrets (not the actual credentials) in the local configuration file
- **FR-035**: System MUST retrieve actual API credentials from configured secret manager at runtime
- **FR-036**: System MUST provide clear error messages when secret manager is unavailable or fails to return credentials
- **FR-037**: System MUST proceed to REPL when all configuration is valid and credentials are successfully retrieved
- **FR-038**: System MUST support UTF-8 encoding throughout the application to correctly display and accept Polish diacritical characters (ą, ć, ę, ł, ń, ó, ś, ź, ż)
- **FR-039**: System MUST fall back to cached results when offline for translation operations and display clear error message when no cached result is available
- **FR-040**: System MUST fall back to cached LLM responses when offline for LLM provider operations and display clear error message when no cache entry exists
- **FR-041**: System MUST display clear error message when language detection fails or is uncertain, asking user to use explicit language commands (`/tp`, `/te`, `/ttp`, `/tte`) instead of auto-detect commands
- **FR-042**: System MUST detect the language before performing grammar review and display error message stating grammar review only supports English text when non-English text is detected
- **FR-043**: System MUST display error message showing the invalid command and suggest using `/help` or `/h` to see available commands when user enters an invalid REPL command
- **FR-044**: System MUST provide the closest conceptual equivalent with an explanation when a word has no direct translation equivalent between Polish and English
- **FR-045**: System MUST store operation history without automatic cleanup or storage limits, allowing users to manage their own data

### Key Entities

- **Translation Request**: Represents a word or text to be translated, including source language, target language, and the content to translate
- **Translation Result**: Contains translations with popularity ranking, linguistic metadata (part of speech, countability, CMU Arpabet phonetic transcription for English words), and example sentences
- **Grammar Review Request**: Represents English text submitted for grammar and vocabulary analysis
- **Grammar Review Result**: Contains identified errors, correction suggestions, and vocabulary enhancement recommendations
- **Pronunciation Request**: Represents an English word or expression for which audio pronunciation is requested, including optional part of speech for disambiguation
- **Operation History Entry**: Records a single operation including type, timestamp, input, and key results for future retrieval
- **LLM Provider Configuration**: Represents the configuration for LLM provider(s) including provider type, secret reference (not actual credentials), operation-to-provider mapping, and provider-specific settings
- **TTS Provider Configuration**: Represents the configuration for ElevenLabs text-to-speech provider including secret reference (not actual credentials), voice ID, and model settings
- **Secret Manager Configuration**: Represents the configuration for Azure Key Vault including connection settings (Key Vault URL) and authentication method (DefaultAzureCredential via az login or managed identity)

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can translate a single word and receive results in under 3 seconds
- **SC-002**: Word translations display at least 3 different translations when multiple meanings exist
- **SC-003**: 100% of word translations include example sentences demonstrating usage
- **SC-004**: Text snippet translation handles inputs up to 5000 characters without errors
- **SC-005**: Grammar review identifies common grammar mistakes (subject-verb agreement, tense errors, article usage) with 90% accuracy (aspirational target dependent on LLM provider capabilities; actual accuracy measured against manually validated test dataset during integration testing)
- **SC-006**: Pronunciation audio plays within 2 seconds of request
- **SC-007**: Operation history persists across sessions and retains all past operations
- **SC-008**: Users can access their complete operation history in under 1 second
- **SC-009**: System handles Polish diacritical characters without corruption or errors
- **SC-010**: 95% of operations complete successfully with appropriate error messages for failures
- **SC-011**: Configuration validation at startup completes in under 1 second for valid configuration
- **SC-012**: Configuration validation provides actionable error messages listing all missing/invalid fields in a single check

## Assumptions

- Users have internet connectivity for accessing LLM-based translation, grammar review, TTS services, and dedicated secret manager services
- Users have audio playback capability for pronunciation features
- Users have access to Azure Key Vault for storing API credentials (v1 scope)
- Users can manually create and edit JSON configuration files
- Users can authenticate to Azure Key Vault via Azure CLI (az login) or managed identity in production environments
- Users can manually store API credentials in their secret manager and obtain secret references (paths/IDs)
- LLM providers (e.g., OpenAI, Anthropic, Google, etc.) can provide popularity rankings for translations
- LLM providers can provide linguistic metadata (parts of speech, countability, example sentences)
- LLM providers can generate accurate CMU Arpabet phonetic transcriptions for English words
- LLM providers can perform grammar and vocabulary review with high accuracy
- LLM providers inherently handle minor spelling variations and typos in input without requiring explicit spell-checking logic
- ElevenLabs text-to-speech service can generate high-quality English pronunciation audio
- TTS providers support natural-sounding pronunciation with appropriate intonation for both words and phrases
- Operation history, LLM provider configuration (excluding credentials), TTS provider configuration (excluding credentials), and secret manager configuration storage in standard user config directory on local filesystem is acceptable
- Azure Key Vault provides reliable and available access to stored credentials at runtime
- REPL-style interactive interface is suitable for user interaction
- LLM providers can reliably perform language auto-detection to distinguish between Polish and English text
- Text snippet translation limit of 5000 characters is reasonable for CLI usage and aligns with common LLM token limits
- Different LLM providers offer compatible APIs or can be abstracted through a common interface
- ElevenLabs SDK (ElevenLabs-DotNet) provides reliable .NET integration for audio generation
- Azure Key Vault SDK provides reliable .NET integration via Azure.Security.KeyVault.Secrets and Azure.Identity packages
- Users have terminals/consoles that support UTF-8 encoding for displaying Polish diacritical characters
- PostgreSQL database provides sufficient storage capacity for unlimited operation history
- Users are responsible for managing their own database storage and cleaning up old history entries if needed
- Offline cache fallback is acceptable for providing limited functionality when internet connectivity is unavailable
- PostgreSQL database serves as cache storage for both translations and pronunciations to enable offline fallback

