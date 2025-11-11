using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Encoding;

public class TranslateWordMultiplePolishDiacriticsEncodesCorrectlyTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordMultiplePolishDiacriticsEncodesCorrectlyTest(IntegrationTestFixture fixture) : base(fixture)
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
        // Arrange - Word with multiple diacritics
        string polishWord = "mąż"; // husband
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""husband"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""HH AH1 Z B AH0 N D"",
      ""examples"": [""Mój mąż pracuje.""]
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

        // Verify UTF-8 encoding
        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(result.InputText);
        string decodedText = System.Text.Encoding.UTF8.GetString(utf8Bytes);
        string hexString = BitConverter.ToString(utf8Bytes);

        // Assert
        await Verify(new
        {
            Result = result,
            Utf8RoundTrip = decodedText,
            HexRepresentation = hexString,
            ContainsAgonekBytes = hexString.Contains("C4-85"), // ą
            ContainsZDotBytes = hexString.Contains("C5-BC") // ż
        });
    }
}
