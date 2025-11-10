using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordPolishDiacriticsRendersCorrectlyInUtf8Test : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordPolishDiacriticsRendersCorrectlyInUtf8Test(IntegrationTestFixture fixture) : base(fixture)
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
      ""word"": ""yellow bile"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""uncountable"",
      ""cmuArpabet"": ""Y EH1 L OW0 B AY1 L"",
      ""examples"": [""Yellow bile was one of the four humors.""]
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

        var request = new TranslateWordRequest("żółć", SourceLanguage.Polish, "English", UseCache: false);

        // Act
        TranslationResult result = await _handler.HandleAsync(request);

        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(result.InputText);
        string roundtrip = System.Text.Encoding.UTF8.GetString(utf8Bytes);

        // Assert
        await Verify(new
        {
            Result = result,
            Utf8Roundtrip = roundtrip
        });
    }
}
