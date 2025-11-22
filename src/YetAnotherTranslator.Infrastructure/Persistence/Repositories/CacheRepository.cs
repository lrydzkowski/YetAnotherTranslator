using Microsoft.EntityFrameworkCore;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.TranslateText.Models;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;
using PlayPronunciationCacheRepository =
    YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces.ICacheRepository;
using TranslateTextCacheRepository = YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces.ICacheRepository;
using TranslateWordCacheRepository = YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces.ICacheRepository;

namespace YetAnotherTranslator.Infrastructure.Persistence.Repositories;

internal class CacheRepository
    : PlayPronunciationCacheRepository, TranslateTextCacheRepository, TranslateWordCacheRepository
{
    private readonly CacheKeyGenerator _cacheKeyGenerator;
    private readonly TimeSpan _cacheLifespan = TimeSpan.FromDays(30);
    private readonly TranslatorDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISerializer _serializer;

    public CacheRepository(
        TranslatorDbContext context,
        CacheKeyGenerator cacheKeyGenerator,
        IDateTimeProvider dateTimeProvider,
        ISerializer serializer
    )
    {
        _context = context;
        _cacheKeyGenerator = cacheKeyGenerator;
        _dateTimeProvider = dateTimeProvider;
        _serializer = serializer;
    }

    public async Task SavePronunciationAsync(
        string text,
        string? partOfSpeech,
        byte[] audioData,
        string voiceId,
        CancellationToken cancellationToken = default
    )
    {
        if (audioData is null || audioData.Length == 0)
        {
            throw new ArgumentException("Audio data cannot be empty", nameof(audioData));
        }

        string cacheKey = _cacheKeyGenerator.Generate(text, partOfSpeech);
        CacheEntity? existingCache = await _context.CacheEntries
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (existingCache is not null)
        {
            existingCache.ResultByte = audioData;
            existingCache.CreatedAt = _dateTimeProvider.UtcNowOffset;
            await _context.SaveChangesAsync(cancellationToken);

            return;
        }

        CacheEntity cacheEntity = new()
        {
            Id = Guid.CreateVersion7(),
            CacheKey = cacheKey,
            ResultByte = audioData,
            CreatedAt = _dateTimeProvider.UtcNowOffset
        };
        _context.CacheEntries.Add(cacheEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<byte[]?> GetPronunciationAsync(
        string text,
        string? partOfSpeech,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = _cacheKeyGenerator.Generate(text, partOfSpeech);
        CacheEntity? cached = await _context.CacheEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (cached?.ResultByte is null)
        {
            return null;
        }

        if (_dateTimeProvider.UtcNowOffset - cached.CreatedAt > _cacheLifespan)
        {
            return null;
        }

        return cached.ResultByte;
    }

    public async Task<TextTranslationResult?> GetTextTranslationAsync(
        string text,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = _cacheKeyGenerator.Generate(text, sourceLanguage, targetLanguage);
        CacheEntity? cached = await _context.CacheEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (cached?.ResultJson is null)
        {
            return null;
        }

        if (_dateTimeProvider.UtcNowOffset - cached.CreatedAt > _cacheLifespan)
        {
            return null;
        }

        return _serializer.Deserialize<TextTranslationResult>(cached.ResultJson);
    }

    public async Task SaveTextTranslationAsync(
        TextTranslationResult result,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = _cacheKeyGenerator.Generate(
            result.InputText,
            result.SourceLanguage,
            result.TargetLanguage
        );
        CacheEntity? existingCache = await _context.CacheEntries
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (existingCache is not null)
        {
            existingCache.ResultJson = _serializer.Serialize(result);
            existingCache.CreatedAt = _dateTimeProvider.UtcNowOffset;
            await _context.SaveChangesAsync(cancellationToken);

            return;
        }

        CacheEntity cacheEntity = new()
        {
            Id = Guid.CreateVersion7(),
            CacheKey = cacheKey,
            ResultJson = _serializer.Serialize(result),
            CreatedAt = _dateTimeProvider.UtcNowOffset
        };
        _context.CacheEntries.Add(cacheEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveTranslationAsync(
        TranslationResult result,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = _cacheKeyGenerator.Generate(
            result.InputText,
            result.SourceLanguage,
            result.TargetLanguage
        );

        CacheEntity? existingCache = await _context.CacheEntries
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (existingCache is not null)
        {
            existingCache.ResultJson = _serializer.Serialize(result);
            existingCache.CreatedAt = _dateTimeProvider.UtcNowOffset;
            await _context.SaveChangesAsync(cancellationToken);

            return;
        }

        CacheEntity cacheEntity = new()
        {
            Id = Guid.CreateVersion7(),
            CacheKey = cacheKey,
            ResultJson = _serializer.Serialize(result),
            CreatedAt = _dateTimeProvider.UtcNowOffset
        };
        _context.CacheEntries.Add(cacheEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TranslationResult?> GetTranslationAsync(
        string word,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = _cacheKeyGenerator.Generate(word, sourceLanguage, targetLanguage);
        CacheEntity? cached = await _context.CacheEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (cached?.ResultJson is null)
        {
            return null;
        }

        if (_dateTimeProvider.UtcNowOffset - cached.CreatedAt > TimeSpan.FromDays(30))
        {
            return null;
        }

        return _serializer.Deserialize<TranslationResult>(cached.ResultJson);
    }
}
