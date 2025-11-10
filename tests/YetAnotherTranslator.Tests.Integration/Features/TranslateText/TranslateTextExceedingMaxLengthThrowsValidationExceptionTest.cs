using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateText;

public class TranslateTextExceedingMaxLengthThrowsValidationExceptionTest : TestBase
{
    private TranslateTextHandler _handler = null!;

    public TranslateTextExceedingMaxLengthThrowsValidationExceptionTest(IntegrationTestFixture fixture) : base(fixture)
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
        // Arrange
        string inputText = new string('a', 5001); // Exceeds 5000 character limit

        var request = new TranslateTextRequest(inputText, SourceLanguage.Auto, "English", UseCache: false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await _handler.HandleAsync(request)
        );

        await Verify(new { ExceptionMessage = exception.Message, Errors = exception.Errors });
    }
}
