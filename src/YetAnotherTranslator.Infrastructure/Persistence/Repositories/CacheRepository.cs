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
        PronunciationCacheEntity? existingCache = await _context.PronunciationCache
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (existingCache is not null)
        {
            return;
        }

        PronunciationCacheEntity cacheEntity = new()
        {
            Id = Guid.CreateVersion7(),
            CacheKey = cacheKey,
            Text = text,
            PartOfSpeech = partOfSpeech,
            AudioData = audioData,
            VoiceId = voiceId,
            CreatedAt = _dateTimeProvider.UtcNowOffset
        };

        _context.PronunciationCache.Add(cacheEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<byte[]?> GetPronunciationAsync(
        string text,
        string? partOfSpeech,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = _cacheKeyGenerator.Generate(text, partOfSpeech);
        PronunciationCacheEntity? cached = await _context.PronunciationCache
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (cached is null)
        {
            return null;
        }

        if (_dateTimeProvider.UtcNowOffset - cached.CreatedAt > TimeSpan.FromDays(30))
        {
            return null;
        }

        return cached.AudioData;
    }

    public async Task<TextTranslationResult?> GetTextTranslationAsync(
        string text,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = _cacheKeyGenerator.Generate(text, sourceLanguage, targetLanguage);
        TextTranslationCacheEntity? cached = await _context.TextTranslationCache
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (cached is null)
        {
            return null;
        }

        if (_dateTimeProvider.UtcNowOffset - cached.CreatedAt > TimeSpan.FromDays(30))
        {
            return null;
        }

        return new TextTranslationResult
        {
            SourceLanguage = cached.SourceLanguage,
            TargetLanguage = cached.TargetLanguage,
            InputText = cached.InputText,
            TranslatedText = cached.TranslatedText
        };
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
        TextTranslationCacheEntity? existingCache = await _context.TextTranslationCache
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (existingCache is not null)
        {
            return;
        }

        TextTranslationCacheEntity cacheEntity = new()
        {
            Id = Guid.CreateVersion7(),
            CacheKey = cacheKey,
            SourceLanguage = result.SourceLanguage,
            TargetLanguage = result.TargetLanguage,
            InputText = result.InputText,
            TranslatedText = result.TranslatedText,
            CreatedAt = _dateTimeProvider.UtcNowOffset
        };

        _context.TextTranslationCache.Add(cacheEntity);
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

        TranslationCacheEntity? existingCache = await _context.TranslationCache
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (existingCache is not null)
        {
            return;
        }

        TranslationCacheEntity cacheEntity = new()
        {
            Id = Guid.CreateVersion7(),
            CacheKey = cacheKey,
            SourceLanguage = result.SourceLanguage,
            TargetLanguage = result.TargetLanguage,
            InputText = result.InputText,
            ResultJson = _serializer.Serialize(result),
            CreatedAt = _dateTimeProvider.UtcNowOffset
        };

        _context.TranslationCache.Add(cacheEntity);
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
        TranslationCacheEntity? cached = await _context.TranslationCache
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CacheKey == cacheKey, cancellationToken);
        if (cached is null)
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
