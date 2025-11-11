using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class SecretsProviderPermissionDeniedThrowsExternalServiceExceptionTest : TestBase
{
    public SecretsProviderPermissionDeniedThrowsExternalServiceExceptionTest(IntegrationTestFixture fixture) : base(fixture)
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
                .WithStatusCode(403)
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
            ContainsAccessDenied = exception.Message.Contains("Access denied"),
            ContainsAzLogin = exception.Message.Contains("az login")
        });
    }
}
