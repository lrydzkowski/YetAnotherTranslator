using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateText;

public class TranslateTextEnglishToPolishReturnsTranslatedTextTest : TestBase
{
    private TranslateTextHandler _handler = null!;

    public TranslateTextEnglishToPolishReturnsTranslatedTextTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var validator = new TranslateTextValidator();
        var llmProvider = new TestLlmProvider(WireMockServer.Url!);
        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        _handler = new TranslateTextHandler(llmProvider, validator, historyRepository);
    }

    [Fact]
    public async Task Run()
    {
        string inputText = "Hello world! This is a text translation test.";
        string translatedText = "Witaj świecie! To jest test tłumaczenia tekstu.";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => b!.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""English""}}]}}")
        );

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => !b!.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(translatedText)}}}]}}")
        );

        var request = new TranslateTextRequest(inputText, SourceLanguage.Auto, "Polish", UseCache: false);

        TextTranslationResult result = await _handler.HandleAsync(request);

        await Verify(result);
    }
}
