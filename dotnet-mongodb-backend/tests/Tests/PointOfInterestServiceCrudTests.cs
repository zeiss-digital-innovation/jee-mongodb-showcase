using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests;

/// <summary>
/// Comprehensive unit tests for PointOfInterestService CRUD operations
/// Tests cover Create, Update, Delete, Search, and Validation logic
/// </summary>
public class PointOfInterestServiceCrudTests
{
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<PointOfInterestEntity>> _mockCollection;
    private readonly Mock<IOptions<MongoSettings>> _mockSettings;
    private readonly Mock<ILogger<PointOfInterestService>> _mockLogger;
    private readonly PointOfInterestService _service;

    public PointOfInterestServiceCrudTests()
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

    #region GetPoiByIdAsync Tests

    [Fact]
    public async Task GetPoiByIdAsync_WithValidId_ReturnsPoi()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId().ToString();
        var expectedPoi = new PointOfInterestEntity {
            Id = testId,
            Category = "restaurant",
            Details = "Test Restaurant",
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(new List<PointOfInterestEntity> { expectedPoi });

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetPoiByIdAsync(testId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testId, result.Id);
        Assert.Equal("restaurant", result.Category);
    }

    [Fact]
    public async Task GetPoiByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetPoiByIdAsync("invalid-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPoiByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId().ToString();

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
        var result = await _service.GetPoiByIdAsync(testId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CreatePoiAsync Tests

    [Fact]
    public async Task CreatePoiAsync_WithValidPoi_CreatesPoi()
    {
        // Arrange
        var newPoi = new PointOfInterestEntity {
            Category = "restaurant",
            Details = "New Restaurant",
            Name = "Test Restaurant",
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        _mockCollection.Setup(c => c.InsertOneAsync(
            It.IsAny<PointOfInterestEntity>(),
            null,
            default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreatePoiAsync(newPoi);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("restaurant", result.Category);
        Assert.Equal("New Restaurant", result.Details);
        // Href is not a property of Entity (only in DTO)
        _mockCollection.Verify(c => c.InsertOneAsync(
            It.IsAny<PointOfInterestEntity>(),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task CreatePoiAsync_WithNullPoi_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.CreatePoiAsync(null!));
    }

    [Fact]
    public async Task CreatePoiAsync_WithMissingDetails_ThrowsArgumentException()
    {
        // Arrange
        var poi = new PointOfInterestEntity {
            Name = "Test POI",
            Category = "restaurant",
            Details = "", // Empty
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreatePoiAsync(poi));
        Assert.Contains("Details", ex.Message);
    }

    [Fact]
    public async Task CreatePoiAsync_WithMissingCategory_ThrowsArgumentException()
    {
        // Arrange
        var poi = new PointOfInterestEntity {
            Name = "Test POI",
            Category = "", // Empty
            Details = "Test Details",
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreatePoiAsync(poi));
        Assert.Contains("Category", ex.Message);
    }

    [Fact]
    public async Task CreatePoiAsync_WithNullLocation_ThrowsArgumentException()
    {
        // Arrange
        var poi = new PointOfInterestEntity {
            Name = "Test POI",
            Category = "restaurant",
            Details = "Test Details",
            Location = null! // Null location
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreatePoiAsync(poi));
        Assert.Contains("Location", ex.Message);
    }

    [Fact]
    public async Task CreatePoiAsync_WithInvalidLatitude_ThrowsArgumentException()
    {
        // Arrange - Latitude > 90
        var poi = new PointOfInterestEntity {
            Name = "Test POI",
            Category = "restaurant",
            Details = "Test Details",
            Location = new LocationEntity { Type = "Point", Coordinates = [0, 91] } // Invalid latitude
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreatePoiAsync(poi));
        Assert.Contains("Latitude", ex.Message);
    }

    [Fact]
    public async Task CreatePoiAsync_WithInvalidLongitude_ThrowsArgumentException()
    {
        // Arrange - Longitude > 180
        var poi = new PointOfInterestEntity {
            Name = "Test POI",
            Category = "restaurant",
            Details = "Test Details",
            Location = new LocationEntity { Type = "Point", Coordinates = [181, 0] } // Invalid longitude
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreatePoiAsync(poi));
        Assert.Contains("Longitude", ex.Message);
    }

    #endregion

    #region UpdatePoiAsync Tests

    [Fact]
    public async Task UpdatePoiAsync_WithValidData_UpdatesPoi()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId().ToString();
        var updatePoi = new PointOfInterestEntity {
            Category = "museum",
            Details = "Updated Museum",
            Name = "Test Museum",
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        var mockResult = new Mock<ReplaceOneResult>();
        mockResult.Setup(r => r.MatchedCount).Returns(1);
        mockResult.Setup(r => r.ModifiedCount).Returns(1);

        _mockCollection.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<PointOfInterestEntity>(),
            It.IsAny<ReplaceOptions>(),
            default))
            .ReturnsAsync(mockResult.Object);

        // Act
        var result = await _service.UpdatePoiAsync(testId, updatePoi);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testId, result.Id);
        Assert.Equal("museum", result.Category);
        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<PointOfInterestEntity>(),
            It.IsAny<ReplaceOptions>(),
            default), Times.Once);
    }

    [Fact]
    public async Task UpdatePoiAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var updatePoi = new PointOfInterestEntity {
            Category = "museum",
            Details = "Updated Museum",
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        // Act
        var result = await _service.UpdatePoiAsync("invalid-id", updatePoi);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePoiAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId().ToString();
        var updatePoi = new PointOfInterestEntity {
            Name = "Updated POI",
            Category = "museum",
            Details = "Updated Museum",
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        var mockResult = new Mock<ReplaceOneResult>();
        mockResult.Setup(r => r.MatchedCount).Returns(0); // Not found

        _mockCollection.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<PointOfInterestEntity>(),
            It.IsAny<ReplaceOptions>(),
            default))
            .ReturnsAsync(mockResult.Object);

        // Act
        var result = await _service.UpdatePoiAsync(testId, updatePoi);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePoiAsync_WithInvalidData_ThrowsArgumentException()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId().ToString();
        var updatePoi = new PointOfInterestEntity {
            Category = "", // Invalid
            Details = "Updated Museum",
            Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdatePoiAsync(testId, updatePoi));
    }

    #endregion

    #region DeletePoiAsync Tests

    [Fact]
    public async Task DeletePoiAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId().ToString();

        var mockResult = new Mock<DeleteResult>();
        mockResult.Setup(r => r.DeletedCount).Returns(1);

        _mockCollection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            default))
            .ReturnsAsync(mockResult.Object);

        // Act
        var result = await _service.DeletePoiAsync(testId);

        // Assert
        Assert.True(result);
        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            default), Times.Once);
    }

    [Fact]
    public async Task DeletePoiAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeletePoiAsync("invalid-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeletePoiAsync_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        var testId = ObjectId.GenerateNewId().ToString();

        var mockResult = new Mock<DeleteResult>();
        mockResult.Setup(r => r.DeletedCount).Returns(0); // Not found

        _mockCollection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            default))
            .ReturnsAsync(mockResult.Object);

        // Act
        var result = await _service.DeletePoiAsync(testId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region SearchPoisAsync Tests

    [Fact]
    public async Task SearchPoisAsync_WithEmptySearchTerm_ReturnsAllPois()
    {
        // Arrange
        var allPois = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity { Category = "restaurant", Details = "Restaurant 1", Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] } },
            new PointOfInterestEntity { Category = "museum", Details = "Museum 1", Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] } }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
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
        var result = await _service.SearchPoisAsync("");

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchPoisAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var searchResults = new List<PointOfInterestEntity>
        {
            new PointOfInterestEntity { Name = "Test POI 1", Category = "restaurant", Details = "Details 1", Location = new LocationEntity { Type = "Point", Coordinates = [13.7373, 51.0504] } }
        };

        var mockCursor = new Mock<IAsyncCursor<PointOfInterestEntity>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockCursor.Setup(c => c.Current).Returns(searchResults);

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            It.IsAny<FindOptions<PointOfInterestEntity, PointOfInterestEntity>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.SearchPoisAsync("test", 5);

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region GetAvailableCategoriesAsync Tests
    // Note: GetAvailableCategoriesAsync is difficult to test due to MongoDB driver implementation
    // This would require integration testing with actual MongoDB or more complex mocking
    #endregion

    #region CountByCategoryAsync Tests

    [Fact]
    public async Task CountByCategoryAsync_ReturnsCount()
    {
        // Arrange
        _mockCollection.Setup(c => c.CountDocumentsAsync(
            It.IsAny<FilterDefinition<PointOfInterestEntity>>(),
            null,
            default))
            .ReturnsAsync(42);

        // Act
        var result = await _service.CountByCategoryAsync("restaurant");

        // Assert
        Assert.Equal(42, result);
    }

    #endregion
}

