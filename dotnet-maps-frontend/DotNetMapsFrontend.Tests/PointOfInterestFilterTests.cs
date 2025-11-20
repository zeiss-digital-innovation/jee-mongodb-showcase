#nullable disable

using Moq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using DotNetMapsFrontend.Services;
using DotNetMapsFrontend.Models;
using DotNetMapsFrontend.Constants;
using System.Net;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Tests for the POI pages - client-side filtering is tested via JavaScript unit tests
/// These tests verify that pages load correctly and contain necessary JavaScript for filtering
/// </summary>
[TestFixture]
public class PointOfInterestFilterTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private Mock<IPointOfInterestService> _mockService;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IPointOfInterestService>();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing service registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IPointOfInterestService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add mock service
                    services.AddSingleton(_mockService.Object);
                });
            });

        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldReturnOK()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/poi?lat=MapDefaults.DefaultLatitude&lon=MapDefaults.DefaultLongitude&radius=MapDefaults.DefaultRadius");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task MapIndex_ShouldReturnOK()
    {
        // Act
        var response = await _client.GetAsync("/Map");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldContainJavaScript()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/poi?lat=MapDefaults.DefaultLatitude&lon=MapDefaults.DefaultLongitude&radius=MapDefaults.DefaultRadius");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check for JavaScript files
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("site.js"));
        Assert.That(content, Does.Contain("category-manager.js"));
    }

    [Test]
    public async Task MapIndex_ShouldContainJavaScript()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check for JavaScript files
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("site.js"));
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldContainCategoryDropdown()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/poi?lat=MapDefaults.DefaultLatitude&lon=MapDefaults.DefaultLongitude&radius=MapDefaults.DefaultRadius");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check for category filter dropdown
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("categoryDropdown"));
        Assert.That(content, Does.Contain("All Categories"));
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldHaveNameAndDetailsFilters()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/poi?lat=MapDefaults.DefaultLatitude&lon=MapDefaults.DefaultLongitude&radius=MapDefaults.DefaultRadius");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check for Name and Details filter inputs
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("poiNameFilterInput"));
        Assert.That(content, Does.Contain("poiDetailsFilterInput"));
    }

    private List<PointOfInterest> GetTestPointsOfInterest()
    {
        return new List<PointOfInterest>
        {
            new PointOfInterest
            {
                Category = "restaurant",
                Details = "Best Italian Restaurant in town",
                Location = new Location
                {
                    Type = "Point",
                    Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
                },
                Href = "/poi/1"
            },
            new PointOfInterest
            {
                Category = "coffee",
                Details = "Coffee shop with great atmosphere",
                Location = new Location
                {
                    Type = "Point",
                    Coordinates = new[] { 13.7383, 51.0514 }
                },
                Href = "/poi/2"
            },
            new PointOfInterest
            {
                Category = "cash",
                Details = "ATM with low fees",
                Location = new Location
                {
                    Type = "Point",
                    Coordinates = new[] { 13.7393, 51.0524 }
                },
                Href = "/poi/3"
            }
        };
    }
}
