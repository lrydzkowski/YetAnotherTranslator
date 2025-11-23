using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Core.Common.Logging;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Common.Validation;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Handlers.TranslateWord;

namespace YetAnotherTranslator.Core;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddCoreServices(IConfiguration configuration)
        {
            services.AddServices();
            services.AddGetHistoryServices();
            services.AddPlayPronunciationServices();
            services.AddReviewGrammarServices();
            services.AddTranslateTextServices();
            services.AddTranslateWordServices();
            services.AddLoggingServices(configuration);
        }

        public void AddOptionsType<TOptions>(
            IConfiguration configuration,
            string configurationPosition
        ) where TOptions : class
        {
            services.AddOptions<TOptions>().Bind(configuration.GetSection(configurationPosition));
        }

        public void AddOptionsTypeWithValidation<TOptions, TValidator>(
            IConfiguration configuration,
            string configurationPosition
        ) where TOptions : class where TValidator : class, IValidator<TOptions>, new()
        {
            services.AddSingleton<IValidator<TOptions>, TValidator>();
            services.AddOptions<TOptions>()
                .Bind(configuration.GetSection(configurationPosition))
                .ValidateFluently()
                .ValidateOnStart();
        }

        public void AddScopedWithPerformanceLogging<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddWithPerformanceLogging<TInterface, TImplementation>(ServiceLifetime.Scoped);
        }

        public void AddSingletonWithPerformanceLogging<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddWithPerformanceLogging<TInterface, TImplementation>(ServiceLifetime.Singleton);
        }

        public void AddTransientWithPerformanceLogging<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddWithPerformanceLogging<TInterface, TImplementation>(ServiceLifetime.Transient);
        }

        private void AddWithPerformanceLogging<TInterface, TImplementation>(ServiceLifetime lifetime)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));

            services.Add(
                new ServiceDescriptor(
                    typeof(TInterface),
                    sp =>
                    {
                        TImplementation implementation = sp.GetRequiredService<TImplementation>();
                        PerformanceLoggingOptions options =
                            sp.GetRequiredService<IOptions<PerformanceLoggingOptions>>().Value;
                        if (!options.IsEnabled)
                        {
                            return implementation;
                        }

                        ILogger<PerformanceLoggingCategory> logger =
                            sp.GetRequiredService<ILogger<PerformanceLoggingCategory>>();

                        return PerformanceLoggingProxy<TInterface>.Create(implementation, logger);
                    },
                    lifetime
                )
            );
        }

        private void AddServices()
        {
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<ISerializer, Serializer>();
            services.AddScoped<IEmbeddedFile, EmbeddedFile>();
        }
    }
}
