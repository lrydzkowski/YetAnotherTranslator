using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class SecretsProviderNetworkTimeoutThrowsExternalServiceExceptionTest : TestBase
{
    public SecretsProviderNetworkTimeoutThrowsExternalServiceExceptionTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Run()
    {
        // Arrange
        WireMockServer.Given(
            Request
                .Create()
                .WithPath("/secrets/test-key")
                .UsingGet()
        )
        .RespondWith(
            Response
                .Create()
                .WithDelay(TimeSpan.FromSeconds(10))
                .WithStatusCode(200)
        );

        var provider = new TestSecretsProvider(WireMockServer.Url!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExternalServiceException>(
            async () => await provider.GetSecretAsync("test-key")
        );

        await Verify(new
        {
            ServiceName = exception.ServiceName,
            Message = exception.Message,
            ContainsTimedOut = exception.Message.Contains("timed out")
        });
    }
}
