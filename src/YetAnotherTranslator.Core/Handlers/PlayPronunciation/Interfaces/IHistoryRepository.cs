using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

public interface IHistoryRepository
{
    Task SaveHistoryAsync(
        CommandType commandType,
        string inputText,
        string outputText,
        CancellationToken cancellationToken = default
    );
}
