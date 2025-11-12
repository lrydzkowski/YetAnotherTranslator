# YetAnotherTranslator Project Context

This file provides technology context for AI coding assistants working on this project.

## Project Overview

Polish-English Translation CLI Tool - A REPL-style command-line interface for translation, grammar review, and pronunciation playback.

## Technology Stack

**Language**: .NET 10 (C#)

**Key Dependencies**:

- **Official Anthropic SDK**: LLM provider for Claude Sonnet 4.5 translations and grammar review
- **PrettyPrompt**: REPL input with syntax highlighting and history
- **Spectre.Console**: Formatted CLI output (tables, colors, markup)
- **FluentValidation**: Input validation
- **EF Core**: ORM for PostgreSQL
- **Azure SDK (Key Vault)**: Secrets management
- **ElevenLabs SDK**: Text-to-speech for pronunciation
- **xUnit**: Testing framework
- **NSubstitute**: Mocking library
- **Testcontainers**: PostgreSQL containers for integration tests
- **WireMock.Net**: External API mocking for integration tests
- **Verify**: Snapshot testing for complex assertions
- **NAudio**: Cross-platform MP3 audio playback

**Database**: PostgreSQL with EF Core for operation history and pronunciation cache

**Project Type**: Multi-project solution (Core + Infrastructure + CLI)

## Architecture

### Project Structure

```text
src/
├── YetAnotherTranslator.Core/          # Business logic (no external dependencies)
│   ├── Handlers/                       # Self-contained handler namespaces
│   │   ├── TranslateWord/              # Word translation feature
│   │   │   ├── TranslateWordHandler.cs          # Main handler
│   │   │   ├── TranslateWordRequest.cs          # Request model
│   │   │   ├── TranslateWordValidator.cs        # FluentValidation validator
│   │   │   ├── TranslationResult.cs             # Result model
│   │   │   └── Translation.cs                   # Nested translation model
│   │   │
│   │   ├── TranslateText/              # Text translation feature
│   │   │   ├── TranslateTextHandler.cs
│   │   │   ├── TranslateTextRequest.cs
│   │   │   ├── TranslateTextValidator.cs
│   │   │   └── TextTranslationResult.cs
│   │   │
│   │   ├── ReviewGrammar/              # Grammar review feature
│   │   │   ├── ReviewGrammarHandler.cs
│   │   │   ├── ReviewGrammarRequest.cs
│   │   │   ├── ReviewGrammarValidator.cs
│   │   │   ├── GrammarReviewResult.cs
│   │   │   ├── GrammarIssue.cs
│   │   │   └── VocabularySuggestion.cs
│   │   │
│   │   ├── PlayPronunciation/          # Pronunciation playback feature
│   │   │   ├── PlayPronunciationHandler.cs
│   │   │   ├── PlayPronunciationRequest.cs
│   │   │   ├── PlayPronunciationValidator.cs
│   │   │   └── PronunciationResult.cs
│   │   │
│   │   └── GetHistory/                 # History retrieval feature
│   │       ├── GetHistoryHandler.cs
│   │       ├── GetHistoryRequest.cs
│   │       ├── GetHistoryValidator.cs
│   │       ├── HistoryEntry.cs
│   │       └── CommandType.cs
│   │
│   └── Interfaces/                     # Shared interfaces (ILlmProvider, ITtsProvider, etc.)
│
├── YetAnotherTranslator.Infrastructure/  # External integrations
│   ├── Llm/                            # Official Anthropic SDK provider
│   │   └── AnthropicProvider.cs        # Implements ILlmProvider
│   ├── Tts/                            # ElevenLabs provider
│   │   ├── ElevenLabsProvider.cs       # Implements ITtsProvider
│   │   └── PortAudioPlayer.cs          # Implements IAudioPlayer (NAudio-based)
│   ├── Secrets/                        # Azure Key Vault provider
│   │   └── AzureKeyVaultProvider.cs    # Implements ISecretsProvider
│   ├── Persistence/                    # EF Core DbContext, repositories
│   │   ├── TranslatorDbContext.cs
│   │   ├── Entities/                   # Database entities
│   │   ├── Repositories/               # Repository implementations
│   │   └── Migrations/                 # EF Core migrations
│   └── Configuration/                  # Config validation
│       ├── ApplicationConfiguration.cs
│       └── Validators/                 # FluentValidation validators for config
│
└── YetAnotherTranslator.Cli/            # User interface
    ├── Repl/                           # PrettyPrompt integration
    │   ├── ReplEngine.cs               # Main REPL loop
    │   ├── CommandParser.cs            # Parse user commands
    │   └── Command.cs                  # Command model
    ├── Display/                        # Spectre.Console formatting
    │   └── DisplayFormatter.cs         # Table formatting
    └── Program.cs                      # Entry point, DI setup

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
    └── Helpers/
        └── VerifySettings.cs           # Verify snapshot configuration
```

### Design Patterns

- **Self-Contained Handlers**: Each handler resides in its own namespace (`YetAnotherTranslator.Core.Handlers.{HandlerName}`) with its models, validator, and any necessary interfaces. This promotes modularity and makes each feature independently understandable.
- **Clean Architecture**: Core has zero dependencies on Infrastructure or CLI; Infrastructure references Core to implement interfaces; CLI references both
- **Dependency Injection**: All dependencies injected via .NET DI container
- **Repository Pattern**: Database access abstracted behind IHistoryRepository interface
- **Cache-Aside**: History table serves as cache with bypass option (--no-cache)
- **Direct Invocation**: Handlers called directly from CLI layer for simplicity (no MediatR)

## Configuration

Configuration stored in JSON at standard user config directory:

- Windows: `%APPDATA%\translator\config.json`
- macOS/Linux: `~/.config/translator/config.json`

Secrets stored in Azure Key Vault, accessed via `az login` authentication.

## Key Technologies Usage

### Official Anthropic SDK

```csharp
using Anthropic;
using Anthropic.Models.Messages;
using Anthropic.Models.Messages.MessageParamProperties;

var client = new AnthropicClient(apiKey);
var parameters = new MessageCreateParams
{
    MaxTokens = 2048,
    Messages = new[]
    {
        new Message
        {
            Role = Role.User,
            Content = "Translate 'kot' to English"
        }
    },
    Model = Model.ClaudeSonnet4_0,
    Temperature = 0.3
};
var response = await client.Messages.Create(parameters);
```

### PrettyPrompt + Spectre.Console

```csharp
var prompt = new Prompt();
while (true)
{
    var response = await prompt.ReadLineAsync("> ");
    if (response.IsSuccess)
    {
        var result = await ProcessCommand(response.Text);
        var table = new Table();
        table.AddColumn("Translation");
        table.AddRow(result);
        AnsiConsole.Write(table);
    }
}
```

### EF Core + PostgreSQL

```csharp
public class TranslatorDbContext : DbContext
{
    public DbSet<HistoryEntry> HistoryEntries { get; set; }
    public DbSet<TranslationCache> TranslationCache { get; set; }
}
```

### Azure Key Vault

```csharp
var credential = new DefaultAzureCredential();
var client = new SecretClient(new Uri(keyVaultUrl), credential);
var secret = await client.GetSecretAsync("anthropic-api-key");
```

### Self-Contained Handler Example

**File**: `src/YetAnotherTranslator.Core/Handlers/TranslateWord/TranslateWordHandler.cs`

```csharp
namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public class TranslateWordHandler
{
    private readonly ILlmProvider _llmProvider;
    private readonly IValidator<TranslateWordRequest> _validator;
    private readonly IHistoryRepository _historyRepository;

    public TranslateWordHandler(
        ILlmProvider llmProvider,
        IValidator<TranslateWordRequest> validator,
        IHistoryRepository historyRepository)
    {
        _llmProvider = llmProvider;
        _validator = validator;
        _historyRepository = historyRepository;
    }

    public async Task<TranslationResult> HandleAsync(
        string word,
        string sourceLang,
        string targetLang,
        bool useCache = true,
        CancellationToken ct = default)
    {
        var request = new TranslateWordRequest(word, sourceLang, targetLang);
        await _validator.ValidateAndThrowAsync(request, ct);

        if (useCache)
        {
            var cached = await _historyRepository.GetCachedTranslationAsync(word, sourceLang);
            if (cached != null)
                return cached;
        }

        var result = await _llmProvider.TranslateWordAsync(word, sourceLang, targetLang);

        await _historyRepository.SaveHistoryAsync(
            new HistoryEntry
            {
                CommandType = CommandType.TranslateWord,
                InputText = word,
                OutputText = JsonSerializer.Serialize(result),
                Timestamp = DateTime.UtcNow
            });

        return result;
    }
}
```

**Usage in CLI**:

```csharp
// In CLI command processor
using YetAnotherTranslator.Core.Handlers.TranslateWord;

var handler = serviceProvider.GetRequiredService<TranslateWordHandler>();
var result = await handler.HandleAsync(word, "Polish", "English");
```

**Key Points**:
- Handler is in its own namespace: `YetAnotherTranslator.Core.Handlers.TranslateWord`
- All models (TranslateWordRequest, TranslationResult) are in the same namespace
- Validator (TranslateWordValidator) is in the same namespace
- Interfaces (ILlmProvider, IHistoryRepository) are shared from `Core.Interfaces`
- Handler is self-contained and independently understandable

## Testing Approach

**Integration tests only** - No unit tests. Each feature is tested end-to-end with:

- **Real PostgreSQL**: Running in Docker containers via Testcontainers
- **Mocked External APIs**: Anthropic, ElevenLabs, Azure Key Vault mocked with WireMock.Net
- **NSubstitute**: For any needed mocking of interfaces
- **Verify**: Snapshot testing for asserting complex objects (e.g., LLM responses, formatted output)
- **FluentAssertions**: Readable assertions for simple cases

Example test with Verify:

```csharp
[Fact]
public async Task TranslateWord_ShouldReturnTranslations()
{
    var handler = serviceProvider.GetRequiredService<TranslateWordHandler>();
    var result = await handler.HandleAsync("kot", "Polish", "English");

    // Snapshot testing - compares result to verified snapshot
    await Verify(result);
}
```

This approach:

- Tests actual database behavior (JSONB, indexes, transactions)
- Verifies real API integration patterns
- Catches configuration issues early
- Avoids false confidence from in-memory databases
- Maintainable complex assertions with Verify snapshots

## Special Considerations

### Performance Requirements

- Word translation: <3s (SC-001)
- Pronunciation playback: <2s (SC-006)
- History retrieval: <1s (SC-008)
- Configuration validation: <1s (SC-011)

### Caching Strategy

- All translations and pronunciations cached in PostgreSQL
- Cache key: SHA256 hash of input parameters
- 30-day expiration policy
- `--no-cache` option to bypass cache

### Error Handling

- Fail fast at startup if configuration invalid
- Clear error messages for all failure scenarios
- Retry logic with exponential backoff for transient failures
- All errors include context for debugging

## Development Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project src/YetAnotherTranslator.Cli

# Create migration
dotnet ef migrations add <MigrationName> \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli

# Update database
dotnet ef database update \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli
```

## Documentation

- Feature Spec: `specs/001-polish-english-translator/spec.md`
- Implementation Plan: `specs/001-polish-english-translator/plan.md`
- Data Model: `specs/001-polish-english-translator/data-model.md`
- Command Interface: `specs/001-polish-english-translator/contracts/command-interface.md`
- Quickstart: `specs/001-polish-english-translator/quickstart.md`
- Research: `specs/001-polish-english-translator/research.md`

## Constitution Principles

See `.specify/memory/constitution.md` for full details. Key principles:

- Single responsibility per component
- Testability through dependency injection
- CLI standard conventions (stdout/stderr/exit codes)
- Fail fast with descriptive errors
- Simplicity over cleverness

---

**Last Updated**: 2025-11-04 (automatically updated by `/speckit.plan`)

