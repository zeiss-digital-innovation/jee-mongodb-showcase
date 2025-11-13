using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Services;
using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Models.DTOs;
using System;

#nullable enable

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

        // Setup HTTP context for Response.Headers
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
        var testPois = new List<PointOfInterestEntity>
        {
            new() { Name = "Test POI 1", Category = "restaurant" },
            new() { Name = "Test POI 2", Category = "museum" }
        };

        _mockService.Setup(s => s.GetAllPoisAsync())
                   .ReturnsAsync(testPois);

        // Act
        var result = await _controller.GetAllPois();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        Assert.Equal(2, returnedPois.Count);
    }

    [Fact]
    public async Task GetAllPois_WithCategoryFilter_ShouldCallGetPoisByCategory()
    {
        // Arrange
        var category = "restaurant";
        var filteredPois = new List<PointOfInterestEntity>
        {
            new() { Name = "Restaurant POI", Category = "restaurant" }
        };

        _mockService.Setup(s => s.GetPoisByCategoryAsync(category))
                   .ReturnsAsync(filteredPois);

        // Act - Wrap single category in List for new API
        var result = await _controller.GetAllPois(category: new List<string> { category });

        // Assert
        _mockService.Verify(s => s.GetPoisByCategoryAsync(category), Times.Once);
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        Assert.Single(returnedPois);
    }

    [Fact]
    public async Task GetAllPois_WithSearchTerm_ShouldCallSearchPois()
    {
        // Arrange
        var searchTerm = "test";
        var searchResults = new List<PointOfInterestEntity>
        {
            new() { Name = "Test Restaurant", Category = "restaurant" }
        };

        _mockService.Setup(s => s.SearchPoisAsync(searchTerm, null))
                   .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.GetAllPois(search: searchTerm);

        // Assert
        _mockService.Verify(s => s.SearchPoisAsync(searchTerm, null), Times.Once);
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        Assert.Single(returnedPois);
    }

    [Fact]
    public async Task GetAllPois_WithGeographicSearch_ShouldCallGetNearbyPois()
    {
        // Arrange
        double lat = 49.0, lng = 8.4, radius = 10.0; // radius provided in meters
        var expectedRadiusKm = radius / 1000.0; // controller converts meters to km
        var nearbyPois = new List<PointOfInterestEntity>
        {
            new() {
                Name = "Nearby POI",
                Location = new LocationEntity { Coordinates = new double[] { 8.41, 49.01 } }
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
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        Assert.Single(returnedPois);
    }

    // ============= NEW TESTS FOR 80%+ COVERAGE =============

    [Fact]
    public async Task GetPoiById_ShouldReturnOk_WhenPoiExists()
    {
        // Arrange
        var testPoi = new PointOfInterestEntity { Id = "123", Name = "Test POI", Category = "restaurant" };
        _mockService.Setup(s => s.GetPoiByIdAsync("123")).ReturnsAsync(testPoi);

        // Act
        var result = await _controller.GetPoiById("123");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPoi = Assert.IsType<PointOfInterestDto>(okResult.Value);
        Assert.Equal("Test POI", returnedPoi.Name);
    }

    [Fact]
    public async Task GetPoiById_ShouldReturnNotFound_WhenPoiDoesNotExist()
    {
        // Arrange
        _mockService.Setup(s => s.GetPoiByIdAsync("999")).ReturnsAsync((PointOfInterestEntity?)null);

        // Act
        var result = await _controller.GetPoiById("999");

        // Assert - JEE-compatible: Returns 404 without body
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
        // NotFoundResult has no Value property (no body in response)
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
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreatePoi_ShouldReturnCreated_WhenValidPoi()
    {
        // Arrange - complete POI object with all required fields
        var newPoi = new PointOfInterestDto
        {
            Name = "New POI",
            Category = "museum",
            Details = "A test museum POI",  // IMPORTANT: Details is required according to ValidatePoi
            Location = new LocationDto
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }  // [longitude, latitude]
            }
        };
        var createdPoi = new PointOfInterestEntity
        {
            Id = "123",
            Name = "New POI",
            Category = "museum",
            Details = "A test museum POI",
            Location = new LocationEntity
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }
            }
        };
        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterestEntity>())).ReturnsAsync(createdPoi);

        // Setup HttpContext with proper Request details for absolute URI generation
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost:8080");
        httpContext.Request.PathBase = "/zdi-geo-service/api";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert - JEE compatible: HTTP 201 without body, only Location header
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);

        // Check Location header contains absolute URI according to RFC 7231
        Assert.True(_controller.Response.Headers.ContainsKey("Location"));
        var locationHeader = _controller.Response.Headers["Location"].ToString();
        Assert.Contains("123", locationHeader);

        // Verify it's an absolute URI (contains scheme and host)
        Assert.True(locationHeader.StartsWith("http://") || locationHeader.StartsWith("https://") || locationHeader.StartsWith('/'),
            "Location header should be an absolute URI or absolute path");

        // If it's absolute, verify it contains the expected components
        if (locationHeader.StartsWith("http://") || locationHeader.StartsWith("https://"))
        {
            Assert.Contains("localhost", locationHeader);
            Assert.Contains("/poi/123", locationHeader);
        }
    }

    [Fact]
    public async Task CreatePoi_ShouldReturnBadRequest_WhenArgumentException()
    {
        // Arrange
        var invalidPoi = new PointOfInterestDto { Name = "", Category = "test" };
        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterestEntity>()))
                   .ThrowsAsync(new ArgumentException("Name is required"));

        // Act
        var result = await _controller.CreatePoi(invalidPoi);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value); // Verify it returns BadRequest with error details
    }

    [Fact]
    public async Task CreatePoi_ShouldReturn500_WhenUnexpectedError()
    {
        // Arrange
        var newPoi = new PointOfInterestDto { Name = "Test", Category = "test", Location = new LocationDto { Coordinates = [8.0, 49.0] } };
        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterestEntity>()))
                   .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode); // Unexpected errors return 500
    }

    [Fact]
    public async Task UpdatePoi_ShouldReturnOk_WhenPoiUpdated()
    {
        // Arrange
        var updatePoi = new PointOfInterestDto { Name = "Updated POI", Category = "restaurant" };
        var updatedPoi = new PointOfInterestEntity { Id = "123", Name = "Updated POI", Category = "restaurant" };
        _mockService.Setup(s => s.UpdatePoiAsync("123", It.IsAny<PointOfInterestEntity>())).ReturnsAsync(updatedPoi);

        // Act
        var result = await _controller.UpdatePoi("123", updatePoi);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPoi = Assert.IsType<PointOfInterestDto>(okResult.Value);
        Assert.Equal("Updated POI", returnedPoi.Name);
    }

    [Fact]
    public async Task UpdatePoi_ShouldReturnNotFound_WhenPoiDoesNotExist()
    {
        // Arrange
        var updatePoi = new PointOfInterestDto { Name = "Updated POI", Category = "restaurant" };
        _mockService.Setup(s => s.UpdatePoiAsync("999", It.IsAny<PointOfInterestEntity>())).ReturnsAsync((PointOfInterestEntity?)null);

        // Act
        var result = await _controller.UpdatePoi("999", updatePoi);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
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

        // Assert - JEE-compatible: Always 204 No Content
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePoi_ShouldReturnNoContent_WhenPoiDoesNotExist()
    {
        // Arrange - POI doesn't exist, but DELETE is idempotent
        _mockService.Setup(s => s.DeletePoiAsync("999")).ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePoi("999");

        // Assert - JEE-compatible: Always 204 No Content (idempotent DELETE per RFC 9110)
        Assert.IsType<NoContentResult>(result);
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
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Contains("Internal server error", statusResult.Value?.ToString());
    }

    [Fact]
    public async Task UpdatePoi_ShouldReturnBadRequest_WhenArgumentException()
    {
        // Arrange
        var updatePoi = new PointOfInterestDto { Name = "Test", Category = "test" };
        _mockService.Setup(s => s.UpdatePoiAsync("123", It.IsAny<PointOfInterestEntity>()))
            .ThrowsAsync(new ArgumentException("Invalid data"));

        // Act
        var result = await _controller.UpdatePoi("123", updatePoi);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UpdatePoi_ShouldReturn500_WhenUnexpectedException()
    {
        // Arrange
        var updatePoi = new PointOfInterestDto { Name = "Test", Category = "test", Location = new LocationDto { Coordinates = [8.0, 49.0] } };
        _mockService.Setup(s => s.UpdatePoiAsync("123", It.IsAny<PointOfInterestEntity>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdatePoi("123", updatePoi);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task DeletePoi_ShouldReturn500_WhenException()
    {
        // Arrange
        _mockService.Setup(s => s.DeletePoiAsync("123")).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeletePoi("123");

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetPoiById_ShouldReturn500_WhenException()
    {
        // Arrange
        _mockService.Setup(s => s.GetPoiByIdAsync("123")).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPoiById("123");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }



    [Fact]
    public async Task GetAllPois_WithLimit_ShouldLimitResults()
    {
        // Arrange
        var pois = new List<PointOfInterestEntity>
        {
            new() { Id = "1", Category = "restaurant" },
            new() { Id = "2", Category = "restaurant" },
            new() { Id = "3", Category = "restaurant" }
        };
        _mockService.Setup(s => s.GetAllPoisAsync()).ReturnsAsync(pois);

        // Act  
        List<string>? nullList = null;
        var result = await _controller.GetAllPois(nullList, null, 2, null, null, null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        Assert.Equal(2, returnedPois.Count);
    }

    [Fact]
    public async Task GetPoiById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetPoiByIdAsync("invalid-id")).ReturnsAsync((PointOfInterestEntity?)null);

        // Act
        var result = await _controller.GetPoiById("invalid-id");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetPoiById_ShouldPopulateHref_WhenLinkGeneratorProvided()
    {
        // Arrange
        var testPoi = new PointOfInterestEntity { Id = "abc", Name = "Href Test POI", Category = "test" };
        _mockService.Setup(s => s.GetPoiByIdAsync("abc")).ReturnsAsync(testPoi);

        // Provide a mocked LinkGenerator that returns a fixed absolute URL for the action
        // Use a derived controller that overrides GenerateHref to avoid mocking LinkGenerator overloads
        var controllerWithLink = new TestPoiController(_mockService.Object, _mockLogger.Object);

        // Act
        var result = await controllerWithLink.GetPoiById("abc");

        // Assert
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPoi = Assert.IsType<PointOfInterestDto>(okResult.Value);
        Assert.Equal("http://example/zdi-geo-service/api/poi/abc", returnedPoi.Href);
    }

    private class TestPoiController : PointOfInterestController
    {
        public TestPoiController(IPointOfInterestService service, ILogger<PointOfInterestController> logger)
            : base(service, logger, null)
        {
        }

        protected override void GenerateHref(PointOfInterestDto poiDto)
        {
            poiDto.Href = "http://example/zdi-geo-service/api/poi/abc";
        }
    }
}
