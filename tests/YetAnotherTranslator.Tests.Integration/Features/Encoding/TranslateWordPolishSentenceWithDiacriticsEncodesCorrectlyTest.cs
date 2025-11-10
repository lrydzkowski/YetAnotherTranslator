using System.Text;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Encoding;

public class TranslateWordPolishSentenceWithDiacriticsEncodesCorrectlyTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordPolishSentenceWithDiacriticsEncodesCorrectlyTest(IntegrationTestFixture fixture) : base(fixture)
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
        string polishWord = "źródło"; // source/spring
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""source"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""S AO1 R S"",
      ""examples"": [""To jest źródło wody.""]
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

        var request = new TranslateWordRequest(polishWord, SourceLanguage.Polish, "English", UseCache: false);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Verify UTF-8 encoding for word
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(result.InputText);
        string decodedText = Encoding.UTF8.GetString(utf8Bytes);

        // Verify UTF-8 encoding for example sentence
        byte[] exampleBytes = Encoding.UTF8.GetBytes(result.Translations[0].Examples[0]);
        string decodedExample = Encoding.UTF8.GetString(exampleBytes);

        // Assert
        await Verify(new
        {
            Result = result,
            InputWordRoundTrip = decodedText,
            ExampleRoundTrip = decodedExample,
            ExampleContainsDiacritics = decodedExample.Contains("źródło")
        });
    }
}
