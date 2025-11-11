using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReviewGrammar;

public class ReviewGrammarNonEnglishTextThrowsValidationExceptionTest : TestBase
{
    private ReviewGrammarHandler _handler = null!;

    public ReviewGrammarNonEnglishTextThrowsValidationExceptionTest(IntegrationTestFixture fixture) : base(fixture)
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
        string inputText = "Witaj świecie! To jest test.";

        // Mock language detection to return Polish
        WireMockServer.Given(
            Request.Create()
                .WithPath("/v1/messages")
                .UsingPost()
                .WithBody(b => b!.Contains("Detect the language"))
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody($@"{{""content"":[{{""type"":""text"",""text"":""{{\\""language\\"": \\""Polish\\"", \\""confidence\\"": 95}}""}}]}}")
        );

        var request = new ReviewGrammarRequest(inputText);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await _handler.HandleAsync(request)
        );

        await Verify(new { ExceptionMessage = exception.Message, Errors = exception.Errors });
    }
}
