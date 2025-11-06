using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                    config.AddJsonFile("appsettings.json", true, true);
                    config.AddEnvironmentVariables();
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
        services.AddDbContext<TranslatorDbContext>(
            options =>
            {
                string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
                options.UseNpgsql(connectionString);
            }
        );
    }
}
