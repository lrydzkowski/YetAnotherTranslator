using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            ApplicationConfiguration appConfig = await ValidateAndLoadConfigurationAsync();

            IHost host = CreateHostBuilder(args, appConfig).Build();
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

    private static async Task<ApplicationConfiguration> ValidateAndLoadConfigurationAsync()
    {
        var loader = new ConfigurationLoader();
        ApplicationConfiguration config = await loader.LoadConfigurationAsync();
        return config;
    }

    private static IHostBuilder CreateHostBuilder(string[] args, ApplicationConfiguration appConfig)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(
                (context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", true, true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                }
            )
            .ConfigureServices((context, services) => { ConfigureServices(services, appConfig); })
            .ConfigureLogging(
                (context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                }
            );
    }

    private static void ConfigureServices(IServiceCollection services, ApplicationConfiguration appConfig)
    {
        services.AddSingleton(appConfig);

        services.AddDbContext<TranslatorDbContext>(
            options => { options.UseNpgsql(appConfig.Database.ConnectionString); }
        );

        services.AddScoped<Core.Interfaces.IHistoryRepository, Infrastructure.Persistence.HistoryRepository>();
        services.AddScoped<Core.Interfaces.ISecretsProvider>(sp =>
            new Infrastructure.Secrets.AzureKeyVaultSecretsProvider(appConfig.SecretManager.KeyVaultUrl)
        );

        services.AddScoped<Core.Interfaces.ILlmProvider>(sp =>
        {
            var secretsProvider = sp.GetRequiredService<Core.Interfaces.ISecretsProvider>();
            string apiKey = secretsProvider.GetSecretAsync(appConfig.LlmProvider.ApiKeySecretName).Result;
            return new Infrastructure.Llm.AnthropicLlmProvider(apiKey, appConfig.LlmProvider.Model);
        });

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.TranslateWord.TranslateWordRequest>,
            Core.Handlers.TranslateWord.TranslateWordValidator>();

        services.AddScoped<Core.Handlers.TranslateWord.TranslateWordHandler>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.TranslateText.TranslateTextRequest>,
            Core.Handlers.TranslateText.TranslateTextValidator>();

        services.AddScoped<Core.Handlers.TranslateText.TranslateTextHandler>();

        services.AddSingleton<Repl.CommandParser>();
        services.AddScoped<Repl.ReplEngine>();

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
