using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YetAnotherTranslator.Infrastructure.Persistence;

namespace YetAnotherTranslator.Infrastructure.Hosting;

internal class DatabaseWarmupService : IHostedService
{
    private readonly ILogger<DatabaseWarmupService> _logger;
    private readonly TranslatorDbContext _dbContext;

    public DatabaseWarmupService(
        ILogger<DatabaseWarmupService> logger,
        TranslatorDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Warming up database connection...");

            await _dbContext.CacheEntries.AnyAsync(cancellationToken);
            await _dbContext.HistoryEntries.AnyAsync(cancellationToken);

            _logger.LogInformation("Database connection warmed up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to warm up database connection, but application will continue normally");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
