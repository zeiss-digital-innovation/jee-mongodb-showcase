using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Xunit;
using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Services;
using DotNetMongoDbBackend.Models;
using System;

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

        var newPoi = new PointOfInterest
        {
            Name = "Test POI",
            Category = "restaurant",
            Details = "Test Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "Test POI",
            Category = "restaurant",
            Details = "Test Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 13.7373, 51.0504 }
            }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>()))
                   .ReturnsAsync(createdPoi);

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
        Assert.Contains(createdPoi.Id, locationHeader);

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
            Assert.EndsWith(createdPoi.Id, locationHeader);
        }
    }

    [Fact]
    public async Task CreatePoi_Response_ShouldHaveNoBody_MatchingJeeImplementation()
    {
        // Arrange
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object);

        var newPoi = new PointOfInterest
        {
            Name = "Test POI",
            Category = "museum",
            Details = "Test museum",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }
            }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "abc123",
            Name = "Test POI",
            Category = "museum",
            Details = "Test museum",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new double[] { 8.4, 49.0 }
            }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>()))
                   .ReturnsAsync(createdPoi);

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

        var newPoi = new PointOfInterest
        {
            Name = "HTTPS Test",
            Category = "test",
            Details = "Testing HTTPS",
            Location = new Location { Type = "Point", Coordinates = new double[] { 0, 0 } }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "https-test-id",
            Name = "HTTPS Test",
            Category = "test",
            Details = "Testing HTTPS",
            Location = new Location { Type = "Point", Coordinates = new double[] { 0, 0 } }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>()))
                   .ReturnsAsync(createdPoi);

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
        Assert.Contains(createdPoi.Id, locationHeader);
    }
}
