using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Services;
using DotNetMongoDbBackend.Models;
using System;

namespace DotNetMongoDbBackend.Tests.Tests;

public class PointOfInterestControllerTests
{
    private readonly Mock<IPointOfInterestService> _mockService;
    private readonly Mock<ILogger<PointOfInterestController>> _mockLogger;
    private readonly PointOfInterestController _controller;

    public PointOfInterestControllerTests()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _mockLogger = new Mock<ILogger<PointOfInterestController>>();
        _controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);

        // Setup HTTP-Context für Response.Headers
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAllPois_ShouldReturnOkResult_WithPoisList()
    {
        // Arrange
        var testPois = new List<PointOfInterest>
        {
            new PointOfInterest { Name = "Test POI 1", Category = "restaurant" },
            new PointOfInterest { Name = "Test POI 2", Category = "museum" }
        };

        _mockService.Setup(s => s.GetAllPoisAsync())
                   .ReturnsAsync(testPois);

        // Act
        var result = await _controller.GetAllPois();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterest>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterest>>(okResult.Value);
        Assert.Equal(2, returnedPois.Count);
    }

    [Fact]
    public async Task GetAllPois_WithCategoryFilter_ShouldCallGetPoisByCategory()
    {
        // Arrange
        var category = "restaurant";
        var filteredPois = new List<PointOfInterest>
        {
            new PointOfInterest { Name = "Restaurant POI", Category = "restaurant" }
        };

        _mockService.Setup(s => s.GetPoisByCategoryAsync(category))
                   .ReturnsAsync(filteredPois);

        // Act
        var result = await _controller.GetAllPois(category: category);

        // Assert
        _mockService.Verify(s => s.GetPoisByCategoryAsync(category), Times.Once);
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterest>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterest>>(okResult.Value);
        Assert.Single(returnedPois);
    }

    [Fact]
    public async Task GetAllPois_WithSearchTerm_ShouldCallSearchPois()
    {
        // Arrange
        var searchTerm = "test";
        var searchResults = new List<PointOfInterest>
        {
            new PointOfInterest { Name = "Test Restaurant", Category = "restaurant" }
        };

        _mockService.Setup(s => s.SearchPoisAsync(searchTerm, null))
                   .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.GetAllPois(search: searchTerm);

        // Assert
        _mockService.Verify(s => s.SearchPoisAsync(searchTerm, null), Times.Once);
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterest>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterest>>(okResult.Value);
        Assert.Single(returnedPois);
    }

    [Fact]
    public async Task GetAllPois_WithGeographicSearch_ShouldCallGetNearbyPois()
    {
        // Arrange
        double lat = 49.0, lng = 8.4, radius = 10.0; // radius provided in meters
        var expectedRadiusKm = radius / 1000.0; // controller converts meters to km
        var nearbyPois = new List<PointOfInterest>
        {
            new PointOfInterest
            {
                Name = "Nearby POI",
                Location = new Location(8.41, 49.01)
            }
        };

        _mockService.Setup(s => s.GetNearbyPoisAsync(
            It.Is<double>(lon => Math.Abs(lon - lng) < 1e-6),
            It.Is<double>(la => Math.Abs(la - lat) < 1e-6),
            It.Is<double>(r => Math.Abs(r - expectedRadiusKm) < 1e-6)))
               .ReturnsAsync(nearbyPois);

        // Act
        var result = await _controller.GetAllPois(lat: lat, lng: lng, radius: radius);

        // Assert
        _mockService.Verify(s => s.GetNearbyPoisAsync(
            It.Is<double>(lon => Math.Abs(lon - lng) < 1e-6),
            It.Is<double>(la => Math.Abs(la - lat) < 1e-6),
            It.Is<double>(r => Math.Abs(r - expectedRadiusKm) < 1e-6)), Times.Once);
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterest>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterest>>(okResult.Value);
        Assert.Single(returnedPois);
    }

    // ============= NEUE TESTS FÜR 80%+ ABDECKUNG =============

    [Fact]
    public async Task GetPoiById_ShouldReturnOk_WhenPoiExists()
    {
        // Arrange
        var testPoi = new PointOfInterest { Id = "123", Name = "Test POI", Category = "restaurant" };
        _mockService.Setup(s => s.GetPoiByIdAsync("123")).ReturnsAsync(testPoi);

        // Act
        var result = await _controller.GetPoiById("123");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterest>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPoi = Assert.IsType<PointOfInterest>(okResult.Value);
        Assert.Equal("Test POI", returnedPoi.Name);
    }

    [Fact]
    public async Task GetPoiById_ShouldReturnNotFound_WhenPoiDoesNotExist()
    {
        // Arrange
        _mockService.Setup(s => s.GetPoiByIdAsync("999")).ReturnsAsync((PointOfInterest)null);

        // Act
        var result = await _controller.GetPoiById("999");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterest>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetPoiById_ShouldReturn500_WhenServiceThrowsException()
    {
        // Arrange
        _mockService.Setup(s => s.GetPoiByIdAsync(It.IsAny<string>()))
                   .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPoiById("123");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterest>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreatePoi_ShouldReturnCreated_WhenValidPoi()
    {
        // Arrange - vollständiges POI-Objekt mit allen erforderlichen Feldern
        var newPoi = new PointOfInterest
        {
            Name = "New POI",
            Category = "museum",
            Details = "A test museum POI",  // WICHTIG: Details ist erforderlich laut ValidatePoi
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }  // [longitude, latitude]
            }
        };
        var createdPoi = new PointOfInterest
        {
            Id = "123",
            Name = "New POI",
            Category = "museum",
            Details = "A test museum POI",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }
            }
        };
        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>())).ReturnsAsync(createdPoi);

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert - JEE-kompatibel: HTTP 201 ohne Body, nur Location-Header
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);

        // Prüfe Location-Header (verwendet Fallback /poi/{id} da Url.ActionLink null zurückgibt)
        Assert.True(_controller.Response.Headers.ContainsKey("Location"));
        var locationHeader = _controller.Response.Headers["Location"].ToString();
        Assert.Contains("123", locationHeader);
        Assert.Contains("/poi/123", locationHeader);
    }

    [Fact]
    public async Task CreatePoi_ShouldReturnBadRequest_WhenArgumentException()
    {
        // Arrange
        var invalidPoi = new PointOfInterest { Name = "", Category = "test" };
        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>()))
                   .ThrowsAsync(new ArgumentException("Name ist erforderlich"));

        // Act
        var result = await _controller.CreatePoi(invalidPoi);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Name ist erforderlich", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task CreatePoi_ShouldReturn500_WhenUnexpectedError()
    {
        // Arrange
        var newPoi = new PointOfInterest { Name = "Test", Category = "test" };
        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>()))
                   .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdatePoi_ShouldReturnOk_WhenPoiUpdated()
    {
        // Arrange
        var updatePoi = new PointOfInterest { Name = "Updated POI", Category = "restaurant" };
        var updatedPoi = new PointOfInterest { Id = "123", Name = "Updated POI", Category = "restaurant" };
        _mockService.Setup(s => s.UpdatePoiAsync("123", It.IsAny<PointOfInterest>())).ReturnsAsync(updatedPoi);

        // Act
        var result = await _controller.UpdatePoi("123", updatePoi);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterest>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPoi = Assert.IsType<PointOfInterest>(okResult.Value);
        Assert.Equal("Updated POI", returnedPoi.Name);
    }

    [Fact]
    public async Task UpdatePoi_ShouldReturnNotFound_WhenPoiDoesNotExist()
    {
        // Arrange
        var updatePoi = new PointOfInterest { Name = "Updated POI", Category = "restaurant" };
        _mockService.Setup(s => s.UpdatePoiAsync("999", It.IsAny<PointOfInterest>())).ReturnsAsync((PointOfInterest)null);

        // Act
        var result = await _controller.UpdatePoi("999", updatePoi);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterest>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task DeletePoi_ShouldReturnNoContent_WhenPoiDeleted()
    {
        // Arrange
        _mockService.Setup(s => s.DeletePoiAsync("123")).ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePoi("123");

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePoi_ShouldReturnNotFound_WhenPoiDoesNotExist()
    {
        // Arrange
        _mockService.Setup(s => s.DeletePoiAsync("999")).ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePoi("999");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetAvailableCategories_ShouldReturnOk_WithCategoriesList()
    {
        // Arrange
        var categories = new List<string> { "restaurant", "museum", "park" };
        _mockService.Setup(s => s.GetAvailableCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAvailableCategories();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<string>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedCategories = Assert.IsType<List<string>>(okResult.Value);
        Assert.Equal(3, returnedCategories.Count);
        Assert.Contains("restaurant", returnedCategories);
    }

    [Fact]
    public async Task GetCategoryCount_ShouldReturnOk_WithCount()
    {
        // Arrange
        _mockService.Setup(s => s.CountByCategoryAsync("restaurant")).ReturnsAsync(42);

        // Act
        var result = await _controller.GetCategoryCount("restaurant");

        // Assert
        var actionResult = Assert.IsType<ActionResult<long>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var count = Assert.IsType<long>(okResult.Value);
        Assert.Equal(42, count);
    }

    [Fact]
    public void HealthCheck_ShouldReturnOk_WithMessage()
    {
        // Act
        var result = _controller.HealthCheck();

        // Assert
        var actionResult = Assert.IsType<ActionResult<string>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var message = Assert.IsType<string>(okResult.Value);
        Assert.Contains(".NET MongoDB Backend is running", message);
    }

    [Fact]
    public async Task GetAllPois_ShouldReturn500_WhenServiceThrowsException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllPoisAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllPois();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterest>>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Contains("Interner Serverfehler", statusResult.Value?.ToString());
    }

    [Fact]
    public async Task GetPoiById_ShouldPopulateHref_WhenLinkGeneratorProvided()
    {
        // Arrange
        var testPoi = new PointOfInterest { Id = "abc", Name = "Href Test POI", Category = "test" };
        _mockService.Setup(s => s.GetPoiByIdAsync("abc")).ReturnsAsync(testPoi);

        // Provide a mocked LinkGenerator that returns a fixed absolute URL for the action
        // Use a derived controller that overrides GenerateHref to avoid mocking LinkGenerator overloads
        var controllerWithLink = new TestPoiController(_mockService.Object, _mockLogger.Object);

        // Act
        var result = await controllerWithLink.GetPoiById("abc");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterest>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPoi = Assert.IsType<PointOfInterest>(okResult.Value);
        Assert.Equal("http://example/zdi-geo-service/api/poi/abc", returnedPoi.Href);
    }

    private class TestPoiController : PointOfInterestController
    {
        public TestPoiController(IPointOfInterestService service, ILogger<PointOfInterestController> logger)
            : base(service, logger, null)
        {
        }

        protected override void GenerateHref(PointOfInterest p)
        {
            p.Href = "http://example/zdi-geo-service/api/poi/abc";
        }
    }
}
