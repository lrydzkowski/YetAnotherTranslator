using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordNoCacheFlagBypassesCacheTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordNoCacheFlagBypassesCacheTest(IntegrationTestFixture fixture) : base(fixture)
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
        string mockResponse1 = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""house"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""HH AW1 S"",
      ""examples"": [""I live in a big house.""]
    }
  ]
}";

        string mockResponse2 = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""home"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""HH OW1 M"",
      ""examples"": [""Welcome to my home.""]
    }
  ]
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .InScenario("no-cache")
        .WillSetStateTo("second-call")
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse1)}}}]}}")
        );

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .InScenario("no-cache")
        .WhenStateIs("second-call")
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(mockResponse2)}}}]}}")
        );

        var request = new TranslateWordRequest("dom", SourceLanguage.Polish, "English", UseCache: false);

        // Act
        TranslationResult firstResult = await _handler.HandleAsync(request);
        TranslationResult secondResult = await _handler.HandleAsync(request);

        // Assert
        var results = new
        {
            FirstResult = firstResult,
            SecondResult = secondResult
        };
        await Verify(results);
    }
}
