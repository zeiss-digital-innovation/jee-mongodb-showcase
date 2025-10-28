#nullable disable

using Moq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using DotNetMapsFrontend.Services;
using DotNetMapsFrontend.Controllers;
using DotNetMapsFrontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Tests for the backend-based category filtering functionality
/// </summary>
[TestFixture]
public class PointOfInterestCategoryFilterTests
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
    public async Task GetAll_WithoutCategories_ShouldPassEmptyListToService()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var expectedPois = GetTestPointsOfInterest();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, null);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, radius, 
            It.Is<List<string>>(list => list.Count == 0)), Times.Once);
    }

    [Test]
    public async Task GetAll_WithSingleCategory_ShouldPassCategoryToService()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum" };
        var expectedPois = GetTestPointsOfInterest().Where(p => p.Category == "museum").ToList();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var pois = jsonResult?.Value as List<PointOfInterest>;
        
        Assert.That(pois, Is.Not.Null);
        Assert.That(pois.Count, Is.EqualTo(2));
        Assert.That(pois.All(p => p.Category == "museum"), Is.True);
        
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, radius, 
            It.Is<List<string>>(list => list.Count == 1 && list.Contains("museum"))), Times.Once);
    }

    [Test]
    public async Task GetAll_WithMultipleCategories_ShouldPassAllCategoriesToService()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum", "castle", "restaurant" };
        var expectedPois = GetTestPointsOfInterest()
            .Where(p => categories.Contains(p.Category))
            .ToList();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var pois = jsonResult?.Value as List<PointOfInterest>;
        
        Assert.That(pois, Is.Not.Null);
        Assert.That(pois.Count, Is.EqualTo(5));
        Assert.That(pois.All(p => categories.Contains(p.Category)), Is.True);
        
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, radius, 
            It.Is<List<string>>(list => list.Count == 3 && 
                list.Contains("museum") && 
                list.Contains("castle") && 
                list.Contains("restaurant"))), Times.Once);
    }

    [Test]
    public async Task GetAll_WithCategories_ShouldReturnOnlyMatchingCategories()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "park" };
        var expectedPois = GetTestPointsOfInterest()
            .Where(p => p.Category == "park")
            .ToList();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var pois = jsonResult?.Value as List<PointOfInterest>;
        
        Assert.That(pois, Is.Not.Null);
        Assert.That(pois.Count, Is.EqualTo(1));
        Assert.That(pois[0].Category, Is.EqualTo("park"));
    }

    [Test]
    public async Task GetAll_WithEmptyCategories_ShouldReturnAllPois()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string>();
        var expectedPois = GetTestPointsOfInterest();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var pois = jsonResult?.Value as List<PointOfInterest>;
        
        Assert.That(pois, Is.Not.Null);
        Assert.That(pois.Count, Is.EqualTo(6));
        
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, radius, 
            It.Is<List<string>>(list => list.Count == 0)), Times.Once);
    }

    [Test]
    public async Task GetAll_WithNonExistingCategory_ShouldReturnEmptyList()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "nonexistent" };
        var expectedPois = new List<PointOfInterest>();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var pois = jsonResult?.Value as List<PointOfInterest>;
        
        Assert.That(pois, Is.Not.Null);
        Assert.That(pois.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAll_WithMixedCaseCategories_ShouldPassExactCaseToService()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "Museum", "CASTLE" };
        var expectedPois = new List<PointOfInterest>();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert - Service should receive categories in exact case as provided
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, radius, 
            It.Is<List<string>>(list => list.Count == 2 && 
                list.Contains("Museum") && 
                list.Contains("CASTLE"))), Times.Once);
    }

    [Test]
    public async Task GetAll_WithDuplicateCategories_ShouldPassAllToService()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum", "museum", "castle" };
        var expectedPois = GetTestPointsOfInterest()
            .Where(p => p.Category == "museum" || p.Category == "castle")
            .ToList();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, radius, 
            It.Is<List<string>>(list => list.Count == 3)), Times.Once);
    }

    [Test]
    public async Task GetAll_WithInvalidCoordinates_ShouldUseFallbackAndReturnJson()
    {
        // Arrange
        double? lat = null;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum" };
        var fallbackPois = GetTestPointsOfInterest();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync())
            .ReturnsAsync(fallbackPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        // When coordinates are invalid, controller uses default coordinates (fallback)
        _mockService.Verify(s => s.GetPointsOfInterestAsync(), Times.Once);
    }

    [Test]
    public async Task GetAll_WithCategoriesAndInvalidRadius_ShouldUseDefaultRadius()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int? radius = null;
        var categories = new List<string> { "museum" };
        var expectedPois = GetTestPointsOfInterest()
            .Where(p => p.Category == "museum")
            .ToList();
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, It.IsAny<int>(), It.IsAny<List<string>>()))
            .ReturnsAsync(expectedPois);

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        // When radius is null, controller uses default radius of 2000m
        _mockService.Verify(s => s.GetPointsOfInterestAsync(lat, lon, 2000, 
            It.Is<List<string>>(list => list.Count == 1 && list.Contains("museum"))), Times.Once);
    }

    [Test]
    public async Task GetAll_ServiceThrowsException_ShouldReturnEmptyJsonList()
    {
        // Arrange
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 2000;
        var categories = new List<string> { "museum" };
        
        _mockService.Setup(s => s.GetPointsOfInterestAsync(lat, lon, radius, It.IsAny<List<string>>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetAll(lat, lon, radius, categories);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var pois = jsonResult?.Value as List<PointOfInterest>;
        
        // Controller returns empty list on exception
        Assert.That(pois, Is.Not.Null);
        Assert.That(pois.Count, Is.EqualTo(0));
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
