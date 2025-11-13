using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests;

/// <summary>
/// Unit tests for PointOfInterestService geographic query operations
/// Tests cover GetNearbyPoisAsync and GetNearbyPoisByCategoriesAsync
/// </summary>
public class PointOfInterestServiceGeoTests
{
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<PointOfInterestEntity>> _mockCollection;
    private readonly Mock<IOptions<MongoSettings>> _mockSettings;
    private readonly Mock<ILogger<PointOfInterestService>> _mockLogger;
    private readonly PointOfInterestService _service;

    public PointOfInterestServiceGeoTests()
    {
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<PointOfInterestEntity>>();
        _mockSettings = new Mock<IOptions<MongoSettings>>();
        _mockLogger = new Mock<ILogger<PointOfInterestService>>();

        // Setup MongoSettings
        var settings = new MongoSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            Database = "test",
            Collections = new MongoSettings.CollectionNames
            {
                Pois = "pois"
            }
        };
        _mockSettings.Setup(s => s.Value).Returns(settings);

        // Setup database to return mock collection
        _mockDatabase.Setup(d => d.GetCollection<PointOfInterestEntity>(It.IsAny<string>(), null))
            .Returns(_mockCollection.Object);

        // Setup database namespace
        var mockNamespace = new DatabaseNamespace("test");
        _mockDatabase.Setup(d => d.DatabaseNamespace).Returns(mockNamespace);

        // Mock CountDocuments for initialization
        _mockCollection.Setup(c => c.CountDocuments(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            null,
            default))
            .Returns(0);

        // Mock indexes for initialization
        var mockIndexManager = new Mock<IMongoIndexManager<PointOfInterestEntity>>();
        _mockCollection.Setup(c => c.Indexes).Returns(mockIndexManager.Object);

        _service = new PointOfInterestService(_mockDatabase.Object, _mockSettings.Object, _mockLogger.Object);
    }

    #region GetNearbyPoisAsync Tests

    [Fact]
    public async Task GetNearbyPoisAsync_WithValidCoordinates_ReturnsPois()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 5.0;

        var nearbyPois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity
            {
                Name = "Nearby Restaurant",
                Category = "restaurant",
                Details = "Close to location",
                Location = new LocationEntity { Type = "Point", Coordinates = [13.74, 51.05] }
            },
            new PointOfInterestEntity
            {
                Name = "Nearby Museum",
                Category = "museum",
                Details = "Also close",
                Location = new LocationEntity { Type = "Point", Coordinates = [13.73, 51.05] }
            }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(nearbyPois);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisAsync(longitude, latitude, radiusKm);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Nearby Restaurant");
        Assert.Contains(result, p => p.Name == "Nearby Museum");
    }

    [Fact]
    public async Task GetNearbyPoisAsync_WithNoResults_ReturnsEmptyList()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 0.1; // Very small radius

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(new List<PointOfInterestEntity>());

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisAsync(longitude, latitude, radiusKm);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNearbyPoisAsync_WithSmallRadius_ReturnsLimitedResults()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 1.0; // Small radius

        var nearbyPois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity
            {
                Name = "Nearby Restaurant",
                Category = "restaurant",
                Details = "Close to location",
                Location = new LocationEntity { Type = "Point", Coordinates = [13.74, 51.05] }
            }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(nearbyPois);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisAsync(longitude, latitude, radiusKm);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetNearbyPoisAsync_WithZeroRadius_ReturnsEmptyList()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 0.0;

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(new List<PointOfInterestEntity>());

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisAsync(longitude, latitude, radiusKm);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNearbyPoisAsync_WithExtremeLatitude_ReturnsResults()
    {
        // Arrange - North Pole
        double longitude = 0;
        double latitude = 89.9;
        double radiusKm = 100.0;

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(new List<PointOfInterestEntity>());

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisAsync(longitude, latitude, radiusKm);

        // Assert - Should not throw
        Assert.NotNull(result);
    }

    #endregion

    #region GetNearbyPoisByCategoriesAsync Tests

    [Fact]
    public async Task GetNearbyPoisByCategoriesAsync_WithValidCategories_ReturnsPois()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 5.0;
        var categories = new List<string> { "restaurant", "museum" };

        var nearbyPois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity
            {
                Name = "Nearby Restaurant",
                Category = "restaurant",
                Details = "Close restaurant",
                Location = new LocationEntity { Type = "Point", Coordinates = [13.74, 51.05] }
            },
            new PointOfInterestEntity
            {
                Name = "Nearby Museum",
                Category = "museum",
                Details = "Close museum",
                Location = new LocationEntity { Type = "Point", Coordinates = [13.73, 51.05] }
            }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(nearbyPois);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisByCategoriesAsync(longitude, latitude, radiusKm, categories);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Category == "restaurant");
        Assert.Contains(result, p => p.Category == "museum");
    }

    [Fact]
    public async Task GetNearbyPoisByCategoriesAsync_WithEmptyCategories_ReturnsEmptyList()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 5.0;
        var categories = new List<string>();

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(new List<PointOfInterestEntity>());

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisByCategoriesAsync(longitude, latitude, radiusKm, categories);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNearbyPoisByCategoriesAsync_WithSingleCategory_ReturnsPois()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 5.0;
        var categories = new List<string> { "restaurant" };

        var nearbyPois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity
            {
                Name = "Nearby Restaurant",
                Category = "restaurant",
                Details = "Close restaurant",
                Location = new LocationEntity { Type = "Point", Coordinates = [13.74, 51.05] }
            }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(nearbyPois);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisByCategoriesAsync(longitude, latitude, radiusKm, categories);

        // Assert
        Assert.Single(result);
        Assert.Equal("restaurant", result[0].Category);
    }

    [Fact]
    public async Task GetNearbyPoisByCategoriesAsync_WithSmallRadius_ReturnsLimitedResults()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 1.0; // Small radius
        var categories = new List<string> { "restaurant", "museum" };

        var nearbyPois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity
            {
                Name = "Nearby Restaurant",
                Category = "restaurant",
                Details = "Close restaurant",
                Location = new LocationEntity { Type = "Point", Coordinates = [13.74, 51.05] }
            }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(nearbyPois);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisByCategoriesAsync(longitude, latitude, radiusKm, categories);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetNearbyPoisByCategoriesAsync_WithNonMatchingCategories_ReturnsEmptyList()
    {
        // Arrange
        double longitude = 13.7373;
        double latitude = 51.0504;
        double radiusKm = 5.0;
        var categories = new List<string> { "nonexistent_category" };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(new List<PointOfInterestEntity>());

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetNearbyPoisByCategoriesAsync(longitude, latitude, radiusKm, categories);

        // Assert
        Assert.Empty(result);
    }

    #endregion
}

