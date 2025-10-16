using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Services;
using System;

namespace DotNetMongoDbBackend.Tests.Tests;

public class CategoryControllerTests
{
    private readonly Mock<IPointOfInterestService> _mockService;
    private readonly Mock<ILogger<CategoryController>> _mockLogger;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _mockLogger = new Mock<ILogger<CategoryController>>();
        _controller = new CategoryController(_mockService.Object, _mockLogger.Object);
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
    public async Task GetAvailableCategories_ShouldReturn500_WhenServiceThrowsException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAvailableCategoriesAsync())
                   .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAvailableCategories();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<string>>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Contains("Internal server error", statusResult.Value?.ToString());
    }
}

