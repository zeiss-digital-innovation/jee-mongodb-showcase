namespace DotNetMongoDbBackend.Tests;

public record WeatherForecast(System.DateOnly Date, int TemperatureC, string Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
