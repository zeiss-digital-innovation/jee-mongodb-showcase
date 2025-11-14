# Testcontainers Setup for .NET MongoDB Backend

This document describes the setup and usage of Testcontainers for integration tests with MongoDB.

## Overview

Testcontainers enables running integration tests against a real MongoDB instance in a Docker container. This eliminates the need for mocks and provides realistic test scenarios.

## Prerequisites

- Docker Desktop (Windows) or Docker Engine (Linux/macOS)
- .NET 9.0 SDK
- xUnit as test framework

## Installation

### 1. Add NuGet Packages

Add the following packages to your test project:

```xml
<PackageReference Include="Testcontainers" Version="3.10.0" />
<PackageReference Include="Testcontainers.MongoDb" Version="3.10.0" />
```

Oder via CLI:

```powershell
dotnet add package Testcontainers --version 3.10.0
dotnet add package Testcontainers.MongoDb --version 3.10.0
```

### 2. Create Test Fixture

Create a fixture class to manage the container lifecycle:

```csharp
public class MongoDbTestFixture : IAsyncLifetime
{
    private MongoDbContainer _mongoContainer = null!;
    private string _testDatabaseName = string.Empty;

    public IMongoDatabase Database { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // 1. Configure and start container
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:8.0")
            .WithPortBinding(27017, true) // Random host port
            .Build();

        await _mongoContainer.StartAsync();

        // 2. Create unique test database
        _testDatabaseName = $"test_db_{Guid.NewGuid():N}";
        var client = new MongoClient(_mongoContainer.GetConnectionString());
        Database = client.GetDatabase(_testDatabaseName);

        // 3. Create MongoDB 2dsphere geo index
        var collection = Database.GetCollection<PointOfInterestEntity>("pointsOfInterest");
        var indexKeys = Builders<PointOfInterestEntity>.IndexKeys
            .Geo2DSphere(poi => poi.Location!.Coordinates);
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<PointOfInterestEntity>(indexKeys)
        );
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }

    // Optional: Clear collection between tests
    public async Task ClearCollectionAsync()
    {
        var collection = Database.GetCollection<PointOfInterestEntity>("pointsOfInterest");
        await collection.DeleteManyAsync(Builders<PointOfInterestEntity>.Filter.Empty);
    }

    // MongoDB settings for dependency injection
    public MongoSettings GetMongoSettings()
    {
        return new MongoSettings
        {
            ConnectionString = _mongoContainer.GetConnectionString(),
            DatabaseName = _testDatabaseName
        };
    }
}
```

### 3. Use Test Fixture in Test Class

#### Service Integration Tests

```csharp
public class PointOfInterestServiceIntegrationTests : IClassFixture<MongoDbTestFixture>
{
    private readonly MongoDbTestFixture _fixture;
    private readonly IPointOfInterestService _service;

    public PointOfInterestServiceIntegrationTests(MongoDbTestFixture fixture)
    {
        _fixture = fixture;
        
        // Initialize service with real MongoDB
        var mongoSettings = Options.Create(_fixture.GetMongoSettings());
        var client = new MongoClient(_fixture.GetMongoSettings().ConnectionString);
        var logger = new NullLogger<PointOfInterestService>();
        
        _service = new PointOfInterestService(client, mongoSettings, logger);
    }

    [Fact]
    public async Task CreatePoi_ShouldPersistToMongoDB()
    {
        // Arrange
        var poi = new PointOfInterestEntity
        {
            Name = "Test Restaurant",
            Category = "restaurant",
            Details = "A test restaurant",
            Location = new LocationEntity
            {
                Type = "Point",
                Coordinates = new[] { 13.7373, 51.0504 } // [longitude, latitude]
            }
        };

        // Act
        var created = await _service.CreatePoiAsync(poi);

        // Assert
        Assert.NotNull(created.Id);
        
        // Verify in database
        var fromDb = await _service.GetPoiByIdAsync(created.Id!);
        Assert.NotNull(fromDb);
        Assert.Equal("Test Restaurant", fromDb.Name);
    }
}
```

#### API End-to-End Tests with WebApplicationFactory

```csharp
public class ApiIntegrationTests : IClassFixture<MongoDbTestFixture>
{
    private readonly HttpClient _client;
    private readonly MongoDbTestFixture _fixture;

    public ApiIntegrationTests(MongoDbTestFixture fixture)
    {
        _fixture = fixture;

        // Configure WebApplicationFactory with test MongoDB
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove production MongoDB configuration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IMongoClient));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add test MongoDB
                    services.AddSingleton<IMongoClient>(sp =>
                        new MongoClient(_fixture.GetMongoSettings().ConnectionString));
                    
                    services.Configure<MongoSettings>(options =>
                    {
                        options.ConnectionString = _fixture.GetMongoSettings().ConnectionString;
                        options.DatabaseName = _fixture.GetMongoSettings().DatabaseName;
                    });
                });
            });

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_CreatePoi_ReturnsCreatedWithLocation()
    {
        // Arrange
        var newPoi = new
        {
            name = "API Test POI",
            category = "museum",
            details = "Created via API",
            location = new
            {
                type = "Point",
                coordinates = new[] { 13.7373, 51.0504 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/poi", newPoi);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }
}
```

## Running Tests

```powershell
# All integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Specific test class
dotnet test --filter "FullyQualifiedName~PointOfInterestServiceIntegrationTests"

# With verbose output
dotnet test --filter "FullyQualifiedName~Integration" --verbosity normal
```

## Best Practices

### 1. Test Isolation

Each test class should receive its own fixture instance:

```csharp
public class MyTests : IClassFixture<MongoDbTestFixture>
{
    // xUnit creates a new fixture instance per test class
    // → Parallel execution with isolated databases
}
```

### 2. Cleanup Between Tests

```csharp
public async Task InitializeAsync()
{
    // Executed before each test
    await _fixture.ClearCollectionAsync();
}
```

### 3. GeoJSON Format

MongoDB expects coordinates in `[longitude, latitude]` format:

```csharp
Coordinates = new[] { 13.7373, 51.0504 }  // ✅ Correct: [lng, lat]
Coordinates = new[] { 51.0504, 13.7373 }  // ❌ Wrong: [lat, lng]
```

### 4. Container Logs on Errors

```csharp
public async Task InitializeAsync()
{
    await _mongoContainer.StartAsync();
    
    // Output logs on errors
    var (stdout, stderr) = await _mongoContainer.GetLogsAsync();
    Console.WriteLine($"Container logs:\n{stdout}");
}
```

## Advantages

- ✅ **Real MongoDB Features**: Geo-spatial queries, aggregations, indexes
- ✅ **No Mocks**: Testing against real database implementation
- ✅ **Isolation**: Each test runs in its own database
- ✅ **Automatic Cleanup**: Containers are removed after tests
- ✅ **CI/CD Ready**: Works everywhere Docker is available
- ✅ **Parallel Execution**: Multiple test classes run simultaneously

## Performance

- **First Start**: ~3-5 seconds (container download + start)
- **Subsequent Starts**: ~2-3 seconds (image is cached)
- **36 Tests**: ~7-8 seconds total runtime

## Troubleshooting

### Container Won't Start

```powershell
# Is Docker running?
docker ps

# Enable Testcontainers logs
$env:TESTCONTAINERS_RYUK_DISABLED="false"
dotnet test --verbosity normal
```

### Port Conflicts

Testcontainers automatically uses free ports. In case of conflicts:

```csharp
.WithPortBinding(27017, true) // true = random host port
```

### Connection String

```csharp
// Get correct connection string from container
var connectionString = _mongoContainer.GetConnectionString();
// Format: mongodb://localhost:<random-port>
```

## Architecture

```
Test Execution:
┌─────────────────────────────────────────┐
│ xUnit Test Runner                       │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ MongoDbTestFixture (IAsyncLife..) │ │
│  │  ├─ StartAsync()                  │ │
│  │  ├─ MongoDB Container (mongo:8.0) │ │
│  │  │   Port: 27017 → Random         │ │
│  │  └─ DisposeAsync()                │ │
│  └───────────────────────────────────┘ │
│           ↓                             │
│  ┌───────────────────────────────────┐ │
│  │ Integration Tests                 │ │
│  │  ├─ Service Tests → MongoDB       │ │
│  │  └─ API Tests → Controller → DB   │ │
│  └───────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## Further Information

- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [Testcontainers MongoDB Module](https://dotnet.testcontainers.org/modules/mongodb/)
- [MongoDB C# Driver](https://www.mongodb.com/docs/drivers/csharp/current/)
