using FluentValidation;
using Microsoft.Extensions.Logging;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

public interface IReviewGrammarHandler
{
    Task<GrammarReviewResult?> HandleAsync(
        ReviewGrammarRequest request,
        CancellationToken cancellationToken = default
    );
}

internal class ReviewGrammarHandler : IReviewGrammarHandler
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ILargeLanguageModelProvider _largeLanguageModelProvider;
    private readonly ILogger<ReviewGrammarHandler> _logger;
    private readonly ISerializer _serializer;
    private readonly IValidator<ReviewGrammarRequest> _validator;

    public ReviewGrammarHandler(
        ILargeLanguageModelProvider largeLanguageModelProvider,
        IValidator<ReviewGrammarRequest> validator,
        IHistoryRepository historyRepository,
        ISerializer serializer,
        ILogger<ReviewGrammarHandler> logger
    )
    {
        _largeLanguageModelProvider = largeLanguageModelProvider;
        _validator = validator;
        _historyRepository = historyRepository;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task<GrammarReviewResult?> HandleAsync(
        ReviewGrammarRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);
            GrammarReviewResult? result = await ReviewGrammarAsync(request, cancellationToken);
            result?.InputText = request.Text;
            await SaveHistoryAsync(request, result, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error reviewing grammar. Text length: {TextLength}",
                request.Text.Length
            );

            throw;
        }
    }

    private async Task<GrammarReviewResult?> ReviewGrammarAsync(
        ReviewGrammarRequest request,
        CancellationToken cancellationToken
    )
    {
        GrammarReviewResult? result = await _largeLanguageModelProvider.ReviewGrammarAsync(
            request.Text,
            cancellationToken
        );

        return result;
    }

    private async Task SaveHistoryAsync(
        ReviewGrammarRequest request,
        GrammarReviewResult? result,
        CancellationToken cancellationToken
    )
    {
        await _historyRepository.SaveHistoryAsync(
            CommandType.ReviewGrammar,
            request.Text,
            result is null ? null : _serializer.Serialize(result),
            cancellationToken
        );
    }
}
