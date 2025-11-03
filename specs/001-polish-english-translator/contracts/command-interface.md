# Command Interface Specification

**Feature**: `001-polish-english-translator`
**Date**: 2025-11-02
**Type**: REPL CLI Commands

## Overview

This document specifies the command interface for the YetAnotherTranslator REPL. All commands start with `/` and support both short and long forms.

## Global Conventions

### Command Format

```text
/<command> [options] <argument>
```

### Options

All translation and review commands support the `--no-cache` option:

```text
/<command> --no-cache <argument>
```

When `--no-cache` is specified, the system bypasses cache lookup and always calls the external API.

### Input Handling

- **Single-line text**: Direct input after command
- **Multi-line text**: Use escaped newlines (`\n`) within single-line input
- **Special characters**: Polish diacritics (ą, ć, ę, ł, ń, ó, ś, ź, ż) supported

### Output Streams

- **Standard output**: Successful results
- **Standard error**: Error messages and warnings

### Exit Codes

- `0`: Successful operation
- `1`: Configuration error, authentication failure, or startup failure
- (REPL does not exit on command errors, only displays error)

## Translation Commands

### 1. Translate Word (Auto-Detect)

**Syntax**:

```text
/t <word>
/translate <word>
```

**Description**: Translates a single word, automatically detecting source language.

**Parameters**:
- `word` (required): Word to translate (max 100 characters)

**Options**:
- `--no-cache`: Bypass cache and fetch fresh translation

**Examples**:

```text
> /t hello
> /translate dzień dobry
> /t --no-cache cat
```

**Output**: Table with columns:
- Rank (#)
- Translation
- Part of Speech
- Countability (for nouns)
- CMU Arpabet (for English translations only when translating Polish→English)
- Example sentence

**Success Output Example (Polish→English)**:

```text
╭───┬─────────────┬────────────────┬──────────────┬─────────────┬────────────────────────────────────────╮
│ # │ Translation │ Part of Speech │ Countability │ CMU Arpabet │ Example                                │
├───┼─────────────┼────────────────┼──────────────┼─────────────┼────────────────────────────────────────┤
│ 1 │ cat         │ noun           │ countable    │ K AE1 T     │ The cat sat on the mat.                │
│ 2 │ tomcat      │ noun           │ countable    │ T AA1 M K AE2 T │ The tomcat roamed the neighborhood. │
╰───┴─────────────┴────────────────┴──────────────┴─────────────┴────────────────────────────────────────╯
```

**Success Output Example (English→Polish, no CMU Arpabet)**:

```text
╭───┬─────────────┬────────────────┬──────────────┬────────────────────────────────────────╮
│ # │ Translation │ Part of Speech │ Countability │ Example                                │
├───┼─────────────┼────────────────┼──────────────┼────────────────────────────────────────┤
│ 1 │ cześć       │ interjection   │ N/A          │ Cześć, jak się masz?                   │
│ 2 │ witaj       │ interjection   │ N/A          │ Witaj z powrotem!                      │
╰───┴─────────────┴────────────────┴──────────────┴────────────────────────────────────────╯
```

**Error Cases**:
- Empty word → "Word cannot be empty"
- Word too long → "Word must be less than 100 characters"
- Language detection failed → "Unable to detect language for '<word>'"
- LLM API error → "Translation failed: <error message>"

### 2. Translate Polish Word

**Syntax**:

```text
/tp <word>
/translate-polish <word>
```

**Description**: Translates a Polish word to English (no auto-detection).

**Parameters**:
- `word` (required): Polish word to translate (max 100 characters)

**Options**:
- `--no-cache`: Bypass cache

**Examples**:

```text
> /tp kot
> /translate-polish --no-cache zamek
```

**Output**: Same table format as `/t`

**Error Cases**: Same as `/t`

### 3. Translate English Word

**Syntax**:

```text
/te <word>
/translate-english <word>
```

**Description**: Translates an English word to Polish (no auto-detection).

**Parameters**:
- `word` (required): English word to translate (max 100 characters)

**Options**:
- `--no-cache`: Bypass cache

**Examples**:

```text
> /te dog
> /translate-english --no-cache castle
```

**Output**: Same table format as `/t`

**Error Cases**: Same as `/t`

### 4. Translate Text (Auto-Detect)

**Syntax**:

```text
/tt <text>
/translate-text <text>
```

**Description**: Translates a text snippet, automatically detecting source language.

**Parameters**:
- `text` (required): Text to translate (max 5000 characters)

**Options**:
- `--no-cache`: Bypass cache

**Examples**:

```text
> /tt The quick brown fox jumps over the lazy dog
> /translate-text Dzień dobry, jak się masz?
> /tt --no-cache This is line one.\nThis is line two.
```

**Output**: Plain text translation

**Success Output Example**:

```text
Translation (Polish → English):

The quick brown fox jumps over the lazy dog
```

**Error Cases**:
- Empty text → "Text cannot be empty"
- Text too long → "Text must be less than 5000 characters (current: <count>)"
- Language detection failed → "Unable to detect language for text"
- LLM API error → "Translation failed: <error message>"

### 5. Translate Polish Text

**Syntax**:

```text
/ttp <text>
/translate-text-polish <text>
```

**Description**: Translates Polish text to English (no auto-detection).

**Parameters**:
- `text` (required): Polish text to translate (max 5000 characters)

**Options**:
- `--no-cache`: Bypass cache

**Examples**:

```text
> /ttp Cześć, co słychać?
> /translate-text-polish --no-cache Idę do sklepu.
```

**Output**: Same format as `/tt`

**Error Cases**: Same as `/tt`

### 6. Translate English Text

**Syntax**:

```text
/tte <text>
/translate-text-english <text>
```

**Description**: Translates English text to Polish (no auto-detection).

**Parameters**:
- `text` (required): English text to translate (max 5000 characters)

**Options**:
- `--no-cache`: Bypass cache

**Examples**:

```text
> /tte Hello, how are you?
> /translate-text-english --no-cache I am going to the store.
```

**Output**: Same format as `/tt`

**Error Cases**: Same as `/tt`

## Grammar Review Commands

### 7. Review English Text

**Syntax**:

```text
/r <text>
/review <text>
```

**Description**: Reviews English text for grammar errors and suggests vocabulary improvements.

**Parameters**:
- `text` (required): English text to review (max 5000 characters)

**Options**:
- `--no-cache`: Bypass cache

**Examples**:

```text
> /r I are going to store
> /review --no-cache The cat sit on the mat yesterday.
```

**Output**:

```text
Grammar Review:

Status: ❌ Issues found

Grammar Issues:
1. Subject-verb agreement
   Original: "I are going"
   Correction: "I am going"
   Explanation: Subject "I" requires verb "am", not "are"

2. Missing article
   Original: "going to store"
   Correction: "going to the store"
   Explanation: Countable noun "store" requires article "the" or "a"

Vocabulary Suggestions:
(none)
```

**Success Output (No Issues)**:

```text
Grammar Review:

Status: ✓ No issues found

The text is grammatically correct.

Vocabulary Suggestions:
1. "big" → "large"
   Reason: More formal and sophisticated
```

**Error Cases**:
- Empty text → "Text cannot be empty"
- Text too long → "Text must be less than 5000 characters (current: <count>)"
- Non-English text → "Grammar review only supports English text"
- LLM API error → "Grammar review failed: <error message>"

## Pronunciation Commands

### 8. Play Pronunciation

**Syntax**:

```text
/p <text> [part-of-speech]
/playback <text> [part-of-speech]
```

**Description**: Plays English pronunciation audio for a word or expression.

**Parameters**:
- `text` (required): English text to pronounce (max 500 characters)
- `part-of-speech` (optional): Part of speech for disambiguation (noun, verb, adjective, adverb)

**Options**:
- `--no-cache`: Bypass cache and generate fresh audio

**Examples**:

```text
> /p hello
> /playback record noun
> /p --no-cache present verb
> /playback How are you?
```

**Output**:

```text
Playing pronunciation...
▶ "hello"
✓ Playback complete (1.2s)
```

**Error Cases**:
- Empty text → "Text cannot be empty"
- Text too long → "Text must be less than 500 characters"
- Invalid part of speech → "Invalid part of speech '<pos>'. Valid options: noun, verb, adjective, adverb"
- TTS API error → "Pronunciation playback failed: <error message>"
- Audio playback error → "Audio playback unavailable: <error message>. Check audio drivers."

## History Commands

### 9. View History

**Syntax**:

```text
/history [count]
/hist [count]
```

**Description**: Displays operation history in reverse chronological order.

**Parameters**:
- `count` (optional): Number of entries to display (default: 10, max: 100)

**Examples**:

```text
> /history
> /hist 25
```

**Output**:

```text
Operation History (Last 10 entries):

╭─────────────────────────┬───────────────┬─────────────────┬──────────────────╮
│ Timestamp               │ Command Type  │ Input           │ Summary          │
├─────────────────────────┼───────────────┼─────────────────┼──────────────────┤
│ 2025-11-02 14:23:45 UTC │ TranslateWord │ kot             │ 3 translations   │
│ 2025-11-02 14:22:10 UTC │ Pronunciation │ hello           │ Audio played     │
│ 2025-11-02 14:20:05 UTC │ TranslateText │ Dzień dobry...  │ Translated       │
│ 2025-11-02 14:18:30 UTC │ ReviewGrammar │ I are going...  │ 2 issues found   │
╰─────────────────────────┴───────────────┴─────────────────┴──────────────────╯
```

**Empty History Output**:

```text
Operation History:

No operations in history yet.
```

**Error Cases**:
- Invalid count → "Count must be between 1 and 100"
- Database error → "Failed to retrieve history: <error message>"

## Utility Commands

### 10. Help

**Syntax**:

```text
/h
/help
```

**Description**: Displays help information with all available commands.

**Examples**:

```text
> /help
> /h
```

**Output**:

```text
Yet Another Translator - Command Reference

Translation Commands:
  /t, /translate <word>              Translate word (auto-detect language)
  /tp, /translate-polish <word>      Translate Polish word to English
  /te, /translate-english <word>     Translate English word to Polish
  /tt, /translate-text <text>        Translate text (auto-detect language)
  /ttp, /translate-text-polish <text> Translate Polish text to English
  /tte, /translate-text-english <text> Translate English text to Polish

Grammar & Vocabulary:
  /r, /review <text>                 Review English text for grammar and vocabulary

Pronunciation:
  /p, /playback <text> [pos]         Play English pronunciation
                                     Optional POS: noun, verb, adjective, adverb

History:
  /history [count]                   View operation history (default: 10 entries)
  /hist [count]

Utility:
  /h, /help                          Display this help message
  /c, /clear                         Clear the screen
  /q, /quit                          Quit the application

Options:
  --no-cache                         Bypass cache for current command

Examples:
  > /t hello
  > /tp --no-cache kot
  > /r I are going to store
  > /p record noun
  > /history 25
```

### 11. Clear Screen

**Syntax**:

```text
/c
/clear
```

**Description**: Clears the terminal screen.

**Examples**:

```text
> /clear
> /c
```

**Output**: Terminal screen cleared, returns to prompt

### 12. Quit

**Syntax**:

```text
/q
/quit
```

**Description**: Exits the REPL and terminates the application.

**Examples**:

```text
> /quit
> /q
```

**Output**:

```text
Goodbye!
```

**Exit Code**: 0 (success)

## Error Handling

### Invalid Command

**Input**:

```text
> /invalid
> /xyz test
```

**Output**:

```text
Error: Unknown command '/invalid'
Type '/help' to see available commands.
```

### Missing Argument

**Input**:

```text
> /t
> /translate
```

**Output**:

```text
Error: Missing required argument <word>
Usage: /t <word>
```

### API Failures

**LLM Provider Unavailable**:

```text
Error: LLM provider unavailable
Failed to connect to Anthropic API: Connection timeout
Please check your internet connection and try again.
```

**TTS Provider Unavailable**:

```text
Error: TTS provider unavailable
Failed to connect to ElevenLabs API: 429 Rate Limit Exceeded
Please try again in a few moments.
```

**Database Unavailable**:

```text
Warning: Database unavailable
Failed to save to history: Connection refused
Operation completed successfully but was not cached.
```

## Performance Requirements

| Command | Target Response Time | Success Criteria |
|---------|---------------------|------------------|
| `/t` (word) | < 3s | SC-001 |
| `/tt` (text) | < 5s | N/A |
| `/r` (review) | < 5s | N/A |
| `/p` (pronunciation) | < 2s | SC-006 |
| `/history` | < 1s | SC-008 |

## Command State Machine

```text
[Start REPL]
     │
     ├─> [Display Welcome]
     │
     ├─> [Prompt for Command]
     │        │
     │        ├─> Valid Command ──> Execute ──> Display Result ──┐
     │        │                                                   │
     │        ├─> Invalid Command ──> Display Error ─────────────┤
     │        │                                                   │
     │        └─> /quit ──> [Exit REPL] ──> Exit(0)             │
     │                                                            │
     └────────────────────────────────────────────────────────────┘
           (Loop back to Prompt)
```

## Validation Summary

All commands implement input validation per FluentValidation rules defined in data-model.md:

- Word length: 1-100 characters
- Text length: 1-5000 characters
- Language codes: "Polish" | "English" | "Auto"
- Part of speech: "noun" | "verb" | "adjective" | "adverb" | null
- History count: 1-100

## Caching Behavior

### Cache Lookup (Default)

1. Generate cache key from command parameters
2. Query cache table
3. If hit: Return cached result
4. If miss: Call API, save to cache, return result

### Cache Bypass (`--no-cache`)

1. Skip cache lookup
2. Call API directly
3. Save result to cache (overwrite if exists)
4. Return fresh result

### Cache Invalidation

- 30-day expiration policy
- LRU eviction when database size limit exceeded
- Manual invalidation not supported in v1

## Summary

The command interface provides:
- ✓ 12 total commands covering all functional requirements
- ✓ Short and long command aliases for usability
- ✓ Consistent `--no-cache` option across all data-fetching commands
- ✓ Clear error messages for all failure scenarios
- ✓ Performance targets aligned with success criteria
- ✓ Input validation aligned with data model
- ✓ Standard CLI conventions (stdout/stderr/exit codes)
