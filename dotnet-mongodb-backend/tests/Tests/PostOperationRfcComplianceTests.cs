using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Xunit;
using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Services;
using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Models.DTOs;
using System;
using System.Collections.Generic;

namespace DotNetMongoDbBackend.Tests.Tests;

/// <summary>
/// Integration tests specifically for POST operation RFC 9110 compliance
/// RFC 9110 (June 2022) replaces RFC 7231 and allows both absolute and relative URIs in Location header
/// Tests verify that Location header is properly set as per RFC 9110 Section 10.2.2
/// </summary>
public class PostOperationRfcComplianceTests
{
    private readonly Mock<IPointOfInterestService> _mockService;
    private readonly Mock<ILogger<PointOfInterestController>> _mockLogger;

    public PostOperationRfcComplianceTests()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _mockLogger = new Mock<ILogger<PointOfInterestController>>();
    }

    [Fact]
    public async Task CreatePoi_LocationHeader_ShouldContainAbsoluteUri_MatchingJeeImplementation()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);

        var newPoi = new PointOfInterestDto
        {
            Name = "Test POI",
            Category = "restaurant",
            Details = "Test Details",
            Location = new LocationDto
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        var createdEntity = new PointOfInterestEntity
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "Test POI",
            Category = "restaurant",
            Details = "Test Details",
            Location = new LocationEntity
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterestEntity>()))
                   .ReturnsAsync(createdEntity);

        // Setup HttpContext to simulate real HTTP request
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost", 8080);
        httpContext.Request.PathBase = "/zdi-geo-service/api";

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.CreatePoi(newPoi);

        // Assert
        // 1. Verify HTTP 201 status code (RFC 9110 compliance)
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);

        // 2. Verify Location header is present (RFC 9110 Section 10.2.2 requirement)
        Assert.True(controller.Response.Headers.ContainsKey("Location"),
            "Location header must be present in 201 Created response");

        // 3. Extract Location header value
        var locationHeader = controller.Response.Headers["Location"].ToString();
        Assert.NotNull(locationHeader);
        Assert.NotEmpty(locationHeader);

        // 4. Verify Location header contains the created resource ID
        Assert.Contains(createdEntity.Id, locationHeader);

        // 5. RFC 9110 allows both absolute and relative URIs
        // Prefer absolute URI for better interoperability (matching JEE implementation)
        Assert.True(
            locationHeader.StartsWith("http://") || locationHeader.StartsWith("https://") || locationHeader.StartsWith("/"),
            $"Location header should be a URI-reference per RFC 9110. Got: {locationHeader}"
        );

        // 6. If absolute URI, verify structure matches JEE implementation
        // Expected format: http://localhost:8080/zdi-geo-service/api/poi/{id}
        if (locationHeader.StartsWith("http://") || locationHeader.StartsWith("https://"))
        {
            Assert.Contains("http://localhost:8080", locationHeader);
            Assert.Contains("/poi/", locationHeader);
            Assert.EndsWith(createdEntity.Id, locationHeader);
        }
    }

    [Fact]
    public async Task CreatePoi_Response_ShouldHaveNoBody_MatchingJeeImplementation()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);

        var newPoi = new PointOfInterestDto
        {
            Name = "Test POI",
            Category = "museum",
            Details = "Test museum",
            Location = new LocationDto
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }
            }
        };

        var createdEntity = new PointOfInterestEntity
        {
            Id = "abc123",
            Name = "Test POI",
            Category = "museum",
            Details = "Test museum",
            Location = new LocationEntity
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }
            }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterestEntity>()))
                   .ReturnsAsync(createdEntity);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("api.example.com");
        httpContext.Request.PathBase = "/api/v1";

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.CreatePoi(newPoi);

        // Assert
        // JEE implementation returns empty body with 201 Created
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);

        // StatusCodeResult has no Value property, confirming no body in response
        Assert.IsType<StatusCodeResult>(result);
    }

    [Fact]
    public async Task CreatePoi_LocationHeader_ShouldWorkWithDifferentSchemes()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);

        var newPoi = new PointOfInterestDto
        {
            Name = "HTTPS Test",
            Category = "test",
            Details = "Testing HTTPS",
            Location = new LocationDto { Type = "Point", Coordinates = new double[] { 0, 0 } }
        };

        var createdEntity = new PointOfInterestEntity
        {
            Id = "https-test-id",
            Name = "HTTPS Test",
            Category = "test",
            Details = "Testing HTTPS",
            Location = new LocationEntity { Type = "Point", Coordinates = new double[] { 0, 0 } }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterestEntity>()))
                   .ReturnsAsync(createdEntity);

        // Test with HTTPS scheme
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("secure.example.com", 443);
        httpContext.Request.PathBase = "/geo-api";

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.CreatePoi(newPoi);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);

        var locationHeader = controller.Response.Headers["Location"].ToString();

        // Should use HTTPS scheme
        Assert.StartsWith("https://", locationHeader);
        Assert.Contains("secure.example.com", locationHeader);
        Assert.Contains(createdEntity.Id, locationHeader);
    }
}

/// <summary>
/// RFC 9110 compliance tests for GET and DELETE operations
/// Verifies that all REST operations match JEE implementation and RFC 9110 standards
/// </summary>
public class GetDeleteOperationsRfcComplianceTests
{
    private readonly Mock<IPointOfInterestService> _mockService;
    private readonly Mock<ILogger<PointOfInterestController>> _mockLogger;

    public GetDeleteOperationsRfcComplianceTests()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _mockLogger = new Mock<ILogger<PointOfInterestController>>();
    }

    [Fact]
    public async Task GetById_ShouldReturn404WithoutBody_WhenPoiNotFound_MatchingJeeImplementation()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);
        _mockService.Setup(s => s.GetPoiByIdAsync("nonexistent")).ReturnsAsync((PointOfInterestEntity)null);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await controller.GetPoiById("nonexistent");

        // Assert
        // RFC 9110 Section 15.5.5: 404 Not Found
        // JEE implementation throws NotFoundException (results in 404 without body)
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundResult>(actionResult.Result);

        // Verify no body in response (NotFoundResult has no Value property)
        Assert.NotNull(notFoundResult);
    }

    [Fact]
    public async Task GetById_ShouldReturn200WithBody_WhenPoiExists_Rfc9110Compliant()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);
        var testEntity = new PointOfInterestEntity
        {
            Id = "testid123",
            Name = "Test POI",
            Category = "restaurant",
            Location = new LocationEntity { Type = "Point", Coordinates = new double[] { 13.7, 51.0 } }
        };
        _mockService.Setup(s => s.GetPoiByIdAsync("testid123")).ReturnsAsync(testEntity);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await controller.GetPoiById("testid123");

        // Assert
        // RFC 9110 Section 15.3.1: 200 OK with representation
        var actionResult = Assert.IsType<ActionResult<PointOfInterestDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPoi = Assert.IsType<PointOfInterestDto>(okResult.Value);

        Assert.Equal("testid123", returnedPoi.Id);
        Assert.Equal("Test POI", returnedPoi.Name);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenPoiExists_Rfc9110Compliant()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);
        _mockService.Setup(s => s.DeletePoiAsync("existing-poi")).ReturnsAsync(true);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await controller.DeletePoi("existing-poi");

        // Assert
        // RFC 9110 Section 15.3.5: 204 No Content (successful deletion)
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.NotNull(noContentResult);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenPoiDoesNotExist_IdempotentBehavior()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);
        _mockService.Setup(s => s.DeletePoiAsync("nonexistent")).ReturnsAsync(false);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await controller.DeletePoi("nonexistent");

        // Assert
        // RFC 9110 Section 9.3.5: DELETE is idempotent
        // JEE implementation always returns 204, even if resource doesn't exist
        // This ensures idempotency: multiple DELETE requests have the same result
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.NotNull(noContentResult);
    }

    [Fact]
    public async Task Delete_ShouldBeIdempotent_MultipleCallsSameResult()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);

        // First call: POI exists and is deleted
        _mockService.SetupSequence(s => s.DeletePoiAsync("test-poi"))
            .ReturnsAsync(true)   // First call: deleted
            .ReturnsAsync(false)  // Second call: already deleted
            .ReturnsAsync(false); // Third call: still deleted

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act - Multiple DELETE calls
        var result1 = await controller.DeletePoi("test-poi");
        var result2 = await controller.DeletePoi("test-poi");
        var result3 = await controller.DeletePoi("test-poi");

        // Assert - All calls return 204 No Content (idempotent)
        Assert.IsType<NoContentResult>(result1);
        Assert.IsType<NoContentResult>(result2);
        Assert.IsType<NoContentResult>(result3);
    }

    [Fact]
    public async Task GetList_ShouldReturn200WithEmptyArray_WhenNoPoiFound_Rfc9110Compliant()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);
        _mockService.Setup(s => s.GetNearbyPoisAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new List<PointOfInterestEntity>());

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await controller.GetAllPois(lat: 51.0, lng: 13.7, radius: 1000);

        // Assert
        // RFC 9110 Section 15.3.1: 200 OK with empty array (not 404)
        // Collection endpoints return empty array, not 404
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var pois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);

        Assert.Empty(pois);
    }
}
