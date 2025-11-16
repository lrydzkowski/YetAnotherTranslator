using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddReviewGrammarServices()
        {
            services.AddScoped<ReviewGrammarHandler>();
            services.AddScoped<IValidator<ReviewGrammarRequest>, ReviewGrammarValidator>();
        }
    }
}
