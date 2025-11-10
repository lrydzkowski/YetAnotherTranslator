using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

    public static string GenerateLlmResponseKey(string operationType, object parameters)
    {
        string parametersJson = JsonSerializer.Serialize(
            parameters,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            }
        );

        var sortedParams = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);
        var sortedJson = JsonSerializer.Serialize(
            sortedParams!.OrderBy(kvp => kvp.Key),
            new JsonSerializerOptions { WriteIndented = false }
        );

        string input = $"{operationType}:{sortedJson}";
        return ComputeHash(input);
    }

    private static string ComputeHash(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
