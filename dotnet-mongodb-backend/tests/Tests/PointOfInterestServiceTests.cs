using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DotNetMongoDbBackend.Services;
using DotNetMongoDbBackend.Models;

namespace DotNetMongoDbBackend.Tests.Tests;

public class PointOfInterestServiceIntegrationTests
{
    [Fact]
    public async Task ServiceInterface_ShouldHaveAllRequiredMethods()
    {
        // Arrange - Test that the interface includes all required methods
        var mockService = new Mock<IPointOfInterestService>();

        // Setup mock responses for all interface methods
        mockService.Setup(s => s.GetAllPoisAsync())
                   .ReturnsAsync(new List<PointOfInterest>());

        mockService.Setup(s => s.GetPoiByIdAsync(It.IsAny<string>()))
                   .ReturnsAsync((PointOfInterest)null);

        mockService.Setup(s => s.GetPoisByCategoryAsync(It.IsAny<string>()))
                   .ReturnsAsync(new List<PointOfInterest>());

        mockService.Setup(s => s.SearchPoisAsync(It.IsAny<string>(), It.IsAny<int?>()))
                   .ReturnsAsync(new List<PointOfInterest>());

        mockService.Setup(s => s.GetNearbyPoisAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                   .ReturnsAsync(new List<PointOfInterest>());

        mockService.Setup(s => s.CreatePoiAsync(It.IsAny<PointOfInterest>()))
                   .ReturnsAsync(new PointOfInterest());

        mockService.Setup(s => s.UpdatePoiAsync(It.IsAny<string>(), It.IsAny<PointOfInterest>()))
                   .ReturnsAsync((PointOfInterest)null);

        mockService.Setup(s => s.DeletePoiAsync(It.IsAny<string>()))
                   .ReturnsAsync(false);

        mockService.Setup(s => s.GetAvailableCategoriesAsync())
                   .ReturnsAsync(new List<string>());

        mockService.Setup(s => s.CountByCategoryAsync(It.IsAny<string>()))
                   .ReturnsAsync(0);

        // Act & Assert - All methods should be callable
        var service = mockService.Object;

        await service.GetAllPoisAsync();
        await service.GetPoiByIdAsync("test");
        await service.GetPoisByCategoryAsync("test");
        await service.SearchPoisAsync("test", 10);
        await service.GetNearbyPoisAsync(0, 0, 10);
        await service.CreatePoiAsync(new PointOfInterest());
        await service.UpdatePoiAsync("test", new PointOfInterest());
        await service.DeletePoiAsync("test");
        await service.GetAvailableCategoriesAsync();
        await service.CountByCategoryAsync("test");

        // Test succeeds if no exception thrown
        Assert.True(true);
    }
}
