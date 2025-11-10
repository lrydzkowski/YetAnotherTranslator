using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Interfaces;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence;

public class HistoryRepository : IHistoryRepository
{
    private readonly TranslatorDbContext _context;

    public HistoryRepository(TranslatorDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TranslationResult?> GetCachedTranslationAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = CacheKeyGenerator.GenerateTranslationKey(sourceLanguage, targetLanguage, word);

        TranslationCacheEntity? cached = await _context.TranslationCache
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CacheKey == cacheKey, cancellationToken);

        if (cached == null)
        {
            return null;
        }

        if (DateTime.UtcNow - cached.CreatedAt > TimeSpan.FromDays(30))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TranslationResult>(cached.ResultJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveTranslationAsync(
        TranslationResult result,
        CancellationToken cancellationToken = default)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        string cacheKey = CacheKeyGenerator.GenerateTranslationKey(
            result.SourceLanguage,
            result.TargetLanguage,
            result.InputText
        );

        var existingCache = await _context.TranslationCache
            .FirstOrDefaultAsync(c => c.CacheKey == cacheKey, cancellationToken);

        if (existingCache != null)
        {
            return;
        }

        var cacheEntity = new TranslationCacheEntity
        {
            Id = Guid.NewGuid(),
            CacheKey = cacheKey,
            SourceLanguage = result.SourceLanguage,
            TargetLanguage = result.TargetLanguage,
            InputText = result.InputText,
            ResultJson = JsonSerializer.Serialize(result),
            CreatedAt = DateTime.UtcNow
        };

        _context.TranslationCache.Add(cacheEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveHistoryAsync(
        CommandType commandType,
        string inputText,
        string outputText,
        CancellationToken cancellationToken = default)
    {
        var historyEntry = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            CommandType = commandType.ToString(),
            InputText = inputText,
            OutputText = outputText,
            CreatedAt = DateTime.UtcNow
        };

        _context.HistoryEntries.Add(historyEntry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<HistoryEntry>> GetHistoryAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var entries = await _context.HistoryEntries
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return entries.Select(e => new HistoryEntry
        {
            CommandType = Enum.Parse<CommandType>(e.CommandType),
            InputText = e.InputText,
            OutputText = e.OutputText,
            Timestamp = e.CreatedAt
        }).ToList();
    }
}
