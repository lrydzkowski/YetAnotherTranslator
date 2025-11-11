using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class LlmProviderConnectionFailureThrowsExternalServiceExceptionTest : TestBase
{
    public LlmProviderConnectionFailureThrowsExternalServiceExceptionTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Run()
    {
        // Arrange
        WireMockServer.Given(
            Request
                .Create()
                .WithPath("/v1/messages")
                .UsingGet()
        )
        .RespondWith(
            Response
                .Create()
                .WithStatusCode(503)
        );

        var provider = new TestLlmProvider(WireMockServer.Url!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExternalServiceException>(
            async () => await provider.DetectLanguageAsync("test")
        );

        await Verify(new
        {
            ServiceName = exception.ServiceName,
            Message = exception.Message,
            ContainsFailedToConnect = exception.Message.Contains("Failed to connect")
        });
    }
}
