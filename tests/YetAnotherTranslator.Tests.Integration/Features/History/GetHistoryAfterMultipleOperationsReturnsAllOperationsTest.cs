using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.History;

public class GetHistoryAfterMultipleOperationsReturnsAllOperationsTest : TestBase
{
    private GetHistoryHandler _handler = null!;
    private TranslateWordHandler _translateWordHandler = null!;
    private TranslateTextHandler _translateTextHandler = null!;
    private ReviewGrammarHandler _reviewGrammarHandler = null!;

    public GetHistoryAfterMultipleOperationsReturnsAllOperationsTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        var getHistoryValidator = new GetHistoryValidator();
        _handler = new GetHistoryHandler(getHistoryValidator, historyRepository);

        var llmProvider = new TestLlmProvider(WireMockServer.Url!);

        var translateWordValidator = new TranslateWordValidator();
        _translateWordHandler = new TranslateWordHandler(llmProvider, translateWordValidator, historyRepository);

        var translateTextValidator = new TranslateTextValidator();
        _translateTextHandler = new TranslateTextHandler(llmProvider, translateTextValidator, historyRepository);

        var reviewGrammarValidator = new ReviewGrammarValidator();
        _reviewGrammarHandler = new ReviewGrammarHandler(llmProvider, reviewGrammarValidator, historyRepository);
    }

    [Fact]
    public async Task Run()
    {

        string wordTranslationResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""dog"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""cmuArpabet"": ""D AO1 G"",
      ""examples"": [""The dog barked.""]
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
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(wordTranslationResponse)}}}]}}")
        );

        var translateWordRequest = new TranslateWordRequest("pies", SourceLanguage.Polish, "English", UseCache: false);
        await _translateWordHandler.HandleAsync(translateWordRequest, CancellationToken.None);

        string detectLanguageResponse = @"{
  ""language"": ""Polish"",
  ""confidence"": 95
}";
        string textTranslationResponse = "This is a test sentence.";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(detectLanguageResponse)}}}]}}")
        );

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(textTranslationResponse)}}}]}}")
        );

        var translateTextRequest = new TranslateTextRequest("To jest zdanie testowe.", SourceLanguage.Polish, "English", UseCache: false);
        await _translateTextHandler.HandleAsync(translateTextRequest, CancellationToken.None);

        string grammarDetectResponse = @"{
  ""language"": ""English"",
  ""confidence"": 98
}";
        string grammarReviewResponse = @"{
  ""grammarIssues"": [
    {
      ""issue"": ""Subject-verb agreement error"",
      ""correction"": ""The dogs are barking"",
      ""explanation"": ""Plural subject requires plural verb""
    }
  ],
  ""vocabularySuggestions"": []
}";

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(grammarDetectResponse)}}}]}}")
        );

        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(grammarReviewResponse)}}}]}}")
        );

        var reviewGrammarRequest = new ReviewGrammarRequest("The dogs is barking");
        await _reviewGrammarHandler.HandleAsync(reviewGrammarRequest, CancellationToken.None);

        var getHistoryRequest = new GetHistoryRequest(Limit: 50);
        var result = await _handler.HandleAsync(getHistoryRequest, CancellationToken.None);

        var settings = new VerifySettings();
        settings.ScrubMember("Timestamp");
        await Verify(result, settings);
    }
}
