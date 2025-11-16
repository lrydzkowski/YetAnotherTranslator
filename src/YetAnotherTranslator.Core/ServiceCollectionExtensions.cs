using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Handlers.TranslateWord;

namespace YetAnotherTranslator.Core;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddCoreServices()
        {
            services.AddServices();
            services.AddGetHistoryServices();
            services.AddPlayPronunciationServices();
            services.AddReviewGrammarServices();
            services.AddTranslateTextServices();
            services.AddTranslateWordServices();
        }

        public IServiceCollection AddOptionsType<TOptions>(
            IConfiguration configuration,
            string configurationPosition
        ) where TOptions : class
        {
            services.AddOptions<TOptions>().Bind(configuration.GetSection(configurationPosition));

            return services;
        }

        private void AddServices()
        {
            services.AddScoped<IDateTimeProvider, IDateTimeProvider>();
            services.AddScoped<ISerializer, Serializer>();
            services.AddScoped<IEmbeddedFile, EmbeddedFile>();
        }
    }
}
