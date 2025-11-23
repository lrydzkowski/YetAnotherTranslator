using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.GetHistory.Models;

namespace YetAnotherTranslator.Core.Handlers.GetHistory;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddGetHistoryServices()
        {
            services.AddScopedWithPerformanceLogging<IGetHistoryHandler, GetHistoryHandler>();
            services.AddScoped<IValidator<GetHistoryRequest>, GetHistoryValidator>();
        }
    }
}
