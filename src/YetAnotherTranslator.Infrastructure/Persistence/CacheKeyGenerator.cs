using System.Security.Cryptography;
using System.Text;

namespace YetAnotherTranslator.Infrastructure.Persistence;

internal class CacheKeyGenerator
{
    public string Generate(string text, string? sourceLanguage, string? targetLanguage)
    {
        string input = $"{text}:{sourceLanguage ?? "-"}:{targetLanguage ?? "-"}";

        return ComputeHash(input);
    }

    public string Generate(string text, string? partOfSpeech = null)
    {
        string input = $"{text}:{partOfSpeech ?? string.Empty}";

        return ComputeHash(input);
    }

    private string ComputeHash(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
