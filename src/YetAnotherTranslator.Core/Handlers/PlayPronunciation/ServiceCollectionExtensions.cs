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
            services.AddScoped<PlayPronunciationHandler>();
            services.AddScoped<IValidator<PlayPronunciationRequest>, PlayPronunciationValidator>();
        }
    }
}
