using Serilog;
using Serilog.Events;

namespace YetAnotherTranslator.Infrastructure.Serilog;

public static class LoggerBuilder
{
    public static ILogger BuildLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.File(
                "logs/logs-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}:{NewLine}{Message:lj}{NewLine}{Exception}"
            )
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
