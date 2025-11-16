using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Infrastructure.Persistence.Repositories;
using PlayPronunciationCacheRepository =
    YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces.ICacheRepository;
using TranslateTextCacheRepository = YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.ICacheRepository;
using TranslateWordCacheRepository = YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.ICacheRepository;
using GetHistoryHistoryRepository = YetAnotherTranslator.Core.Handlers.GetHistory.Interfaces.IHistoryRepository;
using PlayPronunciationHistoryRepository =
    YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces.IHistoryRepository;
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
        }

        private void AddOptions(IConfiguration configuration)
        {
            services.AddOptionsType<DatabaseOptions>(configuration, DatabaseOptions.Position);
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
            services.AddScoped<PlayPronunciationCacheRepository, CacheRepository>();
            services.AddScoped<TranslateTextCacheRepository, CacheRepository>();
            services.AddScoped<TranslateWordCacheRepository, CacheRepository>();

            services.AddScoped<GetHistoryHistoryRepository, HistoryRepository>();
            services.AddScoped<PlayPronunciationHistoryRepository, HistoryRepository>();
            services.AddScoped<ReviewGrammarHistoryRepository, HistoryRepository>();
            services.AddScoped<TranslateTextHistoryRepository, HistoryRepository>();
            services.AddScoped<TranslateWordHistoryRepository, HistoryRepository>();
        }
    }
}
