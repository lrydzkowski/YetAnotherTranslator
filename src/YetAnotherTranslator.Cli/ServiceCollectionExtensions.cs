using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Infrastructure.Persistence;

namespace YetAnotherTranslator.Cli;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddAppServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddConfiguration(configuration);
        services.AddDatabase(configuration);
        services.AddProviders(configuration);
        services.AddValidators();
        services.AddHandlers();
        services.AddReplComponents();
        services.AddHostedServices();

        return services;
    }

    private static IServiceCollection AddConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptionsType<KeyVaultOptions>(configuration, KeyVaultOptions.SectionName)
            .AddOptionsType<LlmProviderOptions>(configuration, LlmProviderOptions.SectionName)
            .AddOptionsType<TtsProviderOptions>(configuration, TtsProviderOptions.SectionName)
            .AddOptionsType<DatabaseOptions>(configuration, DatabaseOptions.SectionName);

        return services;
    }

    private static IServiceCollection AddOptionsType<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationPosition
    ) where TOptions : class
    {
        services.AddOptions<TOptions>().Bind(configuration.GetSection(configurationPosition));
        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<TranslatorDbContext>(
            (sp, options) =>
            {
                var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                string connectionString = configuration[dbOptions.ConnectionStringSecretName]
                    ?? throw new ConfigurationException("Database connection string not found in configuration");
                options.UseNpgsql(connectionString);
            }
        );

        services.AddScoped<Core.Interfaces.IHistoryRepository, Infrastructure.Persistence.HistoryRepository>();

        return services;
    }

    private static IServiceCollection AddProviders(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<Core.Interfaces.ILlmProvider>(sp =>
        {
            var llmOptions = sp.GetRequiredService<IOptions<LlmProviderOptions>>().Value;
            string apiKey = configuration[llmOptions.ApiKeySecretName]
                ?? throw new ConfigurationException("LLM API key not found in configuration");
            return new Infrastructure.Llm.AnthropicLlmProvider(apiKey, llmOptions.Model);
        });

        services.AddScoped<Core.Interfaces.ITtsProvider>(sp =>
        {
            var ttsOptions = sp.GetRequiredService<IOptions<TtsProviderOptions>>().Value;
            string apiKey = configuration[ttsOptions.ApiKeySecretName]
                ?? throw new ConfigurationException("TTS API key not found in configuration");
            return new Infrastructure.Tts.ElevenLabsTtsProvider(apiKey, ttsOptions.VoiceId);
        });

        services.AddScoped<Core.Interfaces.IAudioPlayer, Infrastructure.Tts.PortAudioPlayer>();

        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<FluentValidation.IValidator<Core.Handlers.TranslateWord.TranslateWordRequest>,
            Core.Handlers.TranslateWord.TranslateWordValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.TranslateText.TranslateTextRequest>,
            Core.Handlers.TranslateText.TranslateTextValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.ReviewGrammar.ReviewGrammarRequest>,
            Core.Handlers.ReviewGrammar.ReviewGrammarValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.PlayPronunciation.PlayPronunciationRequest>,
            Core.Handlers.PlayPronunciation.PlayPronunciationValidator>();

        services.AddScoped<FluentValidation.IValidator<Core.Handlers.GetHistory.GetHistoryRequest>,
            Core.Handlers.GetHistory.GetHistoryValidator>();

        return services;
    }

    private static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<Core.Handlers.TranslateWord.TranslateWordHandler>();
        services.AddScoped<Core.Handlers.TranslateText.TranslateTextHandler>();
        services.AddScoped<Core.Handlers.ReviewGrammar.ReviewGrammarHandler>();
        services.AddScoped<Core.Handlers.PlayPronunciation.PlayPronunciationHandler>();
        services.AddScoped<Core.Handlers.GetHistory.GetHistoryHandler>();

        return services;
    }

    private static IServiceCollection AddReplComponents(this IServiceCollection services)
    {
        services.AddSingleton<Repl.CommandParser>();
        services.AddScoped<Repl.ReplEngine>();

        return services;
    }

    private static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<ReplHostedService>();

        return services;
    }
}
