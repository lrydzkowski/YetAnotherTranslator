using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Core.Configuration;
using YetAnotherTranslator.Core.Handlers;
using YetAnotherTranslator.Core.Interfaces;
using YetAnotherTranslator.Infrastructure.Llm;
using YetAnotherTranslator.Infrastructure.Persistence;
using YetAnotherTranslator.Infrastructure.Persistence.Repositories;
using YetAnotherTranslator.Infrastructure.Secrets;
using YetAnotherTranslator.Infrastructure.Tts;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddEnvironmentVariables();

    var config = new TranslatorConfiguration();
    builder.Configuration.Bind(config);

    var validator = new TranslatorConfigurationValidator();
    var validationResult = validator.Validate(config);

    if (!validationResult.IsValid)
    {
        AnsiConsole.MarkupLine("[red]Configuration validation failed:[/]");
        foreach (var error in validationResult.Errors)
        {
            AnsiConsole.MarkupLine($"[red]- {error.ErrorMessage}[/]");
        }
        return 1;
    }

    builder.Services.AddSingleton(config);
    builder.Services.AddSingleton<ISecretsProvider>(sp => new AzureKeyVaultProvider(config.Azure.KeyVaultUrl));

    builder.Services.AddSingleton<ILlmProvider>(sp =>
    {
        var secretsProvider = sp.GetRequiredService<ISecretsProvider>();
        var apiKey = secretsProvider.GetSecretAsync("anthropic-api-key").GetAwaiter().GetResult();
        return new AnthropicProvider(apiKey);
    });

    builder.Services.AddSingleton<ITtsProvider>(sp =>
    {
        var secretsProvider = sp.GetRequiredService<ISecretsProvider>();
        var apiKey = secretsProvider.GetSecretAsync("elevenlabs-api-key").GetAwaiter().GetResult();
        var voiceId = config.ElevenLabs.DefaultVoiceId;
        return new ElevenLabsProvider(apiKey, voiceId);
    });

    builder.Services.AddDbContext<TranslatorDbContext>(options =>
        options.UseNpgsql(config.Database.ConnectionString));

    builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
    builder.Services.AddScoped<TranslateWordHandler>();

    var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TranslatorDbContext>();
        if (!await dbContext.Database.CanConnectAsync())
        {
            AnsiConsole.MarkupLine("[red]Failed to connect to database. Please check your connection string and ensure PostgreSQL is running.[/]");
            return 1;
        }

        var translateWordHandler = scope.ServiceProvider.GetRequiredService<TranslateWordHandler>();
        var replEngine = new ReplEngine(translateWordHandler);
        await replEngine.RunAsync();
    }

    return 0;
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Fatal error during startup: {ex.Message}[/]");
    return 1;
}
