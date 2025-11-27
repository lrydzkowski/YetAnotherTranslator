using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.TranslateText.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddTranslateTextServices()
        {
            services.AddScopedWithPerformanceLogging<ITranslateTextHandler, TranslateTextHandler>();
            services.AddScoped<IValidator<TranslateTextRequest>, TranslateTextValidator>();
        }
    }
}
