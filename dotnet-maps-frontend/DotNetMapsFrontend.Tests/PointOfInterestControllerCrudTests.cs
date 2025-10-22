#nullable disable

using DotNetMapsFrontend.Controllers;
using DotNetMapsFrontend.Models;
using DotNetMapsFrontend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Comprehensive tests for PointOfInterestController CRUD operations
/// Tests cover GetById, Update, and Delete endpoints to improve code coverage
/// </summary>
[TestFixture]
public class PointOfInterestControllerCrudTests
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

    #region GetById Tests

    [Test]
    public async Task GetById_WithValidId_ReturnsJsonResult()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var expectedPoi = new PointOfInterest
        {
            Href = testId,
            Category = "restaurant",
            Details = "Test Restaurant",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        _mockService.Setup(s => s.GetPointOfInterestByIdAsync(testId))
            .ReturnsAsync(expectedPoi);

        // Act
        var result = await _controller.GetById(testId);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        Assert.That(jsonResult.Value, Is.EqualTo(expectedPoi));
        _mockService.Verify(s => s.GetPointOfInterestByIdAsync(testId), Times.Once);
    }

    [Test]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var testId = "nonexistent123";
        _mockService.Setup(s => s.GetPointOfInterestByIdAsync(testId))
            .ReturnsAsync((PointOfInterest)null);

        // Act
        var result = await _controller.GetById(testId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
        _mockService.Verify(s => s.GetPointOfInterestByIdAsync(testId), Times.Once);
    }

    [Test]
    public async Task GetById_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        _mockService.Setup(s => s.GetPointOfInterestByIdAsync(testId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetById(testId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Failed to retrieve Point of Interest."));
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_WithValidData_ReturnsJsonResult()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "museum",
            Details = "Updated Museum Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        var updatedPoi = new PointOfInterest
        {
            Href = testId,
            Category = updatePoi.Category,
            Details = updatePoi.Details,
            Location = updatePoi.Location
        };

        _mockService.Setup(s => s.UpdatePointOfInterestAsync(testId, It.IsAny<PointOfInterest>()))
            .ReturnsAsync(updatedPoi);

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        Assert.That(jsonResult.Value, Is.EqualTo(updatedPoi));
        _mockService.Verify(s => s.UpdatePointOfInterestAsync(testId, It.IsAny<PointOfInterest>()), Times.Once);
    }

    [Test]
    public async Task Update_WithMissingCategory_ReturnsBadRequest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "", // Empty category
            Details = "Updated Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Is.EqualTo("Category is required."));
        _mockService.Verify(s => s.UpdatePointOfInterestAsync(It.IsAny<string>(), It.IsAny<PointOfInterest>()), Times.Never);
    }

    [Test]
    public async Task Update_WithNullCategory_ReturnsBadRequest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = null,
            Details = "Updated Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Is.EqualTo("Category is required."));
    }

    [Test]
    public async Task Update_WithMissingDetails_ReturnsBadRequest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "", // Empty details
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Is.EqualTo("Details are required."));
    }

    [Test]
    public async Task Update_WithNullDetails_ReturnsBadRequest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = null,
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WithNullLocation_ReturnsBadRequest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "Updated Details",
            Location = null
        };

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Is.EqualTo("Valid location is required."));
    }

    [Test]
    public async Task Update_WithNullCoordinates_ReturnsBadRequest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "Updated Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = null
            }
        };

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Is.EqualTo("Valid location is required."));
    }

    [Test]
    public async Task Update_WithInvalidCoordinatesLength_ReturnsBadRequest()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        var updatePoi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "Updated Details",
            Location = new Location
            {
                Type = "Point",
                Coordinates = new[] { 13.7373 } // Only one coordinate
            }
        };

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Is.EqualTo("Valid location is required."));
    }

    [Test]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
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
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        _mockService.Setup(s => s.UpdatePointOfInterestAsync(testId, It.IsAny<PointOfInterest>()))
            .ReturnsAsync((PointOfInterest)null);

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound.Value, Does.Contain(testId));
    }

    [Test]
    public async Task Update_WhenServiceThrowsException_ReturnsInternalServerError()
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
                Coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        _mockService.Setup(s => s.UpdatePointOfInterestAsync(testId, It.IsAny<PointOfInterest>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Update(testId, updatePoi);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Does.Contain("Failed to update Point of Interest"));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        _mockService.Setup(s => s.DeletePointOfInterestAsync(testId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(testId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _mockService.Verify(s => s.DeletePointOfInterestAsync(testId), Times.Once);
    }

    [Test]
    public async Task Delete_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        _mockService.Setup(s => s.DeletePointOfInterestAsync(testId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.Delete(testId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Failed to delete Point of Interest."));
    }

    [Test]
    public async Task Delete_MultipleConsecutiveCalls_AreIdempotent()
    {
        // Arrange
        var testId = "507f1f77bcf86cd799439011";
        _mockService.Setup(s => s.DeletePointOfInterestAsync(testId))
            .Returns(Task.CompletedTask);

        // Act - Delete twice
        var result1 = await _controller.Delete(testId);
        var result2 = await _controller.Delete(testId);

        // Assert - Both should return 204 No Content (idempotent)
        Assert.That(result1, Is.InstanceOf<NoContentResult>());
        Assert.That(result2, Is.InstanceOf<NoContentResult>());
        _mockService.Verify(s => s.DeletePointOfInterestAsync(testId), Times.Exactly(2));
    }

    #endregion
}
