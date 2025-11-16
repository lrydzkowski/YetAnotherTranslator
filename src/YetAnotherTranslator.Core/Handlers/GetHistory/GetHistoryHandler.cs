using FluentValidation;
using YetAnotherTranslator.Core.Handlers.GetHistory.Interfaces;
using YetAnotherTranslator.Core.Handlers.GetHistory.Models;

namespace YetAnotherTranslator.Core.Handlers.GetHistory;

public class GetHistoryHandler
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IValidator<GetHistoryRequest> _validator;

    public GetHistoryHandler(
        IValidator<GetHistoryRequest> validator,
        IHistoryRepository historyRepository
    )
    {
        _validator = validator;
        _historyRepository = historyRepository;
    }

    public async Task<GetHistoryResult> HandleAsync(
        GetHistoryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        List<HistoryEntry> entries = await _historyRepository.GetHistoryAsync(
            request.Limit,
            cancellationToken
        );

        return new GetHistoryResult
        {
            Entries = entries
        };
    }
}
