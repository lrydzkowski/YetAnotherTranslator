using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Core.Common.Exceptions;
using YetAnotherTranslator.Infrastructure;

namespace YetAnotherTranslator.Cli;

// ReSharper disable once ClassNeverInstantiated.Global
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
            .ConfigureAppConfiguration((_, config) => config.AddConfiguration(args))
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddAppServices();
                    services.AddCoreServices();
                    services.AddInfrastructureServices(context.Configuration);
                }
            )
            .ConfigureLogging(
                (_, logging) =>
                {
                    logging.ClearProviders();
                }
            );
    }
}

public class ReplHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IServiceProvider _serviceProvider;

    public ReplHostedService(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime)
    {
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ReplEngine replEngine = scope.ServiceProvider.GetRequiredService<ReplEngine>();
        await replEngine.RunAsync(cancellationToken);
        _lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
