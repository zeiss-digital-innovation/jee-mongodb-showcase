#nullable disable

using DotNetMapsFrontend.Models;
using DotNetMapsFrontend.Services;
using DotNetMapsFrontend.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Comprehensive tests for PointOfInterestService CRUD operations
/// Tests cover GetById, Create, Update, and Delete operations to improve code coverage
/// </summary>
[TestFixture]
public class PointOfInterestServiceCrudTests
{
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILogger<PointOfInterestService>> _mockLogger;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private PointOfInterestService _service;
    private const string TestApiBaseUrl = "http://localhost:8080/api";

    [SetUp]
    public void SetUp()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PointOfInterestService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);
        
        _mockConfiguration.Setup(c => c["PointOfInterestApi:BaseUrl"])
            .Returns(TestApiBaseUrl);

        _service = new PointOfInterestService(
            _mockHttpClientFactory.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    #region GetPointOfInterestByIdAsync Tests

    [Test]
    public async Task GetPointOfInterestByIdAsync_WithValidId_ReturnsPointOfInterest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var expectedPoi = new PointOfInterest
        {
            Href = testId,
            Category = "RESTAURANT",
            Details = "Test Restaurant",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        var responseJson = JsonSerializer.Serialize(expectedPoi);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == $"{TestApiBaseUrl}/poi/{testId}"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetPointOfInterestByIdAsync(testId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Href, Is.EqualTo(testId));
        Assert.That(result.Category, Is.EqualTo("restaurant")); // Should be lowercase
        Assert.That(result.Details, Is.EqualTo("Test Restaurant"));
    }

    [Test]
    public async Task GetPointOfInterestByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var testId = "nonexistent123";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetPointOfInterestByIdAsync(testId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetPointOfInterestByIdAsync_WhenApiReturnsError_ThrowsException()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => 
            await _service.GetPointOfInterestByIdAsync(testId));
    }

    [Test]
    public void GetPointOfInterestByIdAsync_WhenApiThrowsException_PropagatesException()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => 
            await _service.GetPointOfInterestByIdAsync(testId));
    }

    #endregion

    #region CreatePointOfInterestAsync Tests

    [Test]
    public async Task CreatePointOfInterestAsync_WithValidData_ReturnsCreatedPoi()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "New Restaurant",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        var createdPoi = new PointOfInterest
        {
            Href = "507f1f77bcf86cd799439011",
            Category = "RESTAURANT",
            Details = "New Restaurant",
            Location = newPoi.Location
        };

        var responseJson = JsonSerializer.Serialize(createdPoi);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(responseJson)
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString() == $"{TestApiBaseUrl}/poi"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.CreatePointOfInterestAsync(newPoi);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Href, Is.Not.Null);
        Assert.That(result.Category, Is.EqualTo("restaurant")); // Should be lowercase
        Assert.That(result.Details, Is.EqualTo("New Restaurant"));
    }

    [Test]
    public void CreatePointOfInterestAsync_WhenApiReturnsError_ThrowsException()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "New Restaurant",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => 
            await _service.CreatePointOfInterestAsync(newPoi));
    }

    [Test]
    public void CreatePointOfInterestAsync_WhenApiBaseUrlNotConfigured_ThrowsException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["PointOfInterestApi:BaseUrl"])
            .Returns((string)null);

        var service = new PointOfInterestService(
            _mockHttpClientFactory.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );

        var newPoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "New Restaurant",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await service.CreatePointOfInterestAsync(newPoi));
    }

    #endregion

    #region UpdatePointOfInterestAsync Tests

    [Test]
    public async Task UpdatePointOfInterestAsync_WithValidData_ReturnsUpdatedPoi()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "museum",
            Details = "Updated Museum",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        var updatedPoi = new PointOfInterest
        {
            Href = testId,
            Category = "MUSEUM",
            Details = "Updated Museum",
            Location = updatePoi.Location
        };

        var responseJson = JsonSerializer.Serialize(updatedPoi);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson)
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString() == $"{TestApiBaseUrl}/poi/{testId}"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.UpdatePointOfInterestAsync(testId, updatePoi);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Href, Is.EqualTo(testId));
        Assert.That(result.Category, Is.EqualTo("museum")); // Should be lowercase
        Assert.That(result.Details, Is.EqualTo("Updated Museum"));
    }

    [Test]
    public async Task UpdatePointOfInterestAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var testId = "nonexistent123";
        var updatePoi = new PointOfInterest
        {
            Category = "museum",
            Details = "Updated Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.UpdatePointOfInterestAsync(testId, updatePoi);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void UpdatePointOfInterestAsync_WhenApiReturnsError_ThrowsException()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "museum",
            Details = "Updated Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => 
            await _service.UpdatePointOfInterestAsync(testId, updatePoi));
    }

    [Test]
    public void UpdatePointOfInterestAsync_WhenApiBaseUrlNotConfigured_ThrowsException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["PointOfInterestApi:BaseUrl"])
            .Returns((string)null);

        var service = new PointOfInterestService(
            _mockHttpClientFactory.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );

        var updatePoi = new PointOfInterest
        {
            Category = "museum",
            Details = "Updated Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await service.UpdatePointOfInterestAsync("someid", updatePoi));
    }

    #endregion

    #region DeletePointOfInterestAsync Tests

    [Test]
    public async Task DeletePointOfInterestAsync_WithValidId_CompletesSuccessfully()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri.ToString() == $"{TestApiBaseUrl}/poi/{testId}"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _service.DeletePointOfInterestAsync(testId));
    }

    [Test]
    public async Task DeletePointOfInterestAsync_WithOkResponse_CompletesSuccessfully()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _service.DeletePointOfInterestAsync(testId));
    }

    [Test]
    public void DeletePointOfInterestAsync_WhenApiReturnsError_ThrowsException()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => 
            await _service.DeletePointOfInterestAsync(testId));
    }

    [Test]
    public void DeletePointOfInterestAsync_WhenNetworkError_ThrowsException()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => 
            await _service.DeletePointOfInterestAsync(testId));
    }

    [Test]
    public void DeletePointOfInterestAsync_WhenApiBaseUrlNotConfigured_ThrowsException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["PointOfInterestApi:BaseUrl"])
            .Returns((string)null);

        var service = new PointOfInterestService(
            _mockHttpClientFactory.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await service.DeletePointOfInterestAsync("someid"));
    }

    #endregion
}
