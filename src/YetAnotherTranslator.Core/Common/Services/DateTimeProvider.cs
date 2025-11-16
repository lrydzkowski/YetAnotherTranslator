namespace YetAnotherTranslator.Core.Common.Services;

public interface IDateTimeProvider
{
    DateTimeOffset NowOffset { get; }
    DateTimeOffset UtcNowOffset { get; }
}

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset NowOffset => DateTimeOffset.Now;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
