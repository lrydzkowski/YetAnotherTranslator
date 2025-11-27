using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YetAnotherTranslator.Infrastructure.Persistence.Hosting;

internal class DatabaseWarmupService : BackgroundService
{
    private readonly ILogger<DatabaseWarmupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseWarmupService(
        ILogger<DatabaseWarmupService> logger,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up database connection...");

            using IServiceScope scope = _serviceProvider.CreateScope();
            TranslatorDbContext dbContext = scope.ServiceProvider.GetRequiredService<TranslatorDbContext>();

            await dbContext.CacheEntries.AnyAsync(cancellationToken);
            await dbContext.HistoryEntries.AnyAsync(cancellationToken);

            _logger.LogInformation("Database connection warmed up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to warm up database connection, but application will continue normally");
        }
    }
}
