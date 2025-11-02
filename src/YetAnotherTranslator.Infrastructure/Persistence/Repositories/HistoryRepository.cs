using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using YetAnotherTranslator.Core.Interfaces;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence.Repositories;

public class HistoryRepository : IHistoryRepository
{
    private readonly TranslatorDbContext _context;

    public HistoryRepository(TranslatorDbContext context)
    {
        _context = context;
    }

    public async Task SaveHistoryEntryAsync(
        CommandType commandType,
        string inputText,
        string outputJson,
        string? llmMetadataJson,
        CancellationToken cancellationToken = default)
    {
        var entity = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            CommandType = commandType,
            InputText = inputText,
            OutputText = outputJson,
            LlmMetadata = llmMetadataJson,
            CreatedAt = DateTime.UtcNow
        };

        _context.HistoryEntries.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<HistoryEntry>> GetHistoryAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.HistoryEntries
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new HistoryEntry(
            e.Id,
            e.Timestamp,
            e.CommandType,
            e.InputText,
            e.OutputText,
            e.LlmMetadata,
            e.CreatedAt
        )).ToList();
    }

    public async Task<TranslationResult?> GetTranslationCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.TranslationCache
            .FirstOrDefaultAsync(e => e.CacheKey == cacheKey, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        entity.AccessedAt = DateTime.UtcNow;
        entity.AccessCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return JsonSerializer.Deserialize<TranslationResult>(entity.ResultJson);
    }

    public async Task SaveTranslationCacheAsync(
        string cacheKey,
        string sourceWord,
        string sourceLanguage,
        string targetLanguage,
        TranslationResult result,
        CancellationToken cancellationToken = default)
    {
        var entity = new TranslationCacheEntity
        {
            Id = Guid.NewGuid(),
            CacheKey = cacheKey,
            SourceWord = sourceWord,
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage,
            ResultJson = JsonSerializer.Serialize(result),
            CreatedAt = DateTime.UtcNow,
            AccessedAt = DateTime.UtcNow,
            AccessCount = 1
        };

        _context.TranslationCache.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TextTranslationResult?> GetTextTranslationCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.TextTranslationCache
            .FirstOrDefaultAsync(e => e.CacheKey == cacheKey, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        entity.AccessedAt = DateTime.UtcNow;
        entity.AccessCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return JsonSerializer.Deserialize<TextTranslationResult>(entity.ResultJson);
    }

    public async Task SaveTextTranslationCacheAsync(
        string cacheKey,
        string sourceTextHash,
        string sourceLanguage,
        string targetLanguage,
        TextTranslationResult result,
        CancellationToken cancellationToken = default)
    {
        var entity = new TextTranslationCacheEntity
        {
            Id = Guid.NewGuid(),
            CacheKey = cacheKey,
            SourceTextHash = sourceTextHash,
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage,
            ResultJson = JsonSerializer.Serialize(result),
            CreatedAt = DateTime.UtcNow,
            AccessedAt = DateTime.UtcNow,
            AccessCount = 1
        };

        _context.TextTranslationCache.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PronunciationResult?> GetPronunciationCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.PronunciationCache
            .FirstOrDefaultAsync(e => e.CacheKey == cacheKey, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        entity.AccessedAt = DateTime.UtcNow;
        entity.AccessCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return new PronunciationResult(
            entity.Text,
            entity.PartOfSpeech,
            entity.AudioData,
            entity.AudioFormat,
            entity.VoiceId,
            entity.AudioSizeBytes
        );
    }

    public async Task SavePronunciationCacheAsync(
        string cacheKey,
        string text,
        string? partOfSpeech,
        string voiceId,
        PronunciationResult result,
        CancellationToken cancellationToken = default)
    {
        var entity = new PronunciationCacheEntity
        {
            Id = Guid.NewGuid(),
            CacheKey = cacheKey,
            Text = text,
            PartOfSpeech = partOfSpeech,
            VoiceId = voiceId,
            AudioData = result.AudioData,
            AudioFormat = result.AudioFormat,
            AudioSizeBytes = result.AudioSizeBytes,
            CreatedAt = DateTime.UtcNow,
            AccessedAt = DateTime.UtcNow,
            AccessCount = 1
        };

        _context.PronunciationCache.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
