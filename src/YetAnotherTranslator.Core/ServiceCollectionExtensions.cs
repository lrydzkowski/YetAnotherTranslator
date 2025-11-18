using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Common.Validation;
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

        public void AddOptionsType<TOptions>(
            IConfiguration configuration,
            string configurationPosition
        ) where TOptions : class
        {
            services.AddOptions<TOptions>()
                .Bind(configuration.GetSection(configurationPosition))
                .ValidateFluently()
                .ValidateOnStart();
        }

        public void AddOptionsTypeWithValidation<TOptions, TValidator>(
            IConfiguration configuration,
            string configurationPosition
        ) where TOptions : class where TValidator : class, IValidator<TOptions>, new()
        {
            services.AddSingleton<IValidator<TOptions>, TValidator>();
            services.AddOptionsType<TOptions>(configuration, configurationPosition);
        }

        private void AddServices()
        {
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<ISerializer, Serializer>();
            services.AddScoped<IEmbeddedFile, EmbeddedFile>();
        }
    }
}
