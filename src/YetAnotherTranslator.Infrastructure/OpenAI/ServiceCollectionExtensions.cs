using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core;
using ReviewGrammarLargeLanguageProvider =
    YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces.ILargeLanguageModelProvider;
using TranslateTextLargeLanguageProvider =
    YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.ILargeLanguageModelProvider;
using TranslateWordLargeLanguageProvider =
    YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.ILargeLanguageModelProvider;

namespace YetAnotherTranslator.Infrastructure.OpenAI;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddOpenAiServices(IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddServices();
        }

        private void AddOptions(IConfiguration configuration)
        {
            services.AddOptionsTypeWithValidation<OpenAiOptions, OpenAiOptionsValidator>(
                configuration,
                OpenAiOptions.Position
            );
        }

        private void AddServices()
        {
            services.AddScopedWithPerformanceLogging<ReviewGrammarLargeLanguageProvider, OpenAiProvider>();
            services.AddScopedWithPerformanceLogging<TranslateTextLargeLanguageProvider, OpenAiProvider>();
            services.AddScopedWithPerformanceLogging<TranslateWordLargeLanguageProvider, OpenAiProvider>();
        }
    }
}
