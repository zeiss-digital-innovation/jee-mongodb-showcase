using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests;

/// <summary>
/// Exception and error handling tests for PointOfInterestService
/// Tests error scenarios and exception paths to improve branch coverage
/// </summary>
public class PointOfInterestServiceExceptionTests
{
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<PointOfInterest>> _mockCollection;
    private readonly Mock<IOptions<MongoSettings>> _mockSettings;
    private readonly Mock<ILogger<PointOfInterestService>> _mockLogger;
    private readonly PointOfInterestService _service;

    public PointOfInterestServiceExceptionTests()
    {
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<PointOfInterest>>();
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
        _mockDatabase.Setup(d => d.GetCollection<PointOfInterest>(It.IsAny<string>(), null))
            .Returns(_mockCollection.Object);

        // Setup database namespace
        var mockNamespace = new DatabaseNamespace("test");
        _mockDatabase.Setup(d => d.DatabaseNamespace).Returns(mockNamespace);

        // Mock CountDocuments for initialization
        _mockCollection.Setup(c => c.CountDocuments(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            null,
            default))
            .Returns(0);

        // Mock indexes for initialization
        var mockIndexManager = new Mock<IMongoIndexManager<PointOfInterest>>();
        _mockCollection.Setup(c => c.Indexes).Returns(mockIndexManager.Object);

        _service = new PointOfInterestService(_mockDatabase.Object, _mockSettings.Object, _mockLogger.Object);
    }

    #region GetAllPoisAsync Exception Tests

    [Fact]
    public async Task GetAllPoisAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<PointOfInterest>>();
        mockCursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database connection lost"));

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            It.IsAny<FindOptions<PointOfInterest, PointOfInterest>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.GetAllPoisAsync());
    }

    #endregion

    #region GetPoiByIdAsync Exception Tests

    [Fact]
    public async Task GetPoiByIdAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var validId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        
        var mockCursor = new Mock<IAsyncCursor<PointOfInterest>>();
        mockCursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database error"));

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            It.IsAny<FindOptions<PointOfInterest, PointOfInterest>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.GetPoiByIdAsync(validId));
    }

    #endregion

    #region GetPoisByCategoryAsync Exception Tests

    [Fact]
    public async Task GetPoisByCategoryAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<PointOfInterest>>();
        mockCursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database error"));

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            It.IsAny<FindOptions<PointOfInterest, PointOfInterest>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.GetPoisByCategoryAsync("restaurant"));
    }

    #endregion

    #region SearchPoisAsync Exception Tests

    [Fact]
    public async Task SearchPoisAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<PointOfInterest>>();
        mockCursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database error"));

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            It.IsAny<FindOptions<PointOfInterest, PointOfInterest>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.SearchPoisAsync("test search"));
    }

    #endregion

    #region GetNearbyPoisAsync Exception Tests

    [Fact]
    public async Task GetNearbyPoisAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<PointOfInterest>>();
        mockCursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database error"));

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            It.IsAny<FindOptions<PointOfInterest, PointOfInterest>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => 
            _service.GetNearbyPoisAsync(13.7373, 51.0504, 5.0));
    }

    #endregion

    #region GetNearbyPoisByCategoriesAsync Exception Tests

    [Fact]
    public async Task GetNearbyPoisByCategoriesAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var categories = new List<string> { "restaurant", "museum" };
        var mockCursor = new Mock<IAsyncCursor<PointOfInterest>>();
        mockCursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database error"));

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            It.IsAny<FindOptions<PointOfInterest, PointOfInterest>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => 
            _service.GetNearbyPoisByCategoriesAsync(13.7373, 51.0504, 5.0, categories));
    }

    #endregion

    #region CreatePoiAsync Exception Tests

    [Fact]
    public async Task CreatePoiAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var poi = new PointOfInterest
        {
            Category = "restaurant",
            Details = "Test Restaurant",
            Location = new Location(13.7373, 51.0504)
        };

        _mockCollection.Setup(c => c.InsertOneAsync(
            It.IsAny<PointOfInterest>(),
            null,
            default))
            .ThrowsAsync(new MongoException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.CreatePoiAsync(poi));
    }

    #endregion

    #region UpdatePoiAsync Exception Tests

    [Fact]
    public async Task UpdatePoiAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var validId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var poi = new PointOfInterest
        {
            Category = "museum",
            Details = "Updated Museum",
            Location = new Location(13.7373, 51.0504)
        };

        _mockCollection.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            It.IsAny<PointOfInterest>(),
            It.IsAny<ReplaceOptions>(),
            default))
            .ThrowsAsync(new MongoException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.UpdatePoiAsync(validId, poi));
    }

    #endregion

    #region DeletePoiAsync Exception Tests

    [Fact]
    public async Task DeletePoiAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        var validId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

        _mockCollection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            default))
            .ThrowsAsync(new MongoException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.DeletePoiAsync(validId));
    }

    #endregion

    #region CountByCategoryAsync Exception Tests

    [Fact]
    public async Task CountByCategoryAsync_WithDatabaseException_ThrowsException()
    {
        // Arrange
        _mockCollection.Setup(c => c.CountDocumentsAsync(
            It.IsAny<FilterDefinition<PointOfInterest>>(),
            null,
            default))
            .ThrowsAsync(new MongoException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _service.CountByCategoryAsync("restaurant"));
    }

    #endregion
}
