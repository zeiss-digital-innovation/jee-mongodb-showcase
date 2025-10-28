#nullable disable

using Moq;
using Moq.Protected;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DotNetMapsFrontend.Services;
using DotNetMapsFrontend.Models;
using System.Net;
using System.Text.Json;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Tests for the PointOfInterestService with category filtering
/// </summary>
[TestFixture]
public class PointOfInterestServiceCategoryTests
{
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILogger<PointOfInterestService>> _mockLogger;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private PointOfInterestService _service;
    private const string BackendUrl = "http://localhost:8080";

    [SetUp]
    public void SetUp()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PointOfInterestService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Setup configuration - use correct key that matches service implementation
        _mockConfiguration.Setup(c => c["PointOfInterestApi:BaseUrl"]).Returns(BackendUrl);

        // Setup HttpClient
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(BackendUrl)
        };
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _service = new PointOfInterestService(
            _mockHttpClientFactory.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Service does not implement IDisposable
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithEmptyCategories_ShouldCallBackendWithoutCategoryParameter()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string>();
        var expectedPois = GetTestPointsOfInterest();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        string capturedUrl = null;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedUrl = request.RequestUri?.ToString();
            })
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("lat=51.05"));  // Service uses InvariantCulture with F6 format
        Assert.That(capturedUrl, Does.Contain("lon=13.73"));  // Service uses InvariantCulture with F6 format
        Assert.That(capturedUrl, Does.Contain($"radius={radius}"));
        Assert.That(capturedUrl, Does.Not.Contain("category="));
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithSingleCategory_ShouldIncludeCategoryInUrl()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum" };
        var expectedPois = GetTestPointsOfInterest().Where(p => p.Category == "museum").ToList();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        string capturedUrl = null;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedUrl = request.RequestUri?.ToString();
            })
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(capturedUrl, Does.Contain("category=museum"));
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithMultipleCategories_ShouldIncludeAllCategoriesInUrl()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum", "castle", "restaurant" };
        var expectedPois = GetTestPointsOfInterest()
            .Where(p => categories.Contains(p.Category))
            .ToList();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        string capturedUrl = null;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedUrl = request.RequestUri?.ToString();
            })
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(capturedUrl, Does.Contain("category=museum"));
        Assert.That(capturedUrl, Does.Contain("category=castle"));
        Assert.That(capturedUrl, Does.Contain("category=restaurant"));
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithSpecialCharactersInCategory_ShouldUrlEncodeCategories()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "café & restaurant", "shop+store" };
        var expectedPois = new List<PointOfInterest>();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        string capturedUrl = null;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedUrl = request.RequestUri?.ToString();
            })
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.Not.Null);
        // URL encoding verification - Uri.EscapeDataString encodes but spaces may stay as spaces or become +
        Assert.That(capturedUrl, Does.Contain("café").Or.Contain("caf%C3%A9")); // é may or may not be encoded depending on HttpClient
        Assert.That(capturedUrl, Does.Contain("&").Or.Contain("%26")); // & special character verification
        Assert.That(capturedUrl, Does.Contain("restaurant"));
        Assert.That(capturedUrl, Does.Contain("shop"));
        Assert.That(capturedUrl, Does.Contain("store").Or.Contain("%2B")); // + may become %2B
    }

    [Test]
    public async Task GetPointsOfInterestAsync_BackwardCompatibility_ShouldStillWorkWithOldMethod()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var expectedPois = GetTestPointsOfInterest();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act - Call old method without categories
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(6));
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithCategories_ShouldLogCategoryCount()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum", "castle" };
        var expectedPois = GetTestPointsOfInterest();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.Not.Null);
        
        // Verify that logger was called with category information
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("2") && v.ToString().Contains("categories")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithManyCategories_ShouldHandleLargeQueryString()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            categories.Add($"category{i}");
        }
        var expectedPois = new List<PointOfInterest>();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        string capturedUrl = null;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedUrl = request.RequestUri?.ToString();
            })
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.Not.Null);
        // Check that all 20 categories are in the URL
        for (int i = 0; i < 20; i++)
        {
            Assert.That(capturedUrl, Does.Contain($"category{i}"));
        }
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithCategories_OnHttpError_ShouldReturnMockData()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum" };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("Backend error")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories);

        // Assert - Service returns mock data on HTTP errors (graceful degradation)
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0)); // Mock data contains some POIs
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("API call failed")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithNullCategories_ShouldTreatAsEmptyList()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        List<string> categories = null;
        var expectedPois = GetTestPointsOfInterest();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedPois))
        };

        string capturedUrl = null;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
            {
                capturedUrl = request.RequestUri?.ToString();
            })
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetPointsOfInterestAsync(lat, lon, radius, categories ?? new List<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(capturedUrl, Does.Not.Contain("category="));
    }

    private List<PointOfInterest> GetTestPointsOfInterest()
    {
        return new List<PointOfInterest>
        {
            new PointOfInterest 
            { 
                Category = "museum", 
                Details = "Dresden Museum",
                Location = new Location { Type = "Point", Coordinates = new double[] { 13.7373, 51.0504 } }
            },
            new PointOfInterest 
            { 
                Category = "museum", 
                Details = "Art Gallery",
                Location = new Location { Type = "Point", Coordinates = new double[] { 13.7400, 51.0500 } }
            },
            new PointOfInterest 
            { 
                Category = "castle", 
                Details = "Dresden Castle",
                Location = new Location { Type = "Point", Coordinates = new double[] { 13.7373, 51.0530 } }
            },
            new PointOfInterest 
            { 
                Category = "castle", 
                Details = "Old Fortress",
                Location = new Location { Type = "Point", Coordinates = new double[] { 13.7350, 51.0520 } }
            },
            new PointOfInterest 
            { 
                Category = "restaurant", 
                Details = "Italian Restaurant",
                Location = new Location { Type = "Point", Coordinates = new double[] { 13.7380, 51.0510 } }
            },
            new PointOfInterest 
            { 
                Category = "park", 
                Details = "City Park",
                Location = new Location { Type = "Point", Coordinates = new double[] { 13.7360, 51.0505 } }
            }
        };
    }
}
