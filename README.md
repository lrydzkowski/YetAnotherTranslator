# YetAnotherTranslator

A CLI tool for Polish-English translation with advanced linguistic features powered by LLM technology.

## Features

- **Word Translation** - Translate individual words between Polish and English with:
    - Multiple translations ranked by popularity
    - Part of speech information
    - Countability status for nouns
    - CMU Arpabet phonetic transcription for English words
    - Example sentences demonstrating usage
- **Text Translation** - Translate text snippets up to 5000 characters between Polish and English
- **Grammar Review** - Analyze English text for:
    - Grammar errors with correction suggestions
    - Vocabulary improvement recommendations
- **Operation History** - Track and review all translation and grammar review operations across sessions
- **Offline Cache** - Fall back to cached results when internet connectivity is unavailable

## Architecture

### Project Structure

```text
src/
├── YetAnotherTranslator.Cli/            - Console application with REPL interface
├── YetAnotherTranslator.Core/           - Domain logic and interfaces
└── YetAnotherTranslator.Infrastructure/ - External service integrations
```

### Technology Stack

- **.NET 10.0** - Modern .NET runtime
- **Azure AI Foundry** - LLM provider for translation and analysis
- **PostgreSQL** - Cache storage and operation history
- **Azure Key Vault** - Secure credential storage
- **Serilog** - Structured logging
- **Entity Framework Core** - Database access
- **FluentValidation** - Request validation
- **Spectre.Console** - Rich console output
- **PrettyPrompt** - Interactive REPL

## Prerequisites

- .NET 10.0 SDK or later
- PostgreSQL database
- Azure Key Vault instance
- API access to Azure AI Foundry
- Azure CLI (`az login`) for development or managed identity for production

## Configuration

### appsettings.json

Create `src/YetAnotherTranslator.Cli/appsettings.json`:

```json
{
  "KeyVault": {
    "VaultName": "your-keyvault-name"
  },
  "AzureAiFoundry": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o",
    "ApiKey": ""
  },
  "Database": {
    "ConnectionString": ""
  },
  "PerformanceLogging": {
    "IsEnabled": false
  }
}
```

### Azure Key Vault Secrets

Store the following secrets in your Azure Key Vault:

- `AzureAiFoundry--ApiKey` - Azure AI Foundry API key
- `Database--ConnectionString` - PostgreSQL connection string

### Authentication

Development:

```bash
az login
```

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run --project src/YetAnotherTranslator.Cli
```

## Usage

### REPL Commands

#### Word Translation

```bash
/t <word>                    # Auto-detect language
/tp <word>                   # Polish to English
/te <word>                   # English to Polish
/translate <word>            # Auto-detect (long form)
/translate-polish <word>     # Polish to English (long form)
/translate-english <word>    # English to Polish (long form)
```

#### Text Translation

```bash
/tt <text>                   # Auto-detect language
/ttp <text>                  # Polish to English
/tte <text>                  # English to Polish
/translate-text <text>       # Auto-detect (long form)
/translate-text-polish <text> # Polish to English (long form)
/translate-text-english <text> # English to Polish (long form)
```

#### Grammar Review

```bash
/r <text>                    # Review English text
/review <text>               # Review (long form)
```

#### History and Navigation

```bash
/history                     # Show last 100 operations
/history --limit 500         # Show last 500 operations
/hist                        # Short form
/h                           # Help
/help                        # Help (long form)
/c                           # Clear screen
/clear                       # Clear screen (long form)
/q                           # Quit
/quit                        # Quit (long form)
```

### Examples

```bash
# Translate a Polish word
/tp kot

# Translate English text
/tte The quick brown fox jumps over the lazy dog

# Review grammar
/r I goes to school yesterday

# View recent history
/history
```
