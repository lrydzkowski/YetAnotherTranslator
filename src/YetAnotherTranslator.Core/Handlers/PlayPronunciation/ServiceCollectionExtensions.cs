using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Models;

namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPlayPronunciationServices()
        {
            services.AddScopedWithPerformanceLogging<IPlayPronunciationHandler, PlayPronunciationHandler>();
            services.AddScoped<IValidator<PlayPronunciationRequest>, PlayPronunciationValidator>();
        }
    }
}
