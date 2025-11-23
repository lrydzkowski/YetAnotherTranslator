using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Core.Common.Exceptions;
using YetAnotherTranslator.Infrastructure;
using YetAnotherTranslator.Infrastructure.Serilog;

namespace YetAnotherTranslator.Cli;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Log.Logger = LoggerBuilder.BuildLogger();

        try
        {
            IHost host = CreateHostBuilder(args).Build();
            await host.RunAsync();

            return 0;
        }
        catch (ConfigurationException ex)
        {
            Log.Fatal(ex, "Configuration error");
            await Console.Error.WriteLineAsync($"Configuration error: {ex.Message}");

            return 1;
        }
        catch (ExternalServiceException ex)
        {
            Log.Fatal(ex, "Service connection error: {ServiceName}", ex.ServiceName);
            await Console.Error.WriteLineAsync($"Service connection error ({ex.ServiceName}): {ex.Message}");

            return 1;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error");
            await Console.Error.WriteLineAsync($"Fatal error: {ex.Message}");

            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((_, config) => config.AddConfiguration(args))
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddAppServices();
                    services.AddCoreServices(context.Configuration);
                    services.AddInfrastructureServices(context.Configuration);
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
