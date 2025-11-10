using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordPronunciationVariantsDifferentArpabetByPartOfSpeechTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordPronunciationVariantsDifferentArpabetByPartOfSpeechTest(IntegrationTestFixture fixture) : base(fixture)
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
      ""word"": ""record"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""R EH1 K ER0 D"",
      ""examples"": [""I bought a vinyl record.""]
    },
    {
      ""rank"": 2,
      ""word"": ""record"",
      ""partOfSpeech"": ""verb"",
      ""countability"": ""N/A"",
      ""cmuArpabet"": ""R IH0 K AO1 R D"",
      ""examples"": [""Please record this meeting.""]
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

        var request = new TranslateWordRequest("nagrywać", SourceLanguage.Polish, "English", UseCache: false);

        // Act
        TranslationResult result = await _handler.HandleAsync(request);

        // Assert
        await Verify(result);
    }
}
