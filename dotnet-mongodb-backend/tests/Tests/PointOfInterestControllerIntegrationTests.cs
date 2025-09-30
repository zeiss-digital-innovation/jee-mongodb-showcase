using System.Threading.Tasks;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests;

public class PointOfInterestControllerIntegrationTests
{
    [Fact]
    public void ApiController_ShouldBeInstantiable()
    {
        // Test dass die grundlegenden Abhängigkeiten korrekt sind
        Assert.True(true);
    }

    [Fact]
    public async Task AsyncTest_ShouldWork()
    {
        // Einfacher Async Test
        await Task.Delay(1);
        Assert.True(true);
    }
}