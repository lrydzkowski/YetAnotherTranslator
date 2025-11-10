using System.Security.Cryptography;
using System.Text;

namespace YetAnotherTranslator.Infrastructure.Persistence;

public static class CacheKeyGenerator
{
    public static string GenerateTranslationKey(string sourceLanguage, string targetLanguage, string inputText)
    {
        string input = $"{sourceLanguage}:{targetLanguage}:{inputText}";
        return ComputeHash(input);
    }

    public static string GenerateTextTranslationKey(string sourceLanguage, string targetLanguage, string inputText)
    {
        string input = $"text:{sourceLanguage}:{targetLanguage}:{inputText}";
        return ComputeHash(input);
    }

    public static string GeneratePronunciationKey(string text, string? partOfSpeech = null)
    {
        string input = $"{text}:{partOfSpeech ?? string.Empty}";
        return ComputeHash(input);
    }

    private static string ComputeHash(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
