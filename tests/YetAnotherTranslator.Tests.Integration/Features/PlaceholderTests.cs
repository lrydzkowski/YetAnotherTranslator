using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class PlaceholderTests : TestBase
{
    public PlaceholderTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task TestInfrastructure_ShouldWork()
    {
        await Task.CompletedTask;
        Assert.True(true);
    }
}
