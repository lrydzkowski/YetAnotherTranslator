using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace YetAnotherTranslator.Core.Common.Logging;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddLoggingServices(IConfiguration configuration)
        {
            services.AddOptions(configuration);
        }

        private void AddOptions(IConfiguration configuration)
        {
            services.AddOptionsType<PerformanceLoggingOptions>(
                configuration,
                PerformanceLoggingOptions.Position
            );
        }
    }
}
