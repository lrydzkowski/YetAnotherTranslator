using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordOfflineCacheRetrievesFromCacheWhenServerUnavailableTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordOfflineCacheRetrievesFromCacheWhenServerUnavailableTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var validator = new TranslateWordValidator();
        var llmProvider = new TestLlmProvider(WireMockServer.Url!);
        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        _handler = new TranslateWordHandler(llmProvider, validator, historyRepository);
    }

    [Fact]
    public async Task Run()
    {
        // Arrange
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""book"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""B UH1 K"",
      ""examples"": [""I read a good book.""]
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

        var request = new TranslateWordRequest("książka", "Polish", "English", UseCache: true);

        // Act
        TranslationResult firstResult = await _handler.HandleAsync(request);

        WireMockServer.Stop();

        TranslationResult offlineResult = await _handler.HandleAsync(request);

        // Assert
        var results = new
        {
            FirstResult = firstResult,
            OfflineResult = offlineResult
        };
        await Verify(results);
    }
}
