using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class SecretsProviderSecretNotFoundThrowsExternalServiceExceptionTest : TestBase
{
    public SecretsProviderSecretNotFoundThrowsExternalServiceExceptionTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Run()
    {
        WireMockServer.Given(
            Request
                .Create()
                .WithPath("/secrets/nonexistent-key")
                .UsingGet()
        )
        .RespondWith(
            Response
                .Create()
                .WithStatusCode(404)
        );

        var provider = new TestSecretsProvider(WireMockServer.Url!);

        var exception = await Assert.ThrowsAsync<ExternalServiceException>(
            async () => await provider.GetSecretAsync("nonexistent-key")
        );

        await Verify(new
        {
            ServiceName = exception.ServiceName,
            Message = exception.Message,
            ContainsNotFound = exception.Message.Contains("not found")
        });
    }
}
