using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Encoding;

public class TranslateWordPolishDiacriticsEncodesCorrectlyInUtf8Test : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordPolishDiacriticsEncodesCorrectlyInUtf8Test(IntegrationTestFixture fixture) : base(fixture)
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

    [Theory]
    [InlineData("ą", "a with ogonek")]
    [InlineData("ć", "c with acute")]
    [InlineData("ę", "e with ogonek")]
    [InlineData("ł", "l with stroke")]
    [InlineData("ń", "n with acute")]
    [InlineData("ó", "o with acute")]
    [InlineData("ś", "s with acute")]
    [InlineData("ź", "z with acute")]
    [InlineData("ż", "z with dot above")]
    public async Task Run(string polishChar, string description)
    {
        string mockResponse = $@"{{
  ""translations"": [
    {{
      ""rank"": 1,
      ""word"": ""{description}"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""uncountable"",
      ""cmuArpabet"": null,
      ""examples"": [""Polish character: {polishChar}""]
    }}
  ]
}}";

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

        var request = new TranslateWordRequest(polishChar, SourceLanguage.Polish, "English", UseCache: false);

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(result.InputText);
        string decodedText = System.Text.Encoding.UTF8.GetString(utf8Bytes);

        await Verify(new
        {
            PolishChar = polishChar,
            Description = description,
            Result = result,
            Utf8RoundTrip = decodedText
        })
        .UseParameters(polishChar, description);
    }
}
