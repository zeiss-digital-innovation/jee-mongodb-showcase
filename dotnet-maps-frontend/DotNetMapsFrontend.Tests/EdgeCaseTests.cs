#nullable disable

using DotNetMapsFrontend.Controllers;
using DotNetMapsFrontend.Models;
using DotNetMapsFrontend.Services;
using DotNetMapsFrontend.Constants;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Additional edge case tests to improve branch coverage
/// </summary>
[TestFixture]
public class EdgeCaseTests
{
    #region PointOfInterestController Edge Cases

    [Test]
    public async Task Create_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var controller = new PointOfInterestController(mockService.Object);
        
        var poi = new PointOfInterest
        {
            Category = "test",
            Details = "test details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };
        
        controller.ModelState.AddModelError("test", "test error");

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithNorthPoleCoordinates_ReturnsJson()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "research_station",
            Name = "North Pole Station",
            Details = "North Pole Station",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 0.0, 90.0 } // North Pole
            }
        };
        
        mockService.Setup(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
            .ReturnsAsync(poi);
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithSouthPoleCoordinates_ReturnsJson()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "research_station",
            Name = "South Pole Station",
            Details = "South Pole Station",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 0.0, -90.0 } // South Pole
            }
        };
        
        mockService.Setup(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
            .ReturnsAsync(poi);
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithInternationalDateLine_ReturnsJson()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "landmark",
            Name = "International Date Line",
            Details = "International Date Line",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 180.0, 0.0 } // Date Line
            }
        };
        
        mockService.Setup(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
            .ReturnsAsync(poi);
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithPrimeMeridian_ReturnsJson()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "landmark",
            Name = "Prime Meridian",
            Details = "Prime Meridian",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 0.0, 0.0 } // Null Island
            }
        };
        
        mockService.Setup(s => s.CreatePointOfInterestAsync(It.IsAny<PointOfInterest>()))
            .ReturnsAsync(poi);
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithLatitudeTooHigh_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "test",
            Name = "Test POI",
            Details = "Invalid location",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 0.0, 91.0 } // Latitude > 90
            }
        };
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Does.Contain("Coordinates are out of valid range"));
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithLatitudeTooLow_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "test",
            Details = "Invalid location",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 0.0, -91.0 } // Latitude < -90
            }
        };
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithLongitudeTooHigh_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "test",
            Details = "Invalid location",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 181.0, 0.0 } // Longitude > 180
            }
        };
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Create_WithLongitudeTooLow_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var poi = new PointOfInterest
        {
            Category = "test",
            Details = "Invalid location",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { -181.0, 0.0 } // Longitude < -180
            }
        };
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Create(poi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        controller.Dispose();
    }

    [Test]
    public async Task Update_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var controller = new PointOfInterestController(mockService.Object);
        
        var poi = new PointOfInterest
        {
            Category = "test",
            Details = "test details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude }
            }
        };
        
        controller.ModelState.AddModelError("test", "test error");

        // Act
        var result = await controller.Update("someid", poi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        controller.Dispose();
    }

    #endregion

    #region GetAll Edge Cases

    [Test]
    public async Task GetAll_WithNoParameters_UsesDefaults()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var expectedPois = new List<PointOfInterest>
        {
            new PointOfInterest
            {
                Category = "test",
                Details = "test",
                Location = new Location { Type = "Point", Coordinates = new[] { MapDefaults.DefaultLongitude, MapDefaults.DefaultLatitude } }
            }
        };
        
        mockService.Setup(s => s.GetPointsOfInterestAsync())
            .ReturnsAsync(expectedPois);
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.GetAll(null, null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        mockService.Verify(s => s.GetPointsOfInterestAsync(), Times.Once);
        controller.Dispose();
    }

    [Test]
    public async Task GetAll_WhenServiceThrows_ReturnsEmptyList()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        mockService.Setup(s => s.GetPointsOfInterestAsync())
            .ThrowsAsync(new Exception("Service error"));
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.GetAll(null, null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var value = jsonResult.Value as List<PointOfInterest>;
        Assert.That(value, Is.Not.Null);
        Assert.That(value, Is.Empty);
        controller.Dispose();
    }

    [Test]
    public async Task Index_WithAllParameters_CallsServiceCorrectly()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var expectedPois = new List<PointOfInterest>();
        
        mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(expectedPois);
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Index(MapDefaults.DefaultLatitude, MapDefaults.DefaultLongitude, 5000);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        mockService.Verify(s => s.GetPointsOfInterestAsync(MapDefaults.DefaultLatitude, MapDefaults.DefaultLongitude, 5000), Times.Once);
        controller.Dispose();
    }

    [Test]
    public async Task Index_WithoutParameters_UsesDefaults()
    {
        // Arrange
        var mockService = new Mock<IPointOfInterestService>();
        var expectedPois = new List<PointOfInterest>();
        
        mockService.Setup(s => s.GetPointsOfInterestAsync())
            .ReturnsAsync(expectedPois);
        
        var controller = new PointOfInterestController(mockService.Object);

        // Act
        var result = await controller.Index(null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        mockService.Verify(s => s.GetPointsOfInterestAsync(), Times.Once);
        controller.Dispose();
    }

    #endregion
}
