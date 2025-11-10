using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReviewGrammar;

public class ReviewGrammarGrammaticallyCorrectTextReturnsVocabularySuggestionsTest : TestBase
{
    private ReviewGrammarHandler _handler = null!;

    public ReviewGrammarGrammaticallyCorrectTextReturnsVocabularySuggestionsTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var validator = new ReviewGrammarValidator();
        var llmProvider = new TestLlmProvider(WireMockServer.Url!);
        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        _handler = new ReviewGrammarHandler(llmProvider, validator, historyRepository);
    }

    [Fact]
    public async Task Run()
    {
        // Arrange
        string inputText = "The weather is good today. I am happy.";

        string grammarResponse = @"{
  ""grammarIssues"": [],
  ""vocabularySuggestions"": [
    {
      ""original"": ""good"",
      ""suggestion"": ""pleasant"",
      ""context"": ""More descriptive for weather""
    },
    {
      ""original"": ""happy"",
      ""suggestion"": ""delighted"",
      ""context"": ""Stronger expression of emotion""
    }
  ]
}";

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
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""{{\\""language\\"": \\""English\\"", \\""confidence\\"": 98}}""}}]}}")
        );

        // Mock grammar review
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => !b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":{System.Text.Json.JsonSerializer.Serialize(grammarResponse)}}}]}}")
        );

        var request = new ReviewGrammarRequest(inputText);

        // Act
        GrammarReviewResult result = await _handler.HandleAsync(request);

        // Assert
        await Verify(result);
    }
}
