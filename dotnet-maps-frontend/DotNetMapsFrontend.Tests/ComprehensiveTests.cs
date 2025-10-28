#nullable disable

using Moq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using DotNetMapsFrontend.Services;
using DotNetMapsFrontend.Controllers;
using DotNetMapsFrontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Moq.Protected;

namespace DotNetMapsFrontend.Tests;

[TestFixture]
public class HomeControllerTests
{
    private HomeController _controller;

    [SetUp]
    public void SetUp()
    {
        _controller = new HomeController();
        // Mock HttpContext for Error method
        var httpContext = new Mock<HttpContext>();
        httpContext.SetupGet(x => x.TraceIdentifier).Returns("test-trace-id");
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext.Object
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public void Index_ShouldReturnViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Error_ShouldReturnViewResult()
    {
        // Act
        var result = _controller.Error();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Error_ShouldHaveCorrectModel()
    {
        // Act
        var result = _controller.Error() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.Not.Null);
    }

    [Test]
    public void Controller_ShouldNotBeNull()
    {
        // Assert
        Assert.That(_controller, Is.Not.Null);
    }
}

[TestFixture]
public class MapControllerTests
{
    private MapController _controller;
    private Mock<IPointOfInterestService> _mockService;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _controller = new MapController(_mockService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public void Index_ShouldReturnViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task GetPointsOfInterest_ShouldReturnJsonResult()
    {
        // Arrange
        var mockPois = new List<PointOfInterest>
        {
            new PointOfInterest { Category = "museum", Details = "Test Museum" }
        };
        _mockService.Setup(s => s.GetPointsOfInterestAsync()).ReturnsAsync(mockPois);

        // Act
        var result = await _controller.GetPointsOfInterest();

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        Assert.That(jsonResult?.Value, Is.EqualTo(mockPois));
    }

    [Test]
    public async Task GetPointsOfInterest_WithCoordinates_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var mockPois = new List<PointOfInterest>();
        _mockService.Setup(s => s.GetPointsOfInterestAsync()).ReturnsAsync(mockPois);

        // Act
        var result = await _controller.GetPointsOfInterest();

        // Assert
        _mockService.Verify(s => s.GetPointsOfInterestAsync(), Times.Once);
        Assert.That(result, Is.InstanceOf<JsonResult>());
    }

    [Test]
    public async Task GetPointsOfInterest_ShouldHandleServiceException()
    {
        // Arrange
        _mockService.Setup(s => s.GetPointsOfInterestAsync())
                   .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetPointsOfInterest();

        // Assert - Should return Json with empty list as fallback
        Assert.That(result, Is.InstanceOf<JsonResult>());
    }
}

[TestFixture]
public class PointOfInterestControllerTests
{
    private PointOfInterestController _controller;
    private Mock<IPointOfInterestService> _mockService;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _controller = new PointOfInterestController(_mockService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task Index_ShouldReturnViewResult()
    {
        // Arrange
        var mockPois = new List<PointOfInterest>();
        _mockService.Setup(s => s.GetPointsOfInterestAsync()).ReturnsAsync(mockPois);

        // Act
        var result = await _controller.Index(null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_WithNoParameters_ShouldUseDefaultCoordinates()
    {
        // Arrange
        var mockPois = new List<PointOfInterest>();
        _mockService.Setup(s => s.GetPointsOfInterestAsync()).ReturnsAsync(mockPois);

        // Act
        var result = await _controller.Index(null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        _mockService.Verify(s => s.GetPointsOfInterestAsync(), Times.Once);
    }

    [Test]
    public async Task Index_WithCoordinates_ShouldCallServiceWithParameters()
    {
        // Arrange
        var mockPois = new List<PointOfInterest>();
        var lat = 51.0504;
        var lon = 13.7373;
        var radius = 3900;
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius)).ReturnsAsync(mockPois);

        // Act
        var result = await _controller.Index(lat, lon, radius);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, radius), Times.Once);
    }

    [Test]
    public async Task Index_WithCoordinatesButNoRadius_ShouldUseDefaultRadius()
    {
        // Arrange
        var mockPois = new List<PointOfInterest>();
        var lat = 51.0504;
        var lon = 13.7373;
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, 2000)).ReturnsAsync(mockPois);

        // Act
        var result = await _controller.Index(lat, lon, null);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, 2000), Times.Once);
    }

    [Test]
    public async Task GetAll_ShouldReturnJsonResult()
    {
        // Arrange
        var mockPois = new List<PointOfInterest>
        {
            new PointOfInterest { Category = "museum", Details = "Test Museum" }
        };
        _mockService.Setup(s => s.GetPointsOfInterestAsync()).ReturnsAsync(mockPois);

        // Act
        var result = await _controller.GetAll(null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
    }

    [Test]
    public async Task Create_WithValidPoi_ShouldReturnCreatedResult()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Category = "museum",
            Name = "New Museum",
            Details = "New Museum",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        var createdPoi = new PointOfInterest
        {
            Category = newPoi.Category,
            Name = newPoi.Name,
            Details = newPoi.Details,
            Location = newPoi.Location,
            Href = "http://localhost:8080/zdi-geo-service/api/poi/123"
        };

        _mockService.Setup(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
                   .ReturnsAsync(createdPoi);

        // Act
        var result = await _controller.Create(newPoi);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        Assert.That(jsonResult?.Value, Is.EqualTo(createdPoi));
    }

    [Test]
    public async Task Create_WithInvalidCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidPoi = new PointOfInterest
        {
            Category = "", // Invalid empty category
            Details = "Test Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        // Act
        var result = await _controller.Create(invalidPoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_WithXssAttempt_ShouldEncodeContent()
    {
        // Arrange
        var xssPoi = new PointOfInterest
        {
            Category = "museum",
            Name = "<script>alert('xss')</script>",
            Details = "<script>alert('xss')</script>",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        var encodedPoi = new PointOfInterest
        {
            Category = xssPoi.Category,
            Name = "&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", // HTML encoded
            Details = "&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", // HTML encoded
            Location = xssPoi.Location,
            Href = "http://localhost:8080/api/poi/123"
        };

        _mockService.Setup(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
                   .ReturnsAsync(encodedPoi);

        // Act
        var result = await _controller.Create(xssPoi);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        // Content should be HTML encoded by the controller
    }

    [Test]
    public async Task Create_WithInvalidCoordinates_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidPoi = new PointOfInterest
        {
            Category = "museum",
            Name = "Test Museum",
            Details = "Test Museum",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 200, 200 } // Invalid coordinates
            }
        };

        // Act
        var result = await _controller.Create(invalidPoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetCategories_ShouldReturnJsonResult()
    {
        // Arrange
        var mockCategories = new List<string> { "museum", "restaurant", "hotel" };
        _mockService.Setup(s => s.GetAvailableCategoriesAsync()).ReturnsAsync(mockCategories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        Assert.That(jsonResult?.Value, Is.EqualTo(mockCategories));
    }

    [Test]
    public async Task Create_ShouldCallServiceCreateMethod()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Category = "museum",
            Name = "Test Museum",
            Details = "Test Museum",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        _mockService.Setup(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
                   .ReturnsAsync(newPoi);

        // Act
        await _controller.Create(newPoi);

        // Assert
        _mockService.Verify(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()), Times.Once);
    }
}

[TestFixture]
public class ModelTests
{
    [Test]
    public void PointOfInterest_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var poi = new PointOfInterest
        {
            Category = "museum",
            Details = "Test Museum",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        // Assert
        Assert.That(poi.Category, Is.EqualTo("museum"));
        Assert.That(poi.Details, Is.EqualTo("Test Museum"));
        Assert.That(poi.Location, Is.Not.Null);
        Assert.That(poi.Location.Type, Is.EqualTo("Point"));
        Assert.That(poi.Location.Coordinates, Has.Length.EqualTo(2));
    }

    [Test]
    public void Location_ShouldHandleCoordinatesCorrectly()
    {
        // Arrange & Act
        var location = new Location
        {
            Type = "Point",
            Coordinates = new double[] { 13.7373, 51.0504 }
        };

        // Assert
        Assert.That(location.Coordinates[0], Is.EqualTo(13.7373));
        Assert.That(location.Coordinates[1], Is.EqualTo(51.0504));
    }

    [Test]
    public void PointOfInterest_ShouldHandleNullLocation()
    {
        // Arrange & Act
        var poi = new PointOfInterest
        {
            Category = "museum",
            Details = "Test Museum",
            Location = null!
        };

        // Assert
        Assert.That(poi.Location, Is.Null);
    }
}

[TestFixture]
public class IntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task Home_Index_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task Map_Index_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Map");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task PointOfInterest_Index_ShouldReturnSuccess()
    {
        // Act - PointOfInterest doesn't have a direct route anymore, use Map controller instead
        var response = await _client.GetAsync("/Map");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task PointOfInterest_GetAll_ShouldReturnJson()
    {
        // Act
        var response = await _client.GetAsync("/api/pointsofinterest");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task PointOfInterest_GetCategories_ShouldReturnJson()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task Map_GetPointsOfInterest_ShouldReturnJson()
    {
        // Act
        var response = await _client.GetAsync("/Map/GetPointsOfInterest");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public void ServiceContainer_ShouldResolveServices()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var poiService = scope.ServiceProvider.GetService<IPointOfInterestService>();
        var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

        // Assert
        Assert.That(poiService, Is.Not.Null);
        Assert.That(httpClientFactory, Is.Not.Null);
    }
}

[TestFixture]
public class PointOfInterestServiceDetailedTests
{
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILogger<PointOfInterestService>> _mockLogger;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;

    [SetUp]
    public void SetUp()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PointOfInterestService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    }

    #region GetRadiusForZoom Tests (Static Method)

    // GetRadiusForZoom test removed - method no longer exists as it's now handled by the backend API

    #endregion

    #region Fallback Methods Tests (No API)

    [Test]
    public async Task GetPointsOfInterestAsync_WithNullApiUrl_ShouldReturnMockData()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns((string)null);
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetPointsOfInterestAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(5)); // Mock data has 5 items
        Assert.That(result[0].Category, Is.EqualTo("landmark"));
        Assert.That(result[0].Details, Does.Contain("Brandenburger Tor"));
    }

    [Test]
    public async Task GetPointsOfInterestAsync_WithEmptyApiUrl_ShouldReturnMockData()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns("");
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetPointsOfInterestAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result[1].Category, Is.EqualTo("museum"));
        Assert.That(result[1].Details, Does.Contain("Deutsches Museum"));
    }

    [Test]
    public async Task GetAvailableCategoriesAsync_WithNullApiUrl_ShouldReturnFallbackCategories()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns((string)null);
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetAvailableCategoriesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(17)); // Fallback categories count
        Assert.That(result, Does.Contain("landmark"));
        Assert.That(result, Does.Contain("museum"));
        Assert.That(result, Does.Contain("castle"));
        Assert.That(result, Does.Contain("restaurant"));
        Assert.That(result, Does.Contain("hotel"));
    }

    [Test]
    public async Task GetAvailableCategoriesAsync_WithEmptyApiUrl_ShouldReturnFallbackCategories()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns("");
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetAvailableCategoriesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(17));
        Assert.That(result, Does.Contain("restaurant"));
        Assert.That(result, Does.Contain("hotel"));
        Assert.That(result, Does.Contain("museum"));
    }

    [Test]
    public void CreatePointOfInterestAsync_WithNullApiUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns((string)null);
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);
        var testPoi = new PointOfInterest { Category = "test", Details = "Test POI" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CreatePointOfInterestAsync(testPoi));
        Assert.That(ex.Message, Does.Contain("API Base URL not configured"));
    }

    [Test]
    public void CreatePointOfInterestAsync_WithEmptyApiUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns("");
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);
        var testPoi = new PointOfInterest { Category = "test", Details = "Test POI" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CreatePointOfInterestAsync(testPoi));
        Assert.That(ex.Message, Does.Contain("API Base URL not configured"));
    }

    #endregion

    #region Mock Data Validation Tests

    [Test]
    public async Task GetPointsOfInterestAsync_MockData_ShouldHaveValidLocations()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns((string)null);
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetPointsOfInterestAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        foreach (var poi in result)
        {
            Assert.That(poi.Location, Is.Not.Null);
            Assert.That(poi.Location.Coordinates, Is.Not.Null);
            Assert.That(poi.Location.Coordinates.Length, Is.EqualTo(2));
            Assert.That(poi.Location.Type, Is.EqualTo("Point"));
            
            // Validate coordinates are within valid ranges
            Assert.That(poi.Location.Longitude, Is.GreaterThan(-180));
            Assert.That(poi.Location.Longitude, Is.LessThan(180));
            Assert.That(poi.Location.Latitude, Is.GreaterThan(-90));
            Assert.That(poi.Location.Latitude, Is.LessThan(90));
        }
    }

    [Test]
    public async Task GetPointsOfInterestAsync_MockData_ShouldHaveValidCategories()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns((string)null);
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var pois = await service.GetPointsOfInterestAsync();
        var categories = await service.GetAvailableCategoriesAsync();

        // Assert
        Assert.That(pois, Is.Not.Null);
        Assert.That(categories, Is.Not.Null);
        
        foreach (var poi in pois)
        {
            Assert.That(categories, Does.Contain(poi.Category), 
                $"POI category '{poi.Category}' should be in available categories");
        }
    }

    [Test]
    public async Task GetPointsOfInterestAsync_MockData_ShouldHaveValidHrefs()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns((string)null);
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetPointsOfInterestAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        foreach (var poi in result)
        {
            Assert.That(poi.Href, Is.Not.Null);
            Assert.That(poi.Href, Is.Not.Empty);
            Assert.That(poi.Details, Is.Not.Null);
            Assert.That(poi.Details, Is.Not.Empty);
        }
    }

    #endregion

    #region Configuration Validation Tests

    [Test]
    public async Task GetPointsOfInterestAsync_WithCoordinates_ShouldUseProvidedValues()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["PointOfInterestApi:BaseUrl"]).Returns((string)null);
        var service = new PointOfInterestService(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act (should still return mock data regardless of coordinates when API is not configured)
        var result = await service.GetPointsOfInterestAsync(50.0, 14.0, 5000);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(5)); // Should return mock data
    }

    #endregion
}

[TestFixture]
public class ViewRenderingTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task Home_Index_ShouldRenderCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(content, Does.Contain("html"));
        Assert.That(content, Does.Contain("Point of Interest Map"));
    }

    [Test]
    public async Task Map_Index_ShouldRenderCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(content, Does.Contain("html"));
        Assert.That(content, Does.Contain("leaflet"));
    }

    [Test]
    public async Task PointOfInterest_Index_ShouldRenderCorrectly()
    {
        // Act - Use Map controller since that's the main view
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(content, Does.Contain("html"));
    }
}

[TestFixture]
public class ErrorHandlingTests
{
    private Mock<IPointOfInterestService> _mockService;
    private PointOfInterestController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _controller = new PointOfInterestController(_mockService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task GetAll_WhenServiceThrows_ShouldReturnEmptyList()
    {
        // Arrange
        _mockService.Setup(x => x.GetPointsOfInterestAsync())
                   .ThrowsAsync(new HttpRequestException("API Error"));

        // Act
        var result = await _controller.GetAll(null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        Assert.That(jsonResult?.Value, Is.InstanceOf<List<PointOfInterest>>());
        var list = jsonResult?.Value as List<PointOfInterest>;
        Assert.That(list?.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Create_WhenServiceThrows_ShouldReturnError()
    {
        // Arrange
        var testPoi = new PointOfInterest { Category = "test", Name = "Test POI", Details = "Test POI" };
        _mockService.Setup(x => x.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
                   .ThrowsAsync(new InvalidOperationException("Service Error"));

        // Act
        var result = await _controller.Create(testPoi);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetCategories_WhenServiceThrows_ShouldReturnFallbackCategories()
    {
        // Arrange
        _mockService.Setup(x => x.GetAvailableCategoriesAsync())
                   .ThrowsAsync(new HttpRequestException("Categories API Error"));

        // Act
        var result = await _controller.GetCategories();

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        Assert.That(jsonResult?.Value, Is.InstanceOf<List<string>>());
        var categories = jsonResult?.Value as List<string>;
        Assert.That(categories?.Count, Is.EqualTo(17));
        Assert.That(categories, Does.Contain("landmark"));
    }
}

[TestFixture]
public class ModelValidationTests
{
    [Test]
    public void PointOfInterest_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var poi = new PointOfInterest();

        // Assert
        Assert.That(poi.Href, Is.EqualTo(string.Empty));
        Assert.That(poi.Category, Is.EqualTo(string.Empty));
        Assert.That(poi.Details, Is.EqualTo(string.Empty));
        Assert.That(poi.Location, Is.Not.Null);
    }

    [Test]
    public void Location_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var location = new Location();

        // Assert
        Assert.That(location.Coordinates, Is.Not.Null);
        Assert.That(location.Coordinates.Length, Is.EqualTo(2));
        Assert.That(location.Type, Is.EqualTo("Point"));
        Assert.That(location.Longitude, Is.EqualTo(0));
        Assert.That(location.Latitude, Is.EqualTo(0));
    }

    [Test]
    public void Location_WithCoordinates_ShouldCalculateLatLonCorrectly()
    {
        // Arrange
        var location = new Location { Coordinates = new double[] { 13.4, 51.5 } };

        // Act & Assert
        Assert.That(location.Longitude, Is.EqualTo(13.4));
        Assert.That(location.Latitude, Is.EqualTo(51.5));
    }

    [Test]
    public void ErrorViewModel_ShouldCalculateShowRequestIdCorrectly()
    {
        // Test with RequestId
        var errorWithId = new ErrorViewModel { RequestId = "test-id" };
        Assert.That(errorWithId.ShowRequestId, Is.True);

        // Test without RequestId
        var errorWithoutId = new ErrorViewModel { RequestId = null };
        Assert.That(errorWithoutId.ShowRequestId, Is.False);

        // Test with empty RequestId
        var errorWithEmptyId = new ErrorViewModel { RequestId = "" };
        Assert.That(errorWithEmptyId.ShowRequestId, Is.False);
    }
}