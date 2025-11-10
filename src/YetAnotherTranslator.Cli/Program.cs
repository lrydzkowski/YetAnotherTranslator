using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Infrastructure.Persistence;

namespace YetAnotherTranslator.Cli;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            IHost host = CreateHostBuilder(args).Build();
            await host.RunAsync();

            return 0;
        }
        catch (ConfigurationException ex)
        {
            await Console.Error.WriteLineAsync($"Configuration error: {ex.Message}");
            return 1;
        }
        catch (ExternalServiceException ex)
        {
            await Console.Error.WriteLineAsync($"Service connection error ({ex.ServiceName}): {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(
                (context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile(
                        $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                        optional: true,
                        reloadOnChange: true
                    );
                    config.AddEnvironmentVariables();

                    // Build intermediate configuration to get Key Vault name
                    IConfigurationRoot intermediateConfig = config.Build();
                    string? vaultName = intermediateConfig["KeyVault:VaultName"];

                    if (!string.IsNullOrWhiteSpace(vaultName))
                    {
                        string keyVaultUri = $"https://{vaultName}.vault.azure.net";
                        var secretClient = new SecretClient(
                            new Uri(keyVaultUri),
                            new DefaultAzureCredential()
                        );

                        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                    }

                    config.AddCommandLine(args);
                }
            )
            .ConfigureServices((context, services) => { ConfigureServices(services, context.Configuration); })
            .ConfigureLogging(
                (context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                }
            );
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure Options
        services.Configure<KeyVaultOptions>(configuration.GetSection(KeyVaultOptions.SectionName));
        services.Configure<LlmProviderOptions>(configuration.GetSection(LlmProviderOptions.SectionName));
        services.Configure<TtsProviderOptions>(configuration.GetSection(TtsProviderOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        // Configure DbContext
        services.AddDbContext<TranslatorDbContext>(
            (sp, options) =>
            {
                var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                string connectionString = configuration[dbOptions.ConnectionStringSecretName]
                    ?? throw new ConfigurationException("Database connection string not found in configuration");
                options.UseNpgsql(connectionString);
            }
        );

        // Register repositories
        services.AddScoped<Core.Interfaces.IHistoryRepository, Infrastructure.Persistence.HistoryRepository>();

        // Register LLM provider
        services.AddScoped<Core.Interfaces.ILlmProvider>(sp =>
        {
            var llmOptions = sp.GetRequiredService<IOptions<LlmProviderOptions>>().Value;
            string apiKey = configuration[llmOptions.ApiKeySecretName]
                ?? throw new ConfigurationException("LLM API key not found in configuration");
            return new Infrastructure.Llm.AnthropicLlmProvider(apiKey, llmOptions.Model);
        });

        // Register TTS provider
        services.AddScoped<Core.Interfaces.ITtsProvider>(sp =>
        {
            var ttsOptions = sp.GetRequiredService<IOptions<TtsProviderOptions>>().Value;
            string apiKey = configuration[ttsOptions.ApiKeySecretName]
                ?? throw new ConfigurationException("TTS API key not found in configuration");
            return new Infrastructure.Tts.ElevenLabsTtsProvider(apiKey, ttsOptions.VoiceId);
        });

        // Register audio player
        services.AddScoped<Core.Interfaces.IAudioPlayer, Infrastructure.Tts.PortAudioPlayer>();

        // Register validators
        services.AddScoped<FluentValidation.IValidator<Core.Handlers.TranslateWord.TranslateWordRequest>,
            Core.Handlers.TranslateWord.TranslateWordValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.TranslateText.TranslateTextRequest>,
            Core.Handlers.TranslateText.TranslateTextValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.ReviewGrammar.ReviewGrammarRequest>,
            Core.Handlers.ReviewGrammar.ReviewGrammarValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.PlayPronunciation.PlayPronunciationRequest>,
            Core.Handlers.PlayPronunciation.PlayPronunciationValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.GetHistory.GetHistoryRequest>,
            Core.Handlers.GetHistory.GetHistoryValidator>();

        // Register handlers
        services.AddScoped<Core.Handlers.TranslateWord.TranslateWordHandler>();
        services.AddScoped<Core.Handlers.TranslateText.TranslateTextHandler>();
        services.AddScoped<Core.Handlers.ReviewGrammar.ReviewGrammarHandler>();
        services.AddScoped<Core.Handlers.PlayPronunciation.PlayPronunciationHandler>();
        services.AddScoped<Core.Handlers.GetHistory.GetHistoryHandler>();

        // Register REPL components
        services.AddSingleton<Repl.CommandParser>();
        services.AddScoped<Repl.ReplEngine>();

        // Register hosted service
        services.AddHostedService<ReplHostedService>();
    }
}

public class ReplHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ReplHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var replEngine = scope.ServiceProvider.GetRequiredService<Repl.ReplEngine>();
        await replEngine.RunAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
