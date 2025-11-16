using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Cli.Repl.Commands;

namespace YetAnotherTranslator.Cli;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal void AddAppServices()
        {
            services.AddServices();
        }

        private void AddServices()
        {
            services.AddScoped<CommandParser>();
            services.AddScoped<ReplEngine>();
            services.AddScoped<TranslateWordCommand>();
            services.AddScoped<TranslateTextCommand>();
            services.AddScoped<ReviewGrammarCommand>();
            services.AddScoped<GetHistoryCommand>();
            services.AddHostedService<ReplHostedService>();
        }
    }
}
