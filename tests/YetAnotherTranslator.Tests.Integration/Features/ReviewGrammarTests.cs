using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class ReviewGrammarTests : TestBase
{
    private ReviewGrammarHandler _handler = null!;

    public ReviewGrammarTests(IntegrationTestFixture fixture) : base(fixture)
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
    public async Task ReviewGrammar_TextWithGrammarErrors_ReturnsGrammarIssues()
    {
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

        GrammarReviewResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.InputText.Should().Be(inputText);
        result.GrammarIssues.Should().HaveCount(2);
        result.GrammarIssues[0].Issue.Should().Be("Subject-verb disagreement");
        result.GrammarIssues[0].Correction.Should().Be("The cat sits (not 'sit')");
        result.GrammarIssues[1].Issue.Should().Be("Incorrect verb form");
        result.VocabularySuggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task ReviewGrammar_GrammaticallyCorrectText_ReturnsVocabularySuggestions()
    {
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

        GrammarReviewResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.GrammarIssues.Should().BeEmpty();
        result.VocabularySuggestions.Should().HaveCount(2);
        result.VocabularySuggestions[0].Original.Should().Be("good");
        result.VocabularySuggestions[0].Suggestion.Should().Be("pleasant");
        result.VocabularySuggestions[1].Original.Should().Be("happy");
    }

    [Fact]
    public async Task ReviewGrammar_NonEnglishText_ThrowsValidationException()
    {
        string inputText = "Witaj świecie! To jest test.";

        // Mock language detection to return Polish
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => b.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""{{\\""language\\"": \\""Polish\\"", \\""confidence\\"": 95}}""}}]}}")
        );

        var request = new ReviewGrammarRequest(inputText);

        Func<Task> act = async () => await _handler.HandleAsync(request);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .Where(e => e.Message.Contains("English"));
    }
}
