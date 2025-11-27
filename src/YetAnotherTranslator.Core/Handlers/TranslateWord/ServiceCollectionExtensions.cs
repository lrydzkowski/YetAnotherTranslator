using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddTranslateWordServices()
        {
            services.AddScopedWithPerformanceLogging<ITranslateWordHandler, TranslateWordHandler>();
            services.AddScoped<IValidator<TranslateWordRequest>, TranslateWordValidator>();
        }
    }
}
