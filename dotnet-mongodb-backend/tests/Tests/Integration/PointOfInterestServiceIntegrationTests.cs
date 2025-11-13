using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Services;
using DotNetMongoDbBackend.Tests.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests.Integration;

/// <summary>
/// Integration tests for PointOfInterestService using real MongoDB via Testcontainers.
/// Tests CRUD operations, BSON serialization, and MongoDB-specific features against a real database.
/// </summary>
public class PointOfInterestServiceIntegrationTests : IClassFixture<MongoDbTestFixture>
{
    private readonly MongoDbTestFixture _fixture;
    private readonly PointOfInterestService _service;
    private readonly Mock<ILogger<PointOfInterestService>> _mockLogger;

    public PointOfInterestServiceIntegrationTests(MongoDbTestFixture fixture)
    {
        _fixture = fixture;
        _mockLogger = new Mock<ILogger<PointOfInterestService>>();
        
        // Create service with real MongoDB connection
        _service = new PointOfInterestService(
            _fixture.Database,
            _fixture.GetMongoSettings(),
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreatePoi_ShouldPersistToMongoDB_AndGenerateObjectId()
    {
        // Arrange
        var poi = new PointOfInterestEntity
        {
            Name = "Integration Test POI",
            Details = "Created by integration test",
            Location = new LocationEntity
            {
                Type = "Point",
                Coordinates = [13.7373, 51.0504] // GeoJSON: [longitude, latitude]
            },
            Category = "Restaurant"
        };

        // Act
        var createdPoi = await _service.CreatePoiAsync(poi);

        // Assert
        Assert.NotNull(createdPoi);
        Assert.NotNull(createdPoi.Id);
        Assert.NotEqual("000000000000000000000000", createdPoi.Id);
        Assert.Equal("Integration Test POI", createdPoi.Name);
        Assert.Equal(13.7373, createdPoi.Location!.Coordinates[0]); // Longitude
        Assert.Equal(51.0504, createdPoi.Location.Coordinates[1]); // Latitude
    }

    [Fact]
    public async Task GetPoiById_ShouldRetrievePersistedPoi()
    {
        // Arrange
        var poi = new PointOfInterestEntity
        {
            Name = "Test Retrieve POI",
            Details = "For retrieval test",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Museum"
        };
        var created = await _service.CreatePoiAsync(poi);

        // Act
        var retrieved = await _service.GetPoiByIdAsync(created.Id!);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("Test Retrieve POI", retrieved.Name);
        Assert.Equal("Museum", retrieved.Category);
    }

    [Fact]
    public async Task UpdatePoi_ShouldModifyExistingDocument()
    {
        // Arrange
        var poi = new PointOfInterestEntity
        {
            Name = "Original Name",
            Details = "Original Description",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Park"
        };
        var created = await _service.CreatePoiAsync(poi);

        // Act - Update the POI
        created.Name = "Updated Name";
        created.Details = "Updated Description";
        created.Category = "Garden";
        var updated = await _service.UpdatePoiAsync(created.Id!, created);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Description", updated.Details);
        Assert.Equal("Garden", updated.Category);
        
        // Verify persistence by retrieving again
        var retrieved = await _service.GetPoiByIdAsync(created.Id!);
        Assert.Equal("Updated Name", retrieved!.Name);
    }

    [Fact]
    public async Task DeletePoi_ShouldRemoveFromDatabase()
    {
        // Arrange
        var poi = new PointOfInterestEntity
        {
            Name = "To Be Deleted",
            Details = "This will be deleted",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Test"
        };
        var created = await _service.CreatePoiAsync(poi);

        // Act
        await _service.DeletePoiAsync(created.Id!);

        // Assert - POI should not exist anymore
        var retrieved = await _service.GetPoiByIdAsync(created.Id!);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetAllPois_ShouldReturnAllCreatedPois()
    {
        // Arrange - Clear collection first
        await _fixture.ClearCollectionAsync();
        
        var poi1 = new PointOfInterestEntity
        {
            Name = "POI 1",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Restaurant"
        };
        var poi2 = new PointOfInterestEntity
        {
            Name = "POI 2",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.8, 51.1] },
            Category = "Museum"
        };
        
        await _service.CreatePoiAsync(poi1);
        await _service.CreatePoiAsync(poi2);

        // Act
        var allPois = await _service.GetAllPoisAsync();

        // Assert
        Assert.NotNull(allPois);
        Assert.True(allPois.Count >= 2, $"Expected at least 2 POIs, got {allPois.Count}");
        Assert.Contains(allPois, p => p.Name == "POI 1");
        Assert.Contains(allPois, p => p.Name == "POI 2");
    }

    [Fact]
    public async Task BsonSerialization_ShouldHandleLocationCorrectly()
    {
        // Arrange - Test BSON serialization of nested Location object
        var poi = new PointOfInterestEntity
        {
            Name = "BSON Test",
            Details = "Test details",
            Location = new LocationEntity
            {
                Coordinates = [13.123456789, 51.987654321]
            },
            Category = "Test"
        };

        // Act
        var created = await _service.CreatePoiAsync(poi);
        var retrieved = await _service.GetPoiByIdAsync(created.Id!);

        // Assert - Verify precision is maintained through BSON serialization
        Assert.NotNull(retrieved);
        Assert.Equal(13.123456789, retrieved.Location!.Coordinates[0], precision: 9);
        Assert.Equal(51.987654321, retrieved.Location.Coordinates[1], precision: 9);
    }

    [Fact]
    public async Task CategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Restaurant A",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Restaurant"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Museum B",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Museum"
        });

        // Act - Get only restaurants
        var restaurants = await _service.GetPoisByCategoryAsync("Restaurant");

        // Assert
        Assert.NotEmpty(restaurants);
        Assert.All(restaurants, poi => Assert.Equal("Restaurant", poi.Category));
        Assert.Contains(restaurants, p => p.Name == "Restaurant A");
        Assert.DoesNotContain(restaurants, p => p.Name == "Museum B");
    }

    [Fact]
    public async Task SearchByName_ShouldFindMatchingPois()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Dresden Central Station",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Transport"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Berlin Station",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.4, 52.5] },
            Category = "Transport"
        });

        // Act - Get all and filter by name (service doesn't have search parameter)
        var allPois = await _service.GetAllPoisAsync();
        var results = allPois.Where(p => p.Name != null && p.Name.Contains("Dresden", StringComparison.OrdinalIgnoreCase)).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, poi => Assert.Contains("Dresden", poi.Name, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(results, p => p.Name == "Berlin Station");
    }

    [Fact]
    public async Task GetAvailableCategories_ShouldReturnUniqueCategories()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "POI 1",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Restaurant"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "POI 2",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Museum"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "POI 3",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7, 51.0] },
            Category = "Restaurant" // Duplicate category
        });

        // Act
        var categories = await _service.GetAvailableCategoriesAsync();

        // Assert
        Assert.NotNull(categories);
        Assert.Contains("Restaurant", categories);
        Assert.Contains("Museum", categories);
        Assert.Equal(2, categories.Count); // Should be unique
    }
}
