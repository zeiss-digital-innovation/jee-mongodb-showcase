using Xunit;

namespace DotNetMongoDbBackend.Tests;

public class WeatherProviderTests
{
    [Fact]
    public void GetForecast_ReturnsArray()
    {
        var provider = new LocalWeatherProvider();
    var arr = provider.GetForecast();
    Assert.NotNull(arr);
    Assert.True(arr.Length > 0);
    }
}
