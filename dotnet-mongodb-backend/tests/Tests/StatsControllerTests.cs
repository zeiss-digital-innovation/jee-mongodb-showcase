using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;
using DotNetMongoDbBackend.Controllers;
using DotNetMongoDbBackend.Services;
using System;

namespace DotNetMongoDbBackend.Tests.Tests;

public class StatsControllerTests
{
    private readonly Mock<IPointOfInterestService> _mockService;
    private readonly Mock<ILogger<StatsController>> _mockLogger;
    private readonly StatsController _controller;

    public StatsControllerTests()
    {
        _mockService = new Mock<IPointOfInterestService>();
        _mockLogger = new Mock<ILogger<StatsController>>();
        _controller = new StatsController(_mockService.Object, _mockLogger.Object);
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
    public async Task GetCategoryCount_ShouldReturn500_WhenServiceThrowsException()
    {
        // Arrange
        _mockService.Setup(s => s.CountByCategoryAsync(It.IsAny<string>()))
                   .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetCategoryCount("restaurant");

        // Assert
        var actionResult = Assert.IsType<ActionResult<long>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Contains("Internal server error", statusResult.Value?.ToString());
    }
}

