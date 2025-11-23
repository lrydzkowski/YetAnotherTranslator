using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation.Models;

public record PlayPronunciationRequest(
    CommandType CommandType,
    string Text,
    string? PartOfSpeech = null,
    bool UseCache = true
);
