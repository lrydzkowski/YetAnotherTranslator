using YetAnotherTranslator.Core.Handlers.GetHistory.Models;

namespace YetAnotherTranslator.Core.Handlers.GetHistory.Interfaces;

public interface IHistoryRepository
{
    Task<List<HistoryEntry>> GetHistoryAsync(
        int limit = 50,
        CancellationToken cancellationToken = default
    );
}
