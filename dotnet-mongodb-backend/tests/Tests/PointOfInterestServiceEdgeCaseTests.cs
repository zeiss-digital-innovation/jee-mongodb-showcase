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
/// Additional edge case tests for PointOfInterestService
/// Focuses on boundary conditions and special scenarios
/// </summary>
public class PointOfInterestServiceEdgeCaseTests
{
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<PointOfInterestEntity>> _mockCollection;
    private readonly Mock<IOptions<MongoSettings>> _mockSettings;
    private readonly Mock<ILogger<PointOfInterestService>> _mockLogger;
    private readonly PointOfInterestService _service;

    public PointOfInterestServiceEdgeCaseTests()
    {
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<PointOfInterestEntity>>();
        _mockSettings = new Mock<IOptions<MongoSettings>>();
        _mockLogger = new Mock<ILogger<PointOfInterestService>>();

        var settings = new MongoSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            Database = "test",
            Collections = new MongoSettings.CollectionNames { Pois = "pois" }
        };
        _mockSettings.Setup(s => s.Value).Returns(settings);

        _mockDatabase.Setup(d => d.GetCollection<PointOfInterestEntity>(It.IsAny<string>(), null))
            .Returns(_mockCollection.Object);

        var mockNamespace = new DatabaseNamespace("test");
        _mockDatabase.Setup(d => d.DatabaseNamespace).Returns(mockNamespace);

        _mockCollection.Setup(c => c.CountDocuments(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            null,
            default))
            .Returns(0);

        var mockIndexManager = new Mock<IMongoIndexManager<PointOfInterestEntity>>();
        _mockCollection.Setup(c => c.Indexes).Returns(mockIndexManager.Object);

        _service = new PointOfInterestService(_mockDatabase.Object, _mockSettings.Object, _mockLogger.Object);
    }

    #region SearchPoisAsync Edge Cases

    [Fact]
    public async Task SearchPoisAsync_WithWhitespaceOnly_ReturnsAllPois()
    {
        // Arrange
        var allPois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity { Name = "POI 1", Category = "restaurant", Details = "Test", Location = new LocationEntity { Type = "Point", Coordinates = [13, 51] } },
            new PointOfInterestEntity { Name = "POI 2", Category = "museum", Details = "Test", Location = new LocationEntity { Type = "Point", Coordinates = [13, 51] } }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(allPois);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.SearchPoisAsync("   ");

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchPoisAsync_WithLimitZero_ReturnsEmptyList()
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
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
        var result = await _service.SearchPoisAsync("test", 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchPoisAsync_WithSpecialCharacters_ReturnsResults()
    {
        // Arrange
        var pois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity { Name = "Café München", Category = "restaurant", Details = "Test", Location = new LocationEntity { Type = "Point", Coordinates = [13, 51] } }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(pois);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.SearchPoisAsync("Café");

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region Coordinate Edge Cases

    [Fact]
    public async Task GetNearbyPoisAsync_WithDateLineCoordinates_ReturnsResults()
    {
        // Arrange - Near International Date Line
        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
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
        var result = await _service.GetNearbyPoisAsync(179.9, 0, 10);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetNearbyPoisAsync_WithSouthPole_ReturnsResults()
    {
        // Arrange - South Pole
        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
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
        var result = await _service.GetNearbyPoisAsync(0, -89.9, 10);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region UpdatePoiAsync Edge Cases

    [Fact]
    public async Task UpdatePoiAsync_WithExistingIdButNullDetails_ThrowsArgumentException()
    {
        // Arrange
        var validId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var poi = new PointOfInterestEntity {
            Category = "restaurant",
            Details = null!, // Null details
            Location = new LocationEntity { Type = "Point", Coordinates = [13, 51] }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdatePoiAsync(validId, poi));
    }

    [Fact]
    public async Task UpdatePoiAsync_WithNegativeLatitude_ThrowsArgumentException()
    {
        // Arrange
        var validId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var poi = new PointOfInterestEntity {
            Category = "restaurant",
            Details = "Test",
            Location = new LocationEntity { Type = "Point", Coordinates = [0, -91] } // Invalid latitude
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdatePoiAsync(validId, poi));
    }

    [Fact]
    public async Task UpdatePoiAsync_WithPositiveLongitudeOverLimit_ThrowsArgumentException()
    {
        // Arrange
        var validId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var poi = new PointOfInterestEntity {
            Category = "restaurant",
            Details = "Test",
            Location = new LocationEntity { Type = "Point", Coordinates = [181, 0] } // Invalid longitude
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdatePoiAsync(validId, poi));
    }

    #endregion

    #region CreatePoiAsync Edge Cases

    [Fact]
    public async Task CreatePoiAsync_WithMinimalValidData_CreatesPoi()
    {
        // Arrange - Minimal required fields
        var poi = new PointOfInterestEntity {
            Name = "Minimal POI",
            Category = "other",
            Details = "Minimal POI",
            Location = new LocationEntity { Type = "Point", Coordinates = [0, 0] } // Null Island
        };

        _mockCollection.Setup(c => c.InsertOneAsync(
            It.IsAny<PointOfInterestEntity>(),
            null,
            default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreatePoiAsync(poi);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("other", result.Category);
    }

    [Fact]
    public async Task CreatePoiAsync_WithNegativeLongitude_CreatesPoi()
    {
        // Arrange - Western hemisphere
        var poi = new PointOfInterestEntity {
            Name = "Western POI",
            Category = "landmark",
            Details = "Western POI",
            Location = new LocationEntity { Type = "Point", Coordinates = [-120, 40] }
        };

        _mockCollection.Setup(c => c.InsertOneAsync(
            It.IsAny<PointOfInterestEntity>(),
            null,
            default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreatePoiAsync(poi);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(-120, result.Location.Coordinates[0]); // Entity uses Coordinates array
    }

    #endregion
}
