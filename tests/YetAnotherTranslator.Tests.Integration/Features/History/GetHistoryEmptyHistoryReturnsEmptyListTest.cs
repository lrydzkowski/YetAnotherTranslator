using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.History;

public class GetHistoryEmptyHistoryReturnsEmptyListTest : TestBase
{
    private GetHistoryHandler _handler = null!;

    public GetHistoryEmptyHistoryReturnsEmptyListTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();
        var getHistoryValidator = new GetHistoryValidator();
        _handler = new GetHistoryHandler(getHistoryValidator, historyRepository);
    }

    [Fact]
    public async Task Run()
    {
        var request = new GetHistoryRequest(Limit: 50);

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        await Verify(result);
    }
}
