using Microsoft.EntityFrameworkCore;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.GetHistory.Models;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;
using GetHistoryHistoryRepository = YetAnotherTranslator.Core.Handlers.GetHistory.Interfaces.IHistoryRepository;
using ReviewGrammarHistoryRepository = YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces.IHistoryRepository;
using TranslateTextHistoryRepository = YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.IHistoryRepository;
using TranslateWordHistoryRepository = YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.IHistoryRepository;

namespace YetAnotherTranslator.Infrastructure.Persistence.Repositories;

internal class HistoryRepository
    : GetHistoryHistoryRepository,
        ReviewGrammarHistoryRepository,
        TranslateTextHistoryRepository,
        TranslateWordHistoryRepository
{
    private readonly TranslatorDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public HistoryRepository(TranslatorDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<HistoryEntry>> GetHistoryAsync(
        int limit = 50,
        CancellationToken cancellationToken = default
    )
    {
        List<HistoryEntryEntity> entries = await _context.HistoryEntries
            .AsNoTracking()
            .OrderByDescending(entity => entity.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return entries.Select(
                entity => new HistoryEntry
                {
                    CommandType = Enum.Parse<CommandType>(entity.CommandType),
                    InputText = entity.InputText,
                    OutputText = entity.OutputText,
                    Timestamp = entity.CreatedAt
                }
            )
            .ToList();
    }

    public async Task SaveHistoryAsync(
        CommandType commandType,
        string inputText,
        string? outputText,
        CancellationToken cancellationToken = default
    )
    {
        HistoryEntryEntity historyEntry = new()
        {
            Id = Guid.CreateVersion7(),
            CommandType = commandType.ToString(),
            InputText = inputText,
            OutputText = outputText,
            CreatedAt = _dateTimeProvider.UtcNowOffset
        };

        _context.HistoryEntries.Add(historyEntry);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
