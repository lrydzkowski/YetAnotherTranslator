using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Infrastructure.Azure.AiFoundry;
using YetAnotherTranslator.Infrastructure.Azure.KeyVault;
using ReviewGrammarLargeLanguageProvider =
    YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces.ILargeLanguageModelProvider;
using TranslateTextLargeLanguageProvider =
    YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.ILargeLanguageModelProvider;
using TranslateWordLargeLanguageProvider =
    YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.ILargeLanguageModelProvider;

namespace YetAnotherTranslator.Infrastructure.Azure;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddAzureServices(IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddServices();
        }

        private void AddOptions(IConfiguration configuration)
        {
            services.AddOptionsTypeWithValidation<KeyVaultOptions, KeyVaultOptionsValidator>(
                configuration,
                KeyVaultOptions.Position
            );
            services.AddOptionsTypeWithValidation<AzureAiFoundryOptions, AzureAiFoundryOptionsValidator>(
                configuration,
                AzureAiFoundryOptions.Position
            );
        }

        private void AddServices()
        {
            services.AddScoped<ReviewGrammarLargeLanguageProvider, AzureAiFoundryProvider>();
            services.AddScoped<TranslateTextLargeLanguageProvider, AzureAiFoundryProvider>();
            services.AddScoped<TranslateWordLargeLanguageProvider, AzureAiFoundryProvider>();
        }
    }
}
