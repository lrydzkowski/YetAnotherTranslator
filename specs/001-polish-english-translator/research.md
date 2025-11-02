# Research: Polish-English Translation CLI Tool

**Feature**: `001-polish-english-translator`
**Date**: 2025-11-02
**Phase**: Phase 0 - Outline & Research

## Overview

This document consolidates research findings for all NEEDS CLARIFICATION items from the Technical Context and best practices for key technology choices. Each section provides decision rationale, alternatives considered, and implementation recommendations.

## 1. Testing Framework

### Decision

**xUnit** with **NSubstitute** (mocking), **Testcontainers** (PostgreSQL containers), **WireMock.Net** (external API mocking), and **Verify** (snapshot testing)

### Rationale

- **xUnit**: Most widely adopted testing framework in modern .NET projects with excellent .NET 10 support
- **NSubstitute**: More elegant and readable mocking syntax compared to Moq
- **Testcontainers**: Provides real PostgreSQL instances in Docker containers for integration tests, avoiding in-memory database limitations
- **WireMock.Net**: Allows mocking external HTTP APIs (Anthropic, ElevenLabs, Azure Key Vault) with realistic responses
- **Verify**: Snapshot testing library that makes asserting complex objects easy by comparing to verified snapshots - perfect for testing LLM response parsing, JSON serialization, and formatted output

### Testing Strategy

**Integration tests only** - Test individual features end-to-end with:

1. Real PostgreSQL database running in Docker container (Testcontainers)
2. External APIs (LLM, TTS, Key Vault) mocked with WireMock.Net
3. Complex object assertions using Verify snapshot testing
4. No unit tests - integration tests provide better confidence and catch real issues

### Alternatives Considered

- **Moq**: Popular but NSubstitute has cleaner syntax
- **In-memory database**: Doesn't test real PostgreSQL behavior (e.g., JSONB, indexes)
- **Unit tests with mocks**: Less valuable than integration tests that verify actual behavior
- **Manual assertions**: Verbose and brittle compared to snapshot testing with Verify

### Implementation Recommendations

```xml
<PackageReference Include="xunit" Version="2.9.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="NSubstitute" Version="5.1.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
<PackageReference Include="WireMock.Net" Version="1.6.4" />
<PackageReference Include="Verify.Xunit" Version="26.6.0" />
```

**Integration Test Structure**:

```csharp
public class TranslateWordTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer;
    private WireMockServer _anthropicMock;
    private IServiceProvider _serviceProvider;

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
        await _postgresContainer.StartAsync();

        // Start WireMock server for Anthropic API
        _anthropicMock = WireMockServer.Start();
        _anthropicMock
            .Given(Request.Create()
                .WithPath("/v1/messages")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { /* mock response */ }));

        // Build service provider with real DB and mocked APIs
        _serviceProvider = BuildServiceProvider(
            _postgresContainer.GetConnectionString(),
            _anthropicMock.Url);
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        _anthropicMock.Stop();
    }

    [Fact]
    public async Task TranslateWord_ShouldReturnTranslations()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<TranslateWordHandler>();

        // Act
        var result = await handler.HandleAsync("kot", "Polish", "English");

        // Assert using Verify snapshot testing
        await Verify(result);
    }
}
```

**Verify Usage**:

Verify makes asserting complex objects simple. Instead of writing brittle manual assertions:
```csharp
// Manual assertions (brittle)
result.Should().NotBeNull();
result.Translations.Should().HaveCount(3);
result.Translations[0].Word.Should().Be("cat");
result.Translations[0].PartOfSpeech.Should().Be("noun");
// ... many more assertions
```

Use snapshot testing:
```csharp
// Verify snapshot (maintainable)
await Verify(result);
```

First run creates a `.verified.txt` file with the serialized result. Subsequent runs compare against this snapshot. When output changes intentionally, review the diff and accept the new snapshot.

**Key Benefits**:

- Tests verify actual database behavior (JSONB, indexes, transactions)
- Tests verify actual API integration patterns
- No false confidence from mocked database behavior
- Integration tests catch configuration issues early
- Testcontainers automatically cleans up resources
- Verify snapshots make complex assertions maintainable and catch unintended changes

## 2. Anthropic .NET SDK for Claude Sonnet 4.5

### Decision

**Official Anthropic SDK** (currently in beta)

### Rationale

- Official SDK from Anthropic with guaranteed long-term support
- Follows official SDK design patterns from Python/TypeScript SDKs
- Direct access to latest features and models (Claude 3.5 Sonnet, Haiku 4.5)
- Supports streaming, tool use, prompt caching
- Designed for .NET Standard 2.0+ (compatible with .NET 10)
- Beta status is acceptable for new projects where breaking changes can be managed

### Alternatives Considered

- **Anthropic.SDK (tghamm)**: Popular unofficial SDK but using official SDK avoids reliance on third-party maintenance
- **Claudia (Cysharp)**: Well-designed but less mature than official SDK
- **Direct REST API**: More control but requires manual HTTP client management

### Implementation Recommendations

**Installation**:

```bash
# NuGet package name: Anthropic (check NuGet.org for latest version)
dotnet add package Anthropic
```

**Repository**: https://github.com/anthropics/anthropic-sdk-csharp

**Basic Usage Pattern**:

```csharp
using Anthropic;
using Anthropic.Models.Messages;
using Anthropic.Models.Messages.MessageParamProperties;

public class AnthropicProvider : ILlmProvider
{
    private readonly AnthropicClient _client;

    public AnthropicProvider(string apiKey)
    {
        _client = new AnthropicClient(apiKey);
    }

    public async Task<TranslationResult> TranslateWordAsync(
        string word,
        string sourceLanguage,
        string targetLanguage)
    {
        var parameters = new MessageCreateParams
        {
            MaxTokens = 2048,
            Messages = new[]
            {
                new Message
                {
                    Role = Role.User,
                    Content = $"Translate the {sourceLanguage} word '{word}' to {targetLanguage}. " +
                              "Provide multiple translations ranked by popularity, with part of speech, " +
                              "countability information, and example sentences for each."
                }
            },
            Model = Model.ClaudeSonnet4_0,
            Temperature = 0.3
        };

        var response = await _client.Messages.Create(parameters);

        return ParseTranslationResponse(response);
    }
}
```

**Best Practices**:

- Use dependency injection to inject `ILlmProvider` interface
- Store API key in Azure Key Vault, not in configuration
- Client can use `ANTHROPIC_API_KEY` environment variable by default
- Implement retry logic with exponential backoff for transient failures
- Use prompt caching for repeated system prompts to reduce costs
- Set appropriate `MaxTokens` based on operation type (word: 2048, text: 4096)
- Use lower temperature (0.3) for translations, higher (0.7) for grammar review

## 3. ElevenLabs SDK for Text-to-Speech

### Decision

**ElevenLabs-DotNet v3.6.0+** by RageAgainstThePixel

### Rationale

- Targets .NET 8.0+ (compatible with .NET 10)
- Cross-platform support (Windows, Linux, macOS)
- Works across console apps, WinForms, WPF, ASP.NET
- Active maintenance and community support
- Simple API with async/await patterns
- Returns audio data as byte arrays for easy caching

### Alternatives Considered

- **elevenlabs-dotnet-sdk (newtro)**: Comprehensive but newer, less proven
- **Direct REST API**: More control but requires manual HTTP client management

### Implementation Recommendations

**Installation**:

```bash
dotnet add package ElevenLabs-DotNet --version 3.6.0
```

**Basic Usage Pattern**:

```csharp
public class ElevenLabsProvider : ITtsProvider
{
    private readonly ElevenLabsClient _client;

    public ElevenLabsProvider(string apiKey)
    {
        _client = new ElevenLabsClient(apiKey);
    }

    public async Task<byte[]> GenerateSpeechAsync(
        string text,
        string voiceId = null)
    {
        var voice = voiceId != null
            ? new Voice(voiceId)
            : (await _client.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();

        var request = new TextToSpeechRequest(voice, text);
        var audioClip = await _client.TextToSpeechEndpoint.TextToSpeechAsync(request);

        return audioClip.ClipData.ToArray();
    }
}
```

**Audio Playback**:
For cross-platform audio playback, use **PortAudioSharp** or **Bufdio**:

```bash
dotnet add package PortAudioSharp --version 0.3.0
```

OR for more features:

```bash
dotnet add package Bufdio
```

**Caching Strategy**:

- Store audio byte arrays in PostgreSQL with text + voice ID as cache key
- Use hash of text for efficient lookups
- Set reasonable cache expiration (30 days) to manage database size
- Implement `--no-cache` option to bypass cache and fetch fresh audio

**Best Practices**:

- Cache all TTS responses to minimize API calls and costs
- Use consistent voice ID for all pronunciations
- Implement timeout for TTS requests (5 seconds)
- Handle rate limiting with retry logic
- Store audio format metadata with cached data

## 4. PrettyPrompt + Spectre.Console Integration

### Decision

Use **PrettyPrompt v4.x** for REPL input and **Spectre.Console v0.49+** for formatted output

### Rationale

- **PrettyPrompt**: Provides syntax highlighting, autocompletion, history, multi-line support
- **Spectre.Console**: Excellent table rendering, exception formatting, color support
- Both libraries are complementary - PrettyPrompt handles input, Spectre.Console handles output
- Active maintenance and modern .NET support
- No conflicts when used together

### Implementation Recommendations

**Installation**:

```bash
dotnet add package PrettyPrompt --version 4.0.0
dotnet add package Spectre.Console --version 0.49.1
```

**REPL Engine Pattern**:

```csharp
public class ReplEngine
{
    private readonly Prompt _prompt;

    public ReplEngine()
    {
        _prompt = new Prompt();
    }

    public async Task RunAsync()
    {
        AnsiConsole.MarkupLine("[green]Yet Another Translator[/]");
        AnsiConsole.MarkupLine("Type [cyan]/help[/] for commands, [cyan]/quit[/] to exit\n");

        while (true)
        {
            var response = await _prompt.ReadLineAsync("> ");

            if (!response.IsSuccess)
                break;

            if (string.IsNullOrWhiteSpace(response.Text))
                continue;

            try
            {
                await ProcessCommandAsync(response.Text);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }
}
```

**Table Formatting for Word Translations**:

```csharp
public static void DisplayWordTranslations(TranslationResult result)
{
    var table = new Table();
    table.Border(TableBorder.Rounded);
    table.AddColumn("#");
    table.AddColumn("Translation");
    table.AddColumn("Part of Speech");
    table.AddColumn("Countability");
    table.AddColumn("Example");

    for (int i = 0; i < result.Translations.Count; i++)
    {
        var translation = result.Translations[i];
        table.AddRow(
            (i + 1).ToString(),
            translation.Word,
            translation.PartOfSpeech,
            translation.Countability ?? "N/A",
            translation.Example
        );
    }

    AnsiConsole.Write(table);
}
```

**Command Parsing Pattern**:

```csharp
public class CommandParser
{
    public Command Parse(string input)
    {
        if (!input.StartsWith("/"))
            return new Command { Type = CommandType.Invalid };

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var commandName = parts[0].ToLower();
        var argument = parts.Length > 1 ? parts[1] : string.Empty;

        return commandName switch
        {
            "/t" or "/translate" => new Command
            {
                Type = CommandType.TranslateWord,
                Argument = argument,
                AutoDetectLanguage = true
            },
            "/tp" or "/translate-polish" => new Command
            {
                Type = CommandType.TranslateWord,
                Argument = argument,
                SourceLanguage = "Polish"
            },
            "/te" or "/translate-english" => new Command
            {
                Type = CommandType.TranslateWord,
                Argument = argument,
                SourceLanguage = "English"
            },
            "/tt" or "/translate-text" => new Command
            {
                Type = CommandType.TranslateText,
                Argument = argument.Replace("\\n", "\n"),
                AutoDetectLanguage = true
            },
            "/q" or "/quit" => new Command { Type = CommandType.Quit },
            _ => new Command { Type = CommandType.Invalid }
        };
    }
}
```

**Best Practices**:

- Use PrettyPrompt's cancellation token support for long-running operations
- Leverage Spectre.Console's markup language for colored output
- Use `AnsiConsole.Status()` for showing progress during API calls
- Handle Ctrl-C gracefully through PrettyPrompt's response.IsSuccess check
- Escape user input when displaying in Spectre.Console to prevent markup injection

## 5. Azure Key Vault Integration

### Decision

Use **Azure.Security.KeyVault.Secrets v4.8.0+** with **Azure.Identity v1.17.0+** for `az login` authentication

### Rationale

- Official Microsoft SDK with excellent support
- `DefaultAzureCredential` automatically uses `az login` credentials
- Cross-platform authentication support
- Comprehensive error handling for all failure scenarios
- Actively maintained with .NET 10 compatibility

### Implementation Recommendations

**Installation**:

```bash
dotnet add package Azure.Security.KeyVault.Secrets --version 4.8.0
dotnet add package Azure.Identity --version 1.17.0
```

**Configuration Pattern**:

config.json:

```json
{
  "secretManager": {
    "type": "azure-keyvault",
    "keyVaultUrl": "https://yetanothertranslator.vault.azure.net"
  },
  "llmProvider": {
    "type": "anthropic",
    "secretReference": "anthropic-api-key"
  },
  "ttsProvider": {
    "type": "elevenlabs",
    "secretReference": "elevenlabs-api-key"
  },
  "database": {
    "secretReference": "postgres-connection-string"
  }
}
```

**Secrets Provider Implementation**:

```csharp
public class AzureKeyVaultProvider : ISecretsProvider
{
    private readonly SecretClient _client;
    private readonly Dictionary<string, string> _cache;

    public AzureKeyVaultProvider(string keyVaultUrl)
    {
        var credential = new DefaultAzureCredential();
        _client = new SecretClient(new Uri(keyVaultUrl), credential);
        _cache = new Dictionary<string, string>();
    }

    public async Task<string> GetSecretAsync(
        string secretName,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(secretName, out var cached))
            return cached;

        try
        {
            var secret = await _client.GetSecretAsync(
                secretName,
                cancellationToken: cancellationToken);

            _cache[secretName] = secret.Value;
            return secret.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException(
                $"Secret '{secretName}' not found in Key Vault", ex);
        }
        catch (Azure.Identity.AuthenticationFailedException ex)
        {
            throw new InvalidOperationException(
                "Failed to authenticate with Azure Key Vault. " +
                "Run 'az login' and ensure you have access to the Key Vault", ex);
        }
    }
}
```

**Startup Validation**:

```csharp
public static async Task<int> Main(string[] args)
{
    try
    {
        var config = LoadConfiguration();
        var secretsProvider = new AzureKeyVaultProvider(
            config.SecretManager.KeyVaultUrl);

        var requiredSecrets = new[]
        {
            config.LlmProvider.SecretReference,
            config.TtsProvider.SecretReference,
            config.Database.SecretReference
        };

        foreach (var secretRef in requiredSecrets)
        {
            await secretsProvider.GetSecretAsync(secretRef);
        }

        await RunReplAsync(config, secretsProvider);
        return 0;
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"Configuration Error: {ex.Message}");
        return 1;
    }
}
```

**Best Practices**:

- In-memory cache to reduce Key Vault API calls
- Use `DefaultAzureCredential` authentication chain
- Implement retry logic with exponential backoff
- Set reasonable timeout (10-15 seconds) for secret retrieval
- Fail fast at startup if secrets cannot be retrieved
- Provide clear error messages mentioning `az login`
- Use Azure RBAC (Key Vault Secrets User role) for access control

## 6. PostgreSQL with EF Core

### Decision

Use **EF Core 10.0** (latest for .NET 10) with **Npgsql.EntityFrameworkCore.PostgreSQL**

### Implementation Recommendations

**Installation**:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.0
```

**DbContext Design**:

```csharp
public class TranslatorDbContext : DbContext
{
    public DbSet<HistoryEntry> HistoryEntries { get; set; }
    public DbSet<PronunciationCache> PronunciationCache { get; set; }
    public DbSet<TranslationCache> TranslationCache { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistoryEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.CommandType).IsRequired();
            entity.Property(e => e.InputText).IsRequired();
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<TranslationCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CacheKey).IsRequired();
            entity.HasIndex(e => e.CacheKey).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
```

**Caching Strategy**:

- Create cache key from hash of input parameters (text, source language, target language)
- Store serialized JSON response in cache
- Implement `--no-cache` flag to bypass cache lookup
- Set retention policy (keep last 100 history entries, 30-day cache expiration)

**Testing**:

- Integration tests only with Testcontainers for real PostgreSQL

## 7. Handler Pattern Implementation

### Decision

Direct handler invocation without MediatR - handlers are simple classes with business logic methods

### Rationale

- Simpler than MediatR - no extra abstraction layer
- Direct method calls are easier to understand and debug
- Handlers still provide clear separation of concerns
- Dependency injection still ensures testability
- No need for command/query objects and IRequest/IRequestHandler ceremony

### Implementation Recommendations

**Handler Pattern**:

```csharp
public class TranslateWordHandler
{
    private readonly ILlmProvider _llmProvider;
    private readonly IHistoryRepository _historyRepository;
    private readonly IValidator<TranslateWordRequest> _validator;

    public TranslateWordHandler(
        ILlmProvider llmProvider,
        IHistoryRepository historyRepository,
        IValidator<TranslateWordRequest> validator)
    {
        _llmProvider = llmProvider;
        _historyRepository = historyRepository;
        _validator = validator;
    }

    public async Task<TranslationResult> HandleAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        bool useCache = true,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var request = new TranslateWordRequest(word, sourceLanguage, targetLanguage);
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // Check cache
        if (useCache)
        {
            var cached = await _historyRepository
                .GetCachedTranslationAsync(word, sourceLanguage);

            if (cached != null)
                return cached;
        }

        // Call LLM provider
        var result = await _llmProvider.TranslateWordAsync(
            word,
            sourceLanguage,
            targetLanguage);

        // Save to history
        await _historyRepository.SaveHistoryAsync(
            new HistoryEntry
            {
                CommandType = "TranslateWord",
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
var handler = serviceProvider.GetRequiredService<TranslateWordHandler>();
var result = await handler.HandleAsync(word, "Polish", "English");
```

**Best Practices**:

- Keep handlers focused on single responsibility
- Use FluentValidation for input validation
- Use cancellation tokens throughout
- Inject all dependencies via constructor
- Return domain models, not DTOs

## 8. FluentValidation Integration

### Decision

Use **FluentValidation v11.10+** for input validation in handlers

### Implementation Recommendations

**Installation**:

```bash
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0
```

**Validator Pattern**:

```csharp
public record TranslateWordRequest(
    string Word,
    string SourceLanguage,
    string TargetLanguage);

public class TranslateWordRequestValidator
    : AbstractValidator<TranslateWordRequest>
{
    public TranslateWordRequestValidator()
    {
        RuleFor(x => x.Word)
            .NotEmpty()
            .WithMessage("Word cannot be empty")
            .MaximumLength(100)
            .WithMessage("Word must be less than 100 characters");

        RuleFor(x => x.SourceLanguage)
            .Must(lang => lang is "Polish" or "English" or "Auto")
            .WithMessage("Source language must be Polish, English, or Auto");

        RuleFor(x => x.TargetLanguage)
            .Must(lang => lang is "Polish" or "English")
            .WithMessage("Target language must be Polish or English");
    }
}
```

**Usage in Handlers**:

```csharp
public class TranslateWordHandler
{
    private readonly IValidator<TranslateWordRequest> _validator;
    private readonly ILlmProvider _llmProvider;

    public async Task<TranslationResult> HandleAsync(
        string word,
        string sourceLanguage,
        string targetLanguage)
    {
        // Validate input
        var request = new TranslateWordRequest(word, sourceLanguage, targetLanguage);
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Continue with business logic
        return await _llmProvider.TranslateWordAsync(word, sourceLanguage, targetLanguage);
    }
}
```

Or use `ValidateAndThrowAsync` for shorter syntax:

```csharp
var request = new TranslateWordRequest(word, sourceLanguage, targetLanguage);
await _validator.ValidateAndThrowAsync(request, cancellationToken);
```

## Summary

All NEEDS CLARIFICATION items have been resolved with specific technology choices and implementation patterns. Key decisions:

1. **Testing**: xUnit with NSubstitute (mocking), Testcontainers (PostgreSQL containers), WireMock.Net (external API mocking), Verify (snapshot testing) - integration tests only
2. **LLM**: Official Anthropic SDK (currently in beta) for Claude Sonnet 4.5
3. **TTS**: ElevenLabs-DotNet v3.6.0 with PortAudioSharp for playback
4. **REPL**: PrettyPrompt v4.x for input, Spectre.Console v0.49+ for output
5. **Secrets**: Azure.Security.KeyVault.Secrets v4.8.0 with DefaultAzureCredential
6. **Database**: EF Core 10.0 with Npgsql for PostgreSQL
7. **Handlers**: Direct handler invocation (no MediatR) for simplicity
8. **Validation**: FluentValidation v11.10 called directly in handlers

Testing strategy: Integration tests only using real PostgreSQL (via Testcontainers), mocked external APIs (via WireMock.Net), and snapshot assertions (via Verify). This approach tests actual database behavior and API integration patterns while avoiding false confidence from in-memory databases or excessive mocking. Verify snapshots make complex object assertions maintainable.

Architecture: Simple handler pattern with direct invocation - no MediatR, no command/query objects, no pipeline behaviors. Handlers are plain classes with business logic methods that use dependency injection for testability.

All choices align with constitution principles: testability through real integration tests, simplicity through direct handler invocation and standard libraries, and .NET 10 compatibility throughout.

