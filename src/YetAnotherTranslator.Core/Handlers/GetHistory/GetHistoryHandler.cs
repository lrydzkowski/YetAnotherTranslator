using FluentValidation;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Core.Handlers.GetHistory;

public class GetHistoryHandler
{
    private readonly IValidator<GetHistoryRequest> _validator;
    private readonly IHistoryRepository _historyRepository;

    public GetHistoryHandler(
        IValidator<GetHistoryRequest> validator,
        IHistoryRepository historyRepository)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public async Task<GetHistoryResult> HandleAsync(
        GetHistoryRequest request,
        CancellationToken cancellationToken = default)
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
