using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.History;

public class GetHistoryWithLimitReturnsLimitedResultsTest : TestBase
{
    private GetHistoryHandler _handler = null!;
    private TranslateWordHandler _translateWordHandler = null!;

    public GetHistoryWithLimitReturnsLimitedResultsTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        var getHistoryValidator = new GetHistoryValidator();
        _handler = new GetHistoryHandler(getHistoryValidator, historyRepository);

        var llmProvider = new TestLlmProvider(WireMockServer.Url!);
        var translateWordValidator = new TranslateWordValidator();
        _translateWordHandler = new TranslateWordHandler(llmProvider, translateWordValidator, historyRepository);
    }

    [Fact]
    public async Task Run()
    {
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""test"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""uncountable"",
      ""cmuArpabet"": ""T EH1 S T"",
      ""examples"": [""This is a test.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse)}}}]}}")
        );

        for (int i = 0; i < 5; i++)
        {
            var request = new TranslateWordRequest($"word{i}", SourceLanguage.Polish, "English", UseCache: false);
            await _translateWordHandler.HandleAsync(request, CancellationToken.None);
        }

        var getHistoryRequest = new GetHistoryRequest(Limit: 3);
        var result = await _handler.HandleAsync(getHistoryRequest, CancellationToken.None);

        var settings = new VerifySettings();
        settings.ScrubMember("Timestamp");
        await Verify(result, settings);
    }
}
