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
- **PortAudioSharp**: Cross-platform audio playback

**Database**: PostgreSQL with EF Core for operation history and pronunciation cache

**Project Type**: Multi-project solution (Core + Infrastructure + CLI)

## Architecture

### Project Structure

```text
src/
├── YetAnotherTranslator.Core/          # Business logic
│   ├── Handlers/                       # Business logic handlers
│   │   ├── TranslateWordHandler.cs
│   │   ├── TranslateTextHandler.cs
│   │   ├── ReviewGrammarHandler.cs
│   │   └── PlayPronunciationHandler.cs
│   ├── Interfaces/                     # Abstractions for Infrastructure
│   ├── Models/                         # Domain models
│   └── Validation/                     # FluentValidation validators
│
├── YetAnotherTranslator.Infrastructure/  # External integrations
│   ├── Llm/                            # Official Anthropic SDK provider
│   ├── Tts/                            # ElevenLabs provider
│   ├── Secrets/                        # Azure Key Vault provider
│   ├── Persistence/                    # EF Core DbContext, repositories
│   └── Configuration/                  # Config validation
│
└── YetAnotherTranslator.Cli/            # User interface
    ├── Repl/                           # PrettyPrompt integration
    ├── Display/                        # Spectre.Console formatting
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

### Design Patterns

- **Handler Pattern**: Simple handler classes with business logic methods (no MediatR)
- **Clean Architecture**: Core defines interfaces, Infrastructure implements them
- **Dependency Injection**: All dependencies injected via .NET DI container
- **Repository Pattern**: Database access abstracted behind interfaces
- **Cache-Aside**: History table serves as cache with bypass option
- **Direct Invocation**: Handlers called directly from CLI layer for simplicity

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

### Direct Handler Invocation

```csharp
public class TranslateWordHandler
{
    private readonly ILlmProvider _llmProvider;
    private readonly IValidator<TranslateWordRequest> _validator;

    public TranslateWordHandler(ILlmProvider llmProvider, IValidator<TranslateWordRequest> validator)
    {
        _llmProvider = llmProvider;
        _validator = validator;
    }

    public async Task<TranslationResult> HandleAsync(
        string word,
        string sourceLang,
        string targetLang,
        CancellationToken ct = default)
    {
        // Validate
        var request = new TranslateWordRequest(word, sourceLang, targetLang);
        await _validator.ValidateAndThrowAsync(request, ct);

        // Execute
        return await _llmProvider.TranslateWordAsync(word, sourceLang, targetLang);
    }
}

// Usage in CLI
var handler = serviceProvider.GetRequiredService<TranslateWordHandler>();
var result = await handler.HandleAsync(word, "Polish", "English");
```

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

**Last Updated**: 2025-11-02 (automatically updated by `/speckit.plan`)

