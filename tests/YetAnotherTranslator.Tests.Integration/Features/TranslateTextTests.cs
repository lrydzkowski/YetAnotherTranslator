using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class TranslateTextTests : TestBase
{
    private TranslateTextHandler _handler = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var validator = new TranslateTextValidator();
        var llmProvider = new TestLlmProvider(WireMockServer.Url!);
        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        _handler = new TranslateTextHandler(llmProvider, validator, historyRepository);
    }

    [Fact]
    public async Task TranslateText_PolishToEnglish_ReturnsTranslatedText()
    {
        string inputText = "Witaj świecie! To jest test tłumaczenia tekstu.";
        string translatedText = "Hello world! This is a text translation test.";

        // Mock language detection
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""Polish""}}]}}")
        );

        // Mock translation
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => !b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(translatedText)}}}]}}")
        );

        var request = new TranslateTextRequest(inputText, SourceLanguage.Auto, "English", UseCache: false);

        TextTranslationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.SourceLanguage.Should().Be("Polish");
        result.TargetLanguage.Should().Be("English");
        result.InputText.Should().Be(inputText);
        result.TranslatedText.Should().Be(translatedText);
    }

    [Fact]
    public async Task TranslateText_EnglishToPolish_ReturnsTranslatedText()
    {
        string inputText = "Hello world! This is a text translation test.";
        string translatedText = "Witaj świecie! To jest test tłumaczenia tekstu.";

        // Mock language detection
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""English""}}]}}")
        );

        // Mock translation
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => !b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(translatedText)}}}]}}")
        );

        var request = new TranslateTextRequest(inputText, SourceLanguage.Auto, "Polish", UseCache: false);

        TextTranslationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.SourceLanguage.Should().Be("English");
        result.TargetLanguage.Should().Be("Polish");
        result.InputText.Should().Be(inputText);
        result.TranslatedText.Should().Be(translatedText);
    }

    [Fact]
    public async Task TranslateText_ExceedingMaxLength_ThrowsValidationException()
    {
        string inputText = new string('a', 5001); // Exceeds 5000 character limit

        var request = new TranslateTextRequest(inputText, SourceLanguage.Auto, "English", UseCache: false);

        Func<Task> act = async () => await _handler.HandleAsync(request);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage.Contains("5000")));
    }

    [Fact]
    public async Task TranslateText_MultiLineWithEscapedNewlines_PreservesFormatting()
    {
        string inputText = "Pierwsza linia.\nDruga linia.\n\nTrzecia linia po pustej linii.";
        string translatedText = "First line.\nSecond line.\n\nThird line after an empty line.";

        // Mock language detection
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""Polish""}}]}}")
        );

        // Mock translation
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => !b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(translatedText)}}}]}}")
        );

        var request = new TranslateTextRequest(inputText, SourceLanguage.Auto, "English", UseCache: false);

        TextTranslationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.TranslatedText.Should().Contain("\n");
        result.TranslatedText.Should().Be(translatedText);
    }
}
