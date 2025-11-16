namespace YetAnotherTranslator.Core.Common.Extensions;

public static class StringExtensions
{
    public static bool EqualsIgnoreCase(this string? a, string? b)
    {
        return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
    }
}
