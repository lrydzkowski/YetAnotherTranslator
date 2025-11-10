using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReviewGrammar;

public class ReviewGrammarTextWithGrammarErrorsReturnsGrammarIssuesTest : TestBase
{
    private ReviewGrammarHandler _handler = null!;

    public ReviewGrammarTextWithGrammarErrorsReturnsGrammarIssuesTest(IntegrationTestFixture fixture) : base(fixture)
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
        string inputText = "The cat sit on the mat. She don't like dogs.";

        string grammarResponse = @"{
  ""grammarIssues"": [
    {
      ""issue"": ""Subject-verb disagreement"",
      ""correction"": ""The cat sits (not 'sit')"",
      ""explanation"": ""Singular subject requires singular verb""
    },
    {
      ""issue"": ""Incorrect verb form"",
      ""correction"": ""She doesn't (not 'don't')"",
      ""explanation"": ""Third person singular requires 'doesn't'""
    }
  ],
  ""vocabularySuggestions"": []
}";

        // Mock language detection to confirm it's English
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""{{\\""language\\"": \\""English\\"", \\""confidence\\"": 95}}""}}]}}")
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
