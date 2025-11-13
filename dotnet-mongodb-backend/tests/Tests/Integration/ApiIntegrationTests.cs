using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Models.DTOs;
using DotNetMongoDbBackend.Tests.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests.Integration;

/// <summary>
/// End-to-End API Integration Tests using Testcontainers.
/// Tests complete flow: HTTP Request → Controller → Service → MongoDB Container
/// </summary>
public class ApiIntegrationTests : IClassFixture<MongoDbTestFixture>, IDisposable
{
    private readonly MongoDbTestFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(MongoDbTestFixture fixture)
    {
        _fixture = fixture;
        
        // Create WebApplicationFactory with MongoDB Testcontainer
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Override MongoDB configuration to use test container
                    services.Configure<DotNetMongoDbBackend.Configurations.MongoSettings>(options =>
                    {
                        options.ConnectionString = _fixture.ConnectionString;
                        options.Database = _fixture.DatabaseName;
                        options.Collections = new DotNetMongoDbBackend.Configurations.MongoSettings.CollectionNames
                        {
                            Pois = "pois"
                        };
                    });
                });
            });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreatePoi_EndToEnd_ShouldPersistToMongoDBContainer()
    {
        // Arrange
        var newPoi = new PointOfInterestDto
        {
            Name = "E2E Test POI",
            Details = "Created via HTTP API",
            Location = new LocationDto
            {
                Longitude = 13.7373,
                Latitude = 51.0504
            },
            Category = "Restaurant"
        };

        var json = JsonSerializer.Serialize(newPoi);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - POST to create POI
        var response = await _client.PostAsync("/zdi-geo-service/api/poi", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Verify Location header is present
        Assert.NotNull(response.Headers.Location);
        
        // GET the created POI via Location header
        var getResponse = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var createdPoi = JsonSerializer.Deserialize<PointOfInterestDto>(
            await getResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.NotNull(createdPoi);
        Assert.Equal("E2E Test POI", createdPoi.Name);
        // Note: API returns ID in Href and Location header, not in Id property
    }

    [Fact]
    public async Task GetPoiById_EndToEnd_ShouldRetrieveFromMongoDB()
    {
        // Arrange - Create POI first
        var newPoi = new PointOfInterestDto
        {
            Name = "Test Get By Id",
            Details = "For retrieval test",
            Location = new LocationDto { Longitude = 13.7, Latitude = 51.0 },
            Category = "Museum"
        };

        var json = JsonSerializer.Serialize(newPoi);
        var createResponse = await _client.PostAsync("/zdi-geo-service/api/poi", 
            new StringContent(json, Encoding.UTF8, "application/json"));
        
        Assert.NotNull(createResponse.Headers.Location);

        // Act - GET by ID using Location header
        var getResponse = await _client.GetAsync(createResponse.Headers.Location);

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var retrievedPoi = JsonSerializer.Deserialize<PointOfInterestDto>(
            await getResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.NotNull(retrievedPoi);
        Assert.Equal("Test Get By Id", retrievedPoi.Name);
        // Note: API returns ID in Href and Location header, not in Id property
    }

    [Fact]
    public async Task UpdatePoi_EndToEnd_ShouldModifyInMongoDB()
    {
        // Arrange - Create POI
        var newPoi = new PointOfInterestDto
        {
            Name = "Original Name",
            Details = "Original details",
            Location = new LocationDto { Longitude = 13.7, Latitude = 51.0 },
            Category = "Park"
        };

        var createResponse = await _client.PostAsync("/zdi-geo-service/api/poi",
            new StringContent(JsonSerializer.Serialize(newPoi), Encoding.UTF8, "application/json"));
        
        Assert.NotNull(createResponse.Headers.Location);
        
        // Extract ID from Location header
        var locationUri = createResponse.Headers.Location.ToString();
        var parts = locationUri.Split('/');
        var poiId = parts[parts.Length - 1];
        
        // GET the created POI
        var getResponse = await _client.GetAsync(createResponse.Headers.Location);
        var createdPoi = JsonSerializer.Deserialize<PointOfInterestDto>(
            await getResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        // Act - Update POI
        createdPoi!.Name = "Updated Name";
        createdPoi.Category = "Garden";
        
        var updateResponse = await _client.PutAsync($"/zdi-geo-service/api/poi/{poiId}",
            new StringContent(JsonSerializer.Serialize(createdPoi), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedPoi = JsonSerializer.Deserialize<PointOfInterestDto>(
            await updateResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.Equal("Updated Name", updatedPoi!.Name);
        Assert.Equal("Garden", updatedPoi.Category);
    }

    [Fact]
    public async Task DeletePoi_EndToEnd_ShouldRemoveFromMongoDB()
    {
        // Arrange - Create POI
        var newPoi = new PointOfInterestDto
        {
            Name = "To Be Deleted",
            Details = "Test details",
            Location = new LocationDto { Longitude = 13.7, Latitude = 51.0 },
            Category = "Test"
        };

        var createResponse = await _client.PostAsync("/zdi-geo-service/api/poi",
            new StringContent(JsonSerializer.Serialize(newPoi), Encoding.UTF8, "application/json"));
        
        Assert.NotNull(createResponse.Headers.Location);
        
        // Extract ID from Location header (e.g., /zdi-geo-service/api/poi/6916031307196fe3eeb92e2c)
        var locationUri = createResponse.Headers.Location.ToString();
        var parts = locationUri.Split('/');
        var poiId = parts[parts.Length - 1];

        // Act - Delete POI
        var deleteResponse = await _client.DeleteAsync($"/zdi-geo-service/api/poi/{poiId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        // Verify deletion
        var verifyResponse = await _client.GetAsync($"/zdi-geo-service/api/poi/{poiId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }

    [Fact]
    public async Task GetAllPois_EndToEnd_ShouldReturnFromMongoDB()
    {
        // Arrange - Clear and create test data
        await _fixture.ClearCollectionAsync();
        
        await _client.PostAsync("/zdi-geo-service/api/poi",
            new StringContent(JsonSerializer.Serialize(new PointOfInterestDto
            {
                Name = "POI 1",
                Details = "Test details",
                Location = new LocationDto { Longitude = 13.7, Latitude = 51.0 },
                Category = "Restaurant"
            }), Encoding.UTF8, "application/json"));

        // Act
        var response = await _client.GetAsync("/zdi-geo-service/api/poi");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var pois = JsonSerializer.Deserialize<PointOfInterestDto[]>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.NotNull(pois);
        Assert.NotEmpty(pois);
    }

    [Fact]
    public async Task GeoSearch_EndToEnd_ShouldUseMongoDBGeoIndex()
    {
        // Arrange - Create POIs at different locations
        await _fixture.ClearCollectionAsync();
        
        await _client.PostAsync("/zdi-geo-service/api/poi",
            new StringContent(JsonSerializer.Serialize(new PointOfInterestDto
            {
                Name = "Close POI",
                Details = "Test details",
                Location = new LocationDto { Longitude = 13.7373, Latitude = 51.0504 },
                Category = "Restaurant"
            }), Encoding.UTF8, "application/json"));
        
        await _client.PostAsync("/zdi-geo-service/api/poi",
            new StringContent(JsonSerializer.Serialize(new PointOfInterestDto
            {
                Name = "Far POI",
                Details = "Test details",
                Location = new LocationDto { Longitude = 12.3810, Latitude = 51.3455 },
                Category = "Museum"
            }), Encoding.UTF8, "application/json"));

        // Act - Search within 5km of Dresden (use lng and lat query parameters)
        var response = await _client.GetAsync("/zdi-geo-service/api/poi?lng=13.7373&lat=51.0504&radius=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var pois = JsonSerializer.Deserialize<PointOfInterestDto[]>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.NotNull(pois);
        Assert.Contains(pois, p => p.Name == "Close POI");
        Assert.DoesNotContain(pois, p => p.Name == "Far POI");
    }

    [Fact]
    public async Task GetCategories_EndToEnd_ShouldReturnUniqueCategories()
    {
        // Arrange
        await _fixture.ClearCollectionAsync();
        
        await _client.PostAsync("/zdi-geo-service/api/poi",
            new StringContent(JsonSerializer.Serialize(new PointOfInterestDto
            {
                Name = "Restaurant POI",
                Details = "Test details",
                Location = new LocationDto { Longitude = 13.7, Latitude = 51.0 },
                Category = "Restaurant"
            }), Encoding.UTF8, "application/json"));

        // Act
        var response = await _client.GetAsync("/zdi-geo-service/api/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var categories = JsonSerializer.Deserialize<string[]>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.NotNull(categories);
        Assert.Contains("Restaurant", categories);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
