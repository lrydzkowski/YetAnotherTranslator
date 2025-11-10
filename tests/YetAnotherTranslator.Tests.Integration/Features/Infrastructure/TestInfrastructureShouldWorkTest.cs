using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Infrastructure;

public class TestInfrastructureShouldWorkTest : TestBase
{
    public TestInfrastructureShouldWorkTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Run()
    {
        // Arrange & Act
        await Task.CompletedTask;

        // Assert
        await Verify(new { InfrastructureWorking = true });
    }
}
