using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.TranslateWord;

public class TranslateWordEmptyWordThrowsValidationExceptionTest : TestBase
{
    private TranslateWordHandler _handler = null!;

    public TranslateWordEmptyWordThrowsValidationExceptionTest(IntegrationTestFixture fixture) : base(fixture)
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

    [Fact]
    public async Task Run()
    {
        var request = new TranslateWordRequest("", SourceLanguage.Polish, "English");

        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await _handler.HandleAsync(request)
        );

        await Verify(new { ExceptionMessage = exception.Message, Errors = exception.Errors });
    }
}
