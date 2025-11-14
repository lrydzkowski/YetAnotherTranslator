using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordPolishToEnglishReturnsCmuArpabetTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordPolishToEnglishReturnsCmuArpabetTest(IntegrationTestFixture fixture) : base(fixture)
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
      ""word"": ""cat"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""K AE1 T"",
      ""examples"": [""The cat sat on the mat.""]
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

        var request = new TranslateWordRequest("kot", SourceLanguage.Polish, "English", UseCache: false);

        TranslationResult result = await _handler.HandleAsync(request);

        await Verify(result);
    }
}
