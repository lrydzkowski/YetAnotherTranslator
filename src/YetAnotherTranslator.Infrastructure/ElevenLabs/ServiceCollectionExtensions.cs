using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

namespace YetAnotherTranslator.Infrastructure.ElevenLabs;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddElevenLabsServices(IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddServices();
        }

        private void AddOptions(IConfiguration configuration)
        {
            services.AddOptionsType<ElevenLabsApiOptions>(configuration, ElevenLabsApiOptions.Position);
        }

        private void AddServices()
        {
            services.AddScoped<IAudioPlayer, PortAudioPlayer>();
            services.AddScoped<ITextToSpeechProvider, ElevenLabsApiClient>();
        }
    }
}
