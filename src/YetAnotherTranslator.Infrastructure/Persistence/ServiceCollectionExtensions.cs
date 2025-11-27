using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Infrastructure.Persistence.Hosting;
using YetAnotherTranslator.Infrastructure.Persistence.Repositories;
using TranslateTextCacheRepository = YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.ICacheRepository;
using TranslateWordCacheRepository = YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.ICacheRepository;
using GetHistoryHistoryRepository = YetAnotherTranslator.Core.Handlers.GetHistory.Interfaces.IHistoryRepository;
using ReviewGrammarHistoryRepository = YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces.IHistoryRepository;
using TranslateTextHistoryRepository = YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.IHistoryRepository;
using TranslateWordHistoryRepository = YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.IHistoryRepository;

namespace YetAnotherTranslator.Infrastructure.Persistence;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPersistenceServices(IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddDbContext();
            services.AddRepositories();
            services.AddServices();
            services.AddHostedServices();
        }

        private void AddOptions(IConfiguration configuration)
        {
            services.AddOptionsTypeWithValidation<DatabaseOptions, DatabaseOptionsValidator>(
                configuration,
                DatabaseOptions.Position
            );
        }

        private void AddDbContext()
        {
            services.AddDbContext<TranslatorDbContext>(
                (sp, options) =>
                {
                    DatabaseOptions dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                    options.UseNpgsql(dbOptions.ConnectionString);
                }
            );
        }

        private void AddRepositories()
        {
            services.AddScopedWithPerformanceLogging<TranslateTextCacheRepository, CacheRepository>();
            services.AddScopedWithPerformanceLogging<TranslateWordCacheRepository, CacheRepository>();

            services.AddScopedWithPerformanceLogging<GetHistoryHistoryRepository, HistoryRepository>();
            services.AddScopedWithPerformanceLogging<ReviewGrammarHistoryRepository, HistoryRepository>();
            services.AddScopedWithPerformanceLogging<TranslateTextHistoryRepository, HistoryRepository>();
            services.AddScopedWithPerformanceLogging<TranslateWordHistoryRepository, HistoryRepository>();
        }

        private void AddServices()
        {
            services.AddScoped<CacheKeyGenerator>();
        }

        private void AddHostedServices()
        {
            services.AddHostedService<DatabaseWarmupService>();
        }
    }
}
