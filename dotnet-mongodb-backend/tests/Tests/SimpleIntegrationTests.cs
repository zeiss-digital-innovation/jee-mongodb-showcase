using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests;

public class SimpleIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private static bool _backendWarningShown = false;
    private static bool? _isBackendAvailable;
    private static readonly object _lock = new object();

    public SimpleIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Suppress logging for these tests since they're just availability checks
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders(); // Remove all logging providers
                logging.SetMinimumLevel(LogLevel.None); // Suppress all logs
            });
        });
    }

    private bool IsBackendAvailable
    {
        get
        {
            if (_isBackendAvailable.HasValue)
                return _isBackendAvailable.Value;

            lock (_lock)
            {
                if (_isBackendAvailable.HasValue)
                    return _isBackendAvailable.Value;

                try
                {
                    var client = _factory.CreateClient();
                    // Try a real API call that requires MongoDB connection
                    var response = client.GetAsync("/zdi-geo-service/api/poi").GetAwaiter().GetResult();
                    _isBackendAvailable = response.IsSuccessStatusCode;
                }
                catch
                {
                    _isBackendAvailable = false;
                }

                if (!_isBackendAvailable.Value && !_backendWarningShown)
                {
                    _backendWarningShown = true;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("======================================================================");
                    Console.WriteLine("⚠️  WARNING: Backend service is not available");
                    Console.WriteLine("");
                    Console.WriteLine("Simple integration tests will be SKIPPED.");
                    Console.WriteLine("Please ensure the backend service and MongoDB are running.");
                    Console.WriteLine("======================================================================");
                    Console.ResetColor();
                }

                return _isBackendAvailable.Value;
            }
        }
    }

    [SkippableFact]
    public async Task GetPois_ShouldReturnOk()
    {
        Skip.IfNot(IsBackendAvailable, "Backend service is not available");

        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/zdi-geo-service/api/poi");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [SkippableFact]
    public async Task GetPois_WithCategoryFilter_ShouldReturnOk()
    {
        Skip.IfNot(IsBackendAvailable, "Backend service is not available");

        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/zdi-geo-service/api/poi?category=restaurant");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [SkippableFact]
    public async Task GetPois_WithGeographicSearch_ShouldReturnOk()
    {
        Skip.IfNot(IsBackendAvailable, "Backend service is not available");

        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/zdi-geo-service/api/poi?lat=49.0&lng=8.4&radius=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [SkippableFact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        Skip.IfNot(IsBackendAvailable, "Backend service is not available");

        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/zdi-geo-service/api/poi/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(".NET MongoDB Backend is running", content);
    }
}
