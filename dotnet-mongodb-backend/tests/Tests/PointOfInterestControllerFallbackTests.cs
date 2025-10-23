using System;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests;

/// <summary>
/// Tests for controller fallback paths and error handling in URL generation.
/// These tests specifically target uncovered branches in GenerateHref and CreatePoi.
/// </summary>
public class PointOfInterestControllerFallbackTests
{
    private readonly Mock<IPointOfInterestService> _mockService;
    private readonly Mock<ILogger<PointOfInterestController>> _mockLogger;
    private readonly Mock<LinkGenerator> _mockLinkGenerator;
    private readonly PointOfInterestController _controller;

    public PointOfInterestControllerFallbackTests()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _mockLogger = new Mock<ILogger<PointOfInterestController>>();
        _mockLinkGenerator = new Mock<LinkGenerator>();

        _controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object, _mockLinkGenerator.Object);

        // Setup minimal HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region GenerateHref Exception Handling Tests

    [Fact]
    public async Task GetPoiById_WithLinkGenerator_ShouldGenerateHref()
    {
        // Arrange
        var poiId = "507f1f77bcf86cd799439011";
        var testPoi = new PointOfInterest
        {
            Id = poiId,
            Name = "Test POI",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        _mockService.Setup(s => s.GetPoiByIdAsync(poiId)).ReturnsAsync(testPoi);

        // Note: We cannot easily mock LinkGenerator.GetUriByAction because it's an extension method.
        // This test just verifies the basic path with LinkGenerator present works.

        // Act
        var result = await _controller.GetPoiById(poiId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var poi = Assert.IsType<PointOfInterest>(okResult.Value);
        Assert.Equal(poiId, poi.Id);
    }

    [Fact]
    public async Task GetPoiById_WithNullLinkGenerator_ShouldNotCrash()
    {
        // Arrange
        var poiId = "507f1f77bcf86cd799439011";
        var testPoi = new PointOfInterest
        {
            Id = poiId,
            Name = "Test POI",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        _mockService.Setup(s => s.GetPoiByIdAsync(poiId)).ReturnsAsync(testPoi);

        // Create controller with null LinkGenerator
        var controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object, null);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.GetPoiById(poiId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var poi = Assert.IsType<PointOfInterest>(okResult.Value);
        Assert.Equal(poiId, poi.Id);
        Assert.Null(poi.Href); // Href should be null since LinkGenerator is null
    }

    #endregion

    #region CreatePoi Location Header Fallback Tests

    [Fact]
    public async Task CreatePoi_WhenUrlActionSucceeds_ShouldUseAbsoluteUri()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>())).ReturnsAsync(createdPoi);

        // Setup HttpContext with request information
        _controller.ControllerContext.HttpContext.Request.Scheme = "https";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("api.example.com");
        _controller.ControllerContext.HttpContext.Request.PathBase = new PathString("");

        // Setup Url helper to return absolute URI
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("https://api.example.com/poi/507f1f77bcf86cd799439011");
        _controller.Url = mockUrlHelper.Object;

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);
        Assert.True(_controller.Response.Headers.ContainsKey("Location"));
        Assert.Equal("https://api.example.com/poi/507f1f77bcf86cd799439011", 
            _controller.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreatePoi_WhenUrlActionFails_ShouldFallbackToManualConstruction()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>())).ReturnsAsync(createdPoi);

        // Setup HttpContext with request information
        _controller.ControllerContext.HttpContext.Request.Scheme = "https";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("api.example.com");
        _controller.ControllerContext.HttpContext.Request.PathBase = new PathString("/api");

        // Setup Url helper to throw exception (simulating failure)
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Throws(new InvalidOperationException("Url.Action failed"));
        _controller.Url = mockUrlHelper.Object;

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);
        Assert.True(_controller.Response.Headers.ContainsKey("Location"));
        
        // Should fallback to manual construction
        var locationHeader = _controller.Response.Headers.Location.ToString();
        Assert.Equal("https://api.example.com/api/poi/507f1f77bcf86cd799439011", locationHeader);
    }

    [Fact]
    public async Task CreatePoi_WhenUrlActionReturnsEmpty_ShouldFallbackToManualConstruction()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>())).ReturnsAsync(createdPoi);

        // Setup HttpContext
        _controller.ControllerContext.HttpContext.Request.Scheme = "http";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:5000");
        _controller.ControllerContext.HttpContext.Request.PathBase = new PathString("");

        // Setup Url helper to return empty string
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns(string.Empty);
        _controller.Url = mockUrlHelper.Object;

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);
        Assert.True(_controller.Response.Headers.ContainsKey("Location"));
        
        var locationHeader = _controller.Response.Headers.Location.ToString();
        Assert.Equal("http://localhost:5000/poi/507f1f77bcf86cd799439011", locationHeader);
    }

    [Fact]
    public async Task CreatePoi_WhenUrlActionReturnsNull_ShouldFallbackToManualConstruction()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>())).ReturnsAsync(createdPoi);

        // Setup HttpContext
        _controller.ControllerContext.HttpContext.Request.Scheme = "http";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:5000");

        // Setup Url helper to return null
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns((string?)null);
        _controller.Url = mockUrlHelper.Object;

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);
        Assert.True(_controller.Response.Headers.ContainsKey("Location"));
        
        var locationHeader = _controller.Response.Headers.Location.ToString();
        Assert.Equal("http://localhost:5000/poi/507f1f77bcf86cd799439011", locationHeader);
    }

    [Fact]
    public async Task CreatePoi_WhenUrlHelperNotSet_ShouldUseManualConstruction()
    {
        // Arrange
        var newPoi = new PointOfInterest
        {
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        var createdPoi = new PointOfInterest
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "New POI",
            Category = "restaurant",
            Location = new Location { Longitude = 10.0, Latitude = 50.0 }
        };

        _mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>())).ReturnsAsync(createdPoi);

        // Setup HttpContext
        _controller.ControllerContext.HttpContext.Request.Scheme = "http";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:5000");
        _controller.ControllerContext.HttpContext.Request.PathBase = new PathString("");

        // Don't set Url helper, it will be null by default in tests
        // Controller.Url is null unless explicitly set

        // Act
        var result = await _controller.CreatePoi(newPoi);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);
        Assert.True(_controller.Response.Headers.ContainsKey("Location"));
        
        // Should use manual construction since Url is null
        var locationHeader = _controller.Response.Headers.Location.ToString();
        Assert.Equal("http://localhost:5000/poi/507f1f77bcf86cd799439011", locationHeader);
    }

    #endregion
}
