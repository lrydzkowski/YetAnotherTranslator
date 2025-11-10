using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class EncodingTests : TestBase
{
    private TranslateWordHandler _handler = null!;

    public EncodingTests(IntegrationTestFixture fixture) : base(fixture)
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
    public async Task TranslateWord_PolishDiacritics_EncodesCorrectlyInUtf8(string polishChar, string description)
    {
        // Arrange
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

        var request = new TranslateWordRequest(polishChar, "Polish", "English", UseCache: false);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert - Verify the Polish character is correctly stored
        result.InputText.Should().Be(polishChar);

        // Verify UTF-8 encoding is correct by converting to bytes and back
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(result.InputText);
        string decodedText = Encoding.UTF8.GetString(utf8Bytes);
        decodedText.Should().Be(polishChar);

        // Verify the translation result contains the example with the Polish character
        result.Translations.Should().HaveCount(1);
        result.Translations[0].Examples.Should().Contain(example => example.Contains(polishChar));
    }

    [Fact]
    public async Task TranslateWord_MultiplePolishDiacritics_EncodesCorrectly()
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

        var request = new TranslateWordRequest(polishWord, "Polish", "English", UseCache: false);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.InputText.Should().Be(polishWord);

        // Verify each character is correctly encoded
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(result.InputText);
        string decodedText = Encoding.UTF8.GetString(utf8Bytes);
        decodedText.Should().Be(polishWord);

        // Verify specific bytes for the diacritics
        // 'ą' in UTF-8 is 0xC4 0x85
        // 'ż' in UTF-8 is 0xC5 0xBC
        string hexString = BitConverter.ToString(utf8Bytes);
        hexString.Should().Contain("C4-85"); // ą
        hexString.Should().Contain("C5-BC"); // ż
    }

    [Fact]
    public async Task TranslateWord_PolishSentenceWithDiacritics_EncodesCorrectly()
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

        var request = new TranslateWordRequest(polishWord, "Polish", "English", UseCache: false);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert - Verify the word and examples maintain proper encoding
        result.InputText.Should().Be(polishWord);
        result.Translations[0].Examples[0].Should().Contain("źródło");

        // Verify UTF-8 encoding
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(result.InputText);
        string decodedText = Encoding.UTF8.GetString(utf8Bytes);
        decodedText.Should().Be(polishWord);

        // Verify the example sentence is also correctly encoded
        byte[] exampleBytes = Encoding.UTF8.GetBytes(result.Translations[0].Examples[0]);
        string decodedExample = Encoding.UTF8.GetString(exampleBytes);
        decodedExample.Should().Contain("źródło");
    }

    [Fact]
    public void CommandParser_PolishInput_PreservesEncoding()
    {
        // Arrange
        var parser = new Cli.Repl.CommandParser();
        string polishInput = "/t książka"; // book

        // Act
        var command = parser.Parse(polishInput);

        // Assert
        command.Argument.Should().Be("książka");

        // Verify UTF-8 encoding is preserved
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(command.Argument);
        string decodedText = Encoding.UTF8.GetString(utf8Bytes);
        decodedText.Should().Be("książka");
    }
}
