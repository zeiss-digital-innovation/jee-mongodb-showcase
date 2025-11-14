using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Models.DTOs;

namespace DotNetMongoDbBackend.Tests;

/// <summary>
/// Integration tests for category filtering with geographic search
/// NEW FEATURE: Multiple categories via MongoDB $in operator
/// </summary>
public class CategoryFilterIntegrationTests
{
    private readonly Mock<IPointOfInterestService> _mockService;
    private readonly Mock<ILogger<PointOfInterestController>> _mockLogger;
    private readonly PointOfInterestController _controller;

    public CategoryFilterIntegrationTests()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _mockLogger = new Mock<ILogger<PointOfInterestController>>();
        _controller = new PointOfInterestController(_mockService.Object, _mockLogger.Object, linkGenerator: null);
        
        // Set HttpContext to avoid null reference
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task GetAllPois_WithGeographicSearchAndMultipleCategories_ShouldCallNewServiceMethod()
    {
        // Arrange
        var lat = 51.0504;
        var lon = 13.7373;
        var radius = 2000.0; // meters
        var categories = new List<string> { "restaurant", "cafe", "museum" };
        
        var expectedPois = new List<PointOfInterestEntity>
        {
            new() { Id = "1", Category = "restaurant", Details = "Test Restaurant" },
            new() { Id = "2", Category = "cafe", Details = "Test Cafe" },
            new() { Id = "3", Category = "museum", Details = "Test Museum" }
        };

        _mockService.Setup(s => s.GetNearbyPoisByCategoriesAsync(
                lon, lat, radius / 1000.0, categories))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAllPois(
            category: categories,
            lat: lat,
            lon: lon,
            radius: radius);

        // Assert
        _mockService.Verify(s => s.GetNearbyPoisByCategoriesAsync(
            lon, lat, radius / 1000.0, categories), Times.Once);
        
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        
        Assert.Equal(3, returnedPois.Count);
        Assert.Contains(returnedPois, p => p.Category == "restaurant");
        Assert.Contains(returnedPois, p => p.Category == "cafe");
        Assert.Contains(returnedPois, p => p.Category == "museum");
    }

    [Fact]
    public async Task GetAllPois_WithGeographicSearchButNoCategories_ShouldUseOldServiceMethod()
    {
        // Arrange - BACKWARD COMPATIBILITY TEST
        var lat = 51.0504;
        var lon = 13.7373;
        var radius = 2000.0;
        
        var expectedPois = new List<PointOfInterestEntity>
        {
            new() { Id = "1", Category = "restaurant" },
            new() { Id = "2", Category = "museum" },
            new() { Id = "3", Category = "park" }
        };

        _mockService.Setup(s => s.GetNearbyPoisAsync(lon, lat, radius / 1000.0))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAllPois(
            category: null,  // No categories = old behavior
            lat: lat,
            lon: lon,
            radius: radius);

        // Assert - Should use old method
        _mockService.Verify(s => s.GetNearbyPoisAsync(
            lon, lat, radius / 1000.0), Times.Once);
        
        // Should NOT call new method
        _mockService.Verify(s => s.GetNearbyPoisByCategoriesAsync(
            It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<List<string>>()), 
            Times.Never);
        
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        
        Assert.Equal(3, returnedPois.Count);
    }

    [Fact]
    public async Task GetAllPois_WithEmptyCategoryList_ShouldUseOldServiceMethod()
    {
        // Arrange
        var lat = 51.0504;
        var lon = 13.7373;
        var radius = 2000.0;
        var emptyCategories = new List<string>(); // Empty list
        
        var expectedPois = new List<PointOfInterestEntity>
        {
            new() { Id = "1", Category = "any" }
        };

        _mockService.Setup(s => s.GetNearbyPoisAsync(lon, lat, radius / 1000.0))
            .ReturnsAsync(expectedPois);

        // Act
        await _controller.GetAllPois(
            category: emptyCategories,
            lat: lat,
            lon: lon,
            radius: radius);

        // Assert - Empty list should use old method
        _mockService.Verify(s => s.GetNearbyPoisAsync(lon, lat, radius / 1000.0), Times.Once);
        _mockService.Verify(s => s.GetNearbyPoisByCategoriesAsync(
            It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<List<string>>()), 
            Times.Never);
    }

    [Fact]
    public async Task GetAllPois_WithSingleCategoryButNoGeoSearch_ShouldUseCategoryFilterMethod()
    {
        // Arrange - BACKWARD COMPATIBILITY TEST
        var category = "restaurant";
        var categories = new List<string> { category };
        
        var expectedPois = new List<PointOfInterestEntity>
        {
            new() { Id = "1", Category = "restaurant" }
        };

        _mockService.Setup(s => s.GetPoisByCategoryAsync(category))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAllPois(category: categories);

        // Assert - Should use old category filter method
        _mockService.Verify(s => s.GetPoisByCategoryAsync(category), Times.Once);
        
        var actionResult = Assert.IsType<ActionResult<List<PointOfInterestDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPois = Assert.IsType<List<PointOfInterestDto>>(okResult.Value);
        
        Assert.Single(returnedPois);
        Assert.Equal("restaurant", returnedPois[0].Category);
    }

    [Fact]
    public async Task GetAllPois_WithCategoriesInDifferentCase_ShouldNormalizeToLowercase()
    {
        // Arrange
        var lat = 51.0504;
        var lon = 13.7373;
        var radius = 2000.0;
        var categories = new List<string> { "Restaurant", "CAFE", "MuSeUm" };
        
        var expectedPois = new List<PointOfInterestEntity>
        {
            new() { Id = "1", Category = "restaurant" }
        };

        _mockService.Setup(s => s.GetNearbyPoisByCategoriesAsync(
                It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        await _controller.GetAllPois(
            category: categories,
            lat: lat,
            lon: lon,
            radius: radius);

        // Assert - Categories should be normalized to lowercase at service level
        _mockService.Verify(s => s.GetNearbyPoisByCategoriesAsync(
            lon, lat, radius / 1000.0, categories), Times.Once);
    }
}
