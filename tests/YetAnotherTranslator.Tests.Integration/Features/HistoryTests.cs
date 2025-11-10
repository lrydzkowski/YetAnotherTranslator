using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class HistoryTests : TestBase
{
    private GetHistoryHandler _handler = null!;
    private TranslateWordHandler _translateWordHandler = null!;
    private TranslateTextHandler _translateTextHandler = null!;
    private ReviewGrammarHandler _reviewGrammarHandler = null!;

    public HistoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        // Create GetHistoryHandler
        var getHistoryValidator = new GetHistoryValidator();
        var getHistoryRequest = new GetHistoryRequest();
        _handler = new GetHistoryHandler(historyRepository, getHistoryValidator);

        // Create other handlers for performing operations
        var llmProvider = new TestLlmProvider(WireMockServer.Url!);

        var translateWordValidator = new TranslateWordValidator();
        _translateWordHandler = new TranslateWordHandler(llmProvider, translateWordValidator, historyRepository);

        var translateTextValidator = new TranslateTextValidator();
        _translateTextHandler = new TranslateTextHandler(llmProvider, translateTextValidator, historyRepository);

        var reviewGrammarValidator = new ReviewGrammarValidator();
        _reviewGrammarHandler = new ReviewGrammarHandler(llmProvider, reviewGrammarValidator, historyRepository);
    }

    [Fact]
    public async Task GetHistory_EmptyHistory_ReturnsEmptyList()
    {
        // Arrange
        var request = new GetHistoryRequest(Limit: 50);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_AfterMultipleOperations_ReturnsAllOperations()
    {
        // Arrange - Perform multiple operations to populate history

        // 1. Translate a word (Polish to English)
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

        var translateWordRequest = new TranslateWordRequest("pies", "Polish", "English", UseCache: false);
        await _translateWordHandler.HandleAsync(translateWordRequest, CancellationToken.None);

        // 2. Translate text (Polish to English)
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

        // 3. Review grammar
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

        // Act
        var getHistoryRequest = new GetHistoryRequest(Limit: 50);
        var result = await _handler.HandleAsync(getHistoryRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(3);

        // Verify entries are ordered by timestamp (most recent first)
        result.Entries.Should().BeInDescendingOrder(e => e.Timestamp);

        // Verify command types
        result.Entries.Select(e => e.CommandType).Should().Contain(new[]
        {
            CommandType.TranslateWord,
            CommandType.TranslateText,
            CommandType.ReviewGrammar
        });

        // Verify input texts are captured
        result.Entries.Should().Contain(e => e.InputText == "pies");
        result.Entries.Should().Contain(e => e.InputText == "To jest zdanie testowe.");
        result.Entries.Should().Contain(e => e.InputText == "The dogs is barking");
    }

    [Fact]
    public async Task GetHistory_WithLimit_ReturnsLimitedResults()
    {
        // Arrange - Perform 5 operations
        string mockResponse = @"{
  ""translations"": [
    {
      ""rank"": 1,
      ""word"": ""test"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""uncountable"",
      ""cmuArpabet"": ""T EH1 S T"",
      ""examples"": [""This is a test.""]
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

        for (int i = 0; i < 5; i++)
        {
            var request = new TranslateWordRequest($"word{i}", "Polish", "English", UseCache: false);
            await _translateWordHandler.HandleAsync(request, CancellationToken.None);
        }

        // Act - Request only 3 entries
        var getHistoryRequest = new GetHistoryRequest(Limit: 3);
        var result = await _handler.HandleAsync(getHistoryRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(3);
        result.Entries.Should().BeInDescendingOrder(e => e.Timestamp);
    }
}
