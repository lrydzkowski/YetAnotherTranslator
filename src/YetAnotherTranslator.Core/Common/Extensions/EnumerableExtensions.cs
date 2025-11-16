namespace YetAnotherTranslator.Core.Common.Extensions;

public static class EnumerableExtensions
{
    public static bool ContainsIgnoreCase(this IEnumerable<string> source, string value)
    {
        return source.Contains(value, StringComparer.InvariantCultureIgnoreCase);
    }
}
