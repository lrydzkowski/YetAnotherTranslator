using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Infrastructure.Azure;
using YetAnotherTranslator.Infrastructure.Persistence;

namespace YetAnotherTranslator.Infrastructure;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddInfrastructureServices(IConfiguration configuration)
        {
            services.AddAzureServices(configuration);
            services.AddPersistenceServices(configuration);
        }
    }
}
