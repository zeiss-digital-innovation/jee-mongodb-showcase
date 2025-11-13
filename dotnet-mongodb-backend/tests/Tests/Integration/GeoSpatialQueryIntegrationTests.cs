using System.Threading.Tasks;
using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Services;
using DotNetMongoDbBackend.Tests.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests.Integration;

/// <summary>
/// Integration tests for MongoDB Geo-Spatial queries using Testcontainers.
/// Tests real MongoDB 2dsphere index functionality and geographic search operations.
/// </summary>
public class GeoSpatialQueryIntegrationTests : IClassFixture<MongoDbTestFixture>
{
    private readonly MongoDbTestFixture _fixture;
    private readonly PointOfInterestService _service;
    private readonly Mock<ILogger<PointOfInterestService>> _mockLogger;

    // Dresden coordinates
    private const double DresdenLongitude = 13.7373;
    private const double DresdenLatitude = 51.0504;

    public GeoSpatialQueryIntegrationTests(MongoDbTestFixture fixture)
    {
        _fixture = fixture;
        _mockLogger = new Mock<ILogger<PointOfInterestService>>();
        
        _service = new PointOfInterestService(
            _fixture.Database,
            _fixture.GetMongoSettings(),
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetNearbyPois_ShouldReturnPoisWithinRadius()
    {
        // Arrange - Clear and create test POIs
        await _fixture.ClearCollectionAsync();
        
        // POI in Dresden city center
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Dresden Hauptbahnhof",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7320, 51.0404] },
            Category = "Transport"
        });
        
        // POI close to Dresden
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Dresden Neustadt",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7402, 51.0643] },
            Category = "District"
        });
        
        // POI far from Dresden (Leipzig)
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Leipzig Hauptbahnhof",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [12.3810, 51.3455] },
            Category = "Transport"
        });

        // Act - Search within 5km radius of Dresden center
        var nearbyPois = await _service.GetNearbyPoisAsync(
            DresdenLongitude, 
            DresdenLatitude, 
            5.0 // 5km
        );

        // Assert - Should find 2 Dresden POIs, but not Leipzig
        Assert.NotEmpty(nearbyPois);
        Assert.Contains(nearbyPois, p => p.Name == "Dresden Hauptbahnhof");
        Assert.Contains(nearbyPois, p => p.Name == "Dresden Neustadt");
        Assert.DoesNotContain(nearbyPois, p => p.Name == "Leipzig Hauptbahnhof");
    }

    [Fact]
    public async Task GetNearbyPois_WithSmallRadius_ShouldReturnOnlyClosestPois()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        // Very close POI (within 500m)
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Very Close POI",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7380, 51.0510] },
            Category = "Restaurant"
        });
        
        // Further POI (within 2km)
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Further POI",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7500, 51.0600] },
            Category = "Museum"
        });

        // Act - Search with small 1km radius
        var nearbyPois = await _service.GetNearbyPoisAsync(
            DresdenLongitude, 
            DresdenLatitude, 
            1.0 // 1km
        );

        // Assert - Should only find very close POI
        Assert.NotEmpty(nearbyPois);
        Assert.Contains(nearbyPois, p => p.Name == "Very Close POI");
        Assert.DoesNotContain(nearbyPois, p => p.Name == "Further POI");
    }

    [Fact]
    public async Task GetNearbyPois_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Restaurant in Dresden",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7380, 51.0510] },
            Category = "restaurant"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Museum in Dresden",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7390, 51.0515] },
            Category = "museum"
        });

        // Act - Search within 2km and filter for Restaurants
        var nearbyRestaurants = await _service.GetNearbyPoisByCategoriesAsync(
            DresdenLongitude,
            DresdenLatitude,
            2.0,
            ["Restaurant"]
        );

        // Assert - Should only find Restaurant, not Museum
        Assert.NotEmpty(nearbyRestaurants);
        Assert.All(nearbyRestaurants, poi => Assert.Equal("restaurant", poi.Category));
        Assert.Contains(nearbyRestaurants, p => p.Name == "Restaurant in Dresden");
        Assert.DoesNotContain(nearbyRestaurants, p => p.Name == "Museum in Dresden");
    }

    [Fact]
    public async Task GetNearbyPois_WithMultipleCategories_ShouldReturnAllMatching()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Restaurant A",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7380, 51.0510] },
            Category = "restaurant"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Museum B",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7390, 51.0515] },
            Category = "museum"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Park C",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7400, 51.0520] },
            Category = "park"
        });

        // Act - Search for Restaurants and Museums
        var results = await _service.GetNearbyPoisByCategoriesAsync(
            DresdenLongitude,
            DresdenLatitude,
            2.0,
            ["Restaurant", "Museum"]
        );

        // Assert - Should find both Restaurant and Museum, but not Park
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.Name == "Restaurant A");
        Assert.Contains(results, p => p.Name == "Museum B");
        Assert.DoesNotContain(results, p => p.Name == "Park C");
    }

    [Fact]
    public async Task GetNearbyPois_EmptyRadius_ShouldReturnEmptyList()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "Test POI",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7380, 51.0510] },
            Category = "Test"
        });

        // Act - Search with 0 radius
        var results = await _service.GetNearbyPoisAsync(
            DresdenLongitude,
            DresdenLatitude,
            0.0 // 0 km radius
        );

        // Assert - Should return empty (no POI exactly at center)
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetNearbyPois_LargeRadius_ShouldReturnAllPois()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        // Create POIs at different distances
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "POI 1km",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [13.7480, 51.0600] },
            Category = "Test"
        });
        
        await _service.CreatePoiAsync(new PointOfInterestEntity
        {
            Name = "POI 50km",
            Details = "Test details",
            Location = new LocationEntity { Coordinates = [14.2000, 51.5000] },
            Category = "Test"
        });

        // Act - Search with 100km radius
        var results = await _service.GetNearbyPoisAsync(
            DresdenLongitude,
            DresdenLatitude,
            100.0 // 100km
        );

        // Assert - Should find all POIs
        Assert.True(results.Count >= 2);
        Assert.Contains(results, p => p.Name == "POI 1km");
        Assert.Contains(results, p => p.Name == "POI 50km");
    }

    [Fact]
    public async Task GeoSpatialQuery_ShouldUse2dsphereIndex()
    {
        // Arrange - This test verifies that MongoDB uses the 2dsphere index
        await _fixture.ClearCollectionAsync();
        
        // Create many POIs to make index usage relevant
        for (int i = 0; i < 20; i++)
        {
            await _service.CreatePoiAsync(new PointOfInterestEntity
            {
                Name = $"POI {i}",
                Details = "Test details",
                Location = new LocationEntity 
                { 
                    Coordinates =
                    [
                        DresdenLongitude + (i * 0.01), 
                        DresdenLatitude + (i * 0.01) 
                    ] 
                },
                Category = "Test"
            });
        }

        // Act - Perform geo query (MongoDB will use 2dsphere index automatically)
        var results = await _service.GetNearbyPoisAsync(
            DresdenLongitude,
            DresdenLatitude,
            5.0
        );

        // Assert - Query should succeed and return results
        Assert.NotEmpty(results);
        // If index doesn't exist, query would fail or be very slow
    }

    [Fact]
    public async Task GeoSpatialQuery_WithInvalidCoordinates_ShouldThrowException()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();

        // Act & Assert - Query with invalid coordinates should throw MongoCommandException
        await Assert.ThrowsAsync<MongoDB.Driver.MongoCommandException>(async () =>
        {
            await _service.GetNearbyPoisAsync(
                longitude: 999.0, // Invalid longitude
                latitude: 999.0, // Invalid latitude
                radiusInKm: 1.0
            );
        });
    }
}
