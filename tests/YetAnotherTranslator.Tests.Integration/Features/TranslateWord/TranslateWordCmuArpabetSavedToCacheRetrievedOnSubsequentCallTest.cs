using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordCmuArpabetSavedToCacheRetrievedOnSubsequentCallTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordCmuArpabetSavedToCacheRetrievedOnSubsequentCallTest(IntegrationTestFixture fixture) : base(fixture)
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
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""tree"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""T R IY1"",
      ""examples"": [""The tree is tall.""]
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

        var request = new TranslateWordRequest("drzewo", SourceLanguage.Polish, "English", UseCache: true);

        TranslationResult firstResult = await _handler.HandleAsync(request);

        WireMockServer.ResetMappings();

        TranslationResult cachedResult = await _handler.HandleAsync(request);

        var results = new
        {
            FirstResult = firstResult,
            CachedResult = cachedResult
        };
        await Verify(results);
    }
}
