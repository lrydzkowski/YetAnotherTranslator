using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YetAnotherTranslator.Core.Exceptions;

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
            .ConfigureServices((context, services) => { services.AddAppServices(context.Configuration); })
            .ConfigureLogging(
                (context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                }
            );
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
