using YetAnotherTranslator.Core.Common.Extensions;

namespace YetAnotherTranslator.Core;

public static class TranslatorConstants
{
    public static class EnvironmentVariables
    {
        public const string Environment = "ENVIRONMENT";
    }
    
    public static class Environments
    {
        public const string Development = "Development";
    }
    
    public static class Validation
    {
        public const int MaxWordLength = 100;
        public const int MaxTextLength = 5000;
        public const int MinHistoryLimit = 1;
        public const int MaxHistoryLimit = 1000;
    }

    public static class Languages
    {
        public const string Polish = "Polish";
        public const string English = "English";

        public static readonly List<string> All =
        [
            Polish,
            English
        ];

        public static bool IsSupported(string? language)
        {
            return language is not null && All.ContainsIgnoreCase(language);
        }

        public static string Serialize()
        {
            return $"'{string.Join("', '", All)}'";
        }
    }

    public static class PartsOfSpeech
    {
        public const string Noun = "noun";
        public const string Pronoun = "pronoun";
        public const string Verb = "verb";
        public const string Adjective = "adjective";
        public const string Adverb = "adverb";
        public const string Preposition = "preposition";
        public const string Conjunction = "conjunction";
        public const string Interjection = "interjection";

        public static readonly List<string> All =
        [
            Noun,
            Verb,
            Adjective,
            Adverb,
            Pronoun,
            Preposition,
            Conjunction,
            Interjection
        ];

        public static bool IsSupported(string? partOfSpeech)
        {
            return partOfSpeech is not null && All.ContainsIgnoreCase(partOfSpeech);
        }

        public static string Serialize()
        {
            return $"'{string.Join("', '", All)}'";
        }
    }
}
