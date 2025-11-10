using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordEnglishToPolishNoCmuArpabetTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordEnglishToPolishNoCmuArpabetTest(IntegrationTestFixture fixture) : base(fixture)
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
      ""word"": ""kot"",
      ""partOfSpeech"": ""rzeczownik"",
      ""countability"": ""policzalny"",
      ""examples"": [""Kot siedział na macie.""]
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

        var request = new TranslateWordRequest("cat", SourceLanguage.English, "Polish", UseCache: false);

        // Act
        TranslationResult result = await _handler.HandleAsync(request);

        // Assert
        await Verify(result);
    }
}
