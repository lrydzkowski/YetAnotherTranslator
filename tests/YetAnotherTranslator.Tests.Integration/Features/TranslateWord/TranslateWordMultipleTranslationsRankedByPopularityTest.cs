using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordMultipleTranslationsRankedByPopularityTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordMultipleTranslationsRankedByPopularityTest(IntegrationTestFixture fixture) : base(fixture)
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
      ""word"": ""castle"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""K AE1 S AH0 L"",
      ""examples"": [""The castle stood on the hill.""]
    },
    {
      ""rank"": 2,
      ""word"": ""lock"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""L AA1 K"",
      ""examples"": [""Turn the lock to open the door.""]
    },
    {
      ""rank"": 3,
      ""word"": ""zipper"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""Z IH1 P ER0"",
      ""examples"": [""The zipper on my jacket is broken.""]
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

        var request = new TranslateWordRequest("zamek", SourceLanguage.Polish, "English", UseCache: false);

        TranslationResult result = await _handler.HandleAsync(request);

        await Verify(result);
    }
}
