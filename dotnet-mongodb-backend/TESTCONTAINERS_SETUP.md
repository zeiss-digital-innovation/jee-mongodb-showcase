# Testcontainers Setup für .NET MongoDB Backend

Dieses Dokument beschreibt die Einrichtung und Verwendung von Testcontainers für Integration Tests mit MongoDB.

## Übersicht

Testcontainers ermöglicht das Ausführen von Integration Tests gegen eine echte MongoDB-Instanz in einem Docker-Container. Dies eliminiert die Notwendigkeit von Mocks und bietet realistische Test-Szenarien.

## Voraussetzungen

- Docker Desktop (Windows) oder Docker Engine (Linux/macOS)
- .NET 9.0 SDK
- xUnit als Test-Framework

## Installation

### 1. NuGet-Pakete hinzufügen

Fügen Sie folgende Pakete zum Test-Projekt hinzu:

```xml
<PackageReference Include="Testcontainers" Version="3.10.0" />
<PackageReference Include="Testcontainers.MongoDb" Version="3.10.0" />
```

Oder via CLI:

```powershell
dotnet add package Testcontainers --version 3.10.0
dotnet add package Testcontainers.MongoDb --version 3.10.0
```

### 2. Test-Fixture erstellen

Erstellen Sie eine Fixture-Klasse zur Verwaltung des Container-Lebenszyklus:

```csharp
public class MongoDbTestFixture : IAsyncLifetime
{
    private MongoDbContainer _mongoContainer = null!;
    private string _testDatabaseName = string.Empty;

    public IMongoDatabase Database { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // 1. Container konfigurieren und starten
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:8.0")
            .WithPortBinding(27017, true) // Random host port
            .Build();

        await _mongoContainer.StartAsync();

        // 2. Eindeutige Test-Datenbank erstellen
        _testDatabaseName = $"test_db_{Guid.NewGuid():N}";
        var client = new MongoClient(_mongoContainer.GetConnectionString());
        Database = client.GetDatabase(_testDatabaseName);

        // 3. MongoDB 2dsphere Geo-Index erstellen
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

    // Optional: Collection zwischen Tests leeren
    public async Task ClearCollectionAsync()
    {
        var collection = Database.GetCollection<PointOfInterestEntity>("pointsOfInterest");
        await collection.DeleteManyAsync(Builders<PointOfInterestEntity>.Filter.Empty);
    }

    // MongoDB-Settings für Dependency Injection
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

### 3. Test-Klasse mit Fixture verwenden

#### Service Integration Tests

```csharp
public class PointOfInterestServiceIntegrationTests : IClassFixture<MongoDbTestFixture>
{
    private readonly MongoDbTestFixture _fixture;
    private readonly IPointOfInterestService _service;

    public PointOfInterestServiceIntegrationTests(MongoDbTestFixture fixture)
    {
        _fixture = fixture;
        
        // Service mit echter MongoDB initialisieren
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

#### API End-to-End Tests mit WebApplicationFactory

```csharp
public class ApiIntegrationTests : IClassFixture<MongoDbTestFixture>
{
    private readonly HttpClient _client;
    private readonly MongoDbTestFixture _fixture;

    public ApiIntegrationTests(MongoDbTestFixture fixture)
    {
        _fixture = fixture;

        // WebApplicationFactory mit Test-MongoDB konfigurieren
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Produktive MongoDB-Konfiguration entfernen
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IMongoClient));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Test-MongoDB einbinden
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

## Tests ausführen

```powershell
# Alle Integration Tests
dotnet test --filter "FullyQualifiedName~Integration"

# Spezifische Test-Klasse
dotnet test --filter "FullyQualifiedName~PointOfInterestServiceIntegrationTests"

# Mit verbosem Output
dotnet test --filter "FullyQualifiedName~Integration" --verbosity normal
```

## Best Practices

### 1. Test-Isolation

Jede Test-Klasse sollte eine eigene Fixture-Instanz erhalten:

```csharp
public class MyTests : IClassFixture<MongoDbTestFixture>
{
    // xUnit erstellt eine neue Fixture-Instanz pro Test-Klasse
    // → Parallele Ausführung mit isolierten Datenbanken
}
```

### 2. Cleanup zwischen Tests

```csharp
public async Task InitializeAsync()
{
    // Wird vor jedem Test ausgeführt
    await _fixture.ClearCollectionAsync();
}
```

### 3. GeoJSON-Format beachten

MongoDB erwartet Koordinaten im Format `[longitude, latitude]`:

```csharp
Coordinates = new[] { 13.7373, 51.0504 }  // ✅ Richtig: [lng, lat]
Coordinates = new[] { 51.0504, 13.7373 }  // ❌ Falsch: [lat, lng]
```

### 4. Container-Logs bei Fehlern

```csharp
public async Task InitializeAsync()
{
    await _mongoContainer.StartAsync();
    
    // Logs bei Problemen ausgeben
    var (stdout, stderr) = await _mongoContainer.GetLogsAsync();
    Console.WriteLine($"Container logs:\n{stdout}");
}
```

## Vorteile

- ✅ **Echte MongoDB-Features**: Geo-Spatial Queries, Aggregations, Indizes
- ✅ **Keine Mocks**: Testen gegen echte Datenbank-Implementierung
- ✅ **Isolation**: Jeder Test läuft in eigener Datenbank
- ✅ **Automatisches Cleanup**: Container werden nach Tests entfernt
- ✅ **CI/CD Ready**: Funktioniert überall wo Docker verfügbar ist
- ✅ **Parallele Ausführung**: Mehrere Test-Klassen gleichzeitig

## Performance

- **Erster Start**: ~3-5 Sekunden (Container-Download + Start)
- **Folgende Starts**: ~2-3 Sekunden (Image ist gecached)
- **36 Tests**: ~7-8 Sekunden Gesamtlaufzeit

## Troubleshooting

### Container startet nicht

```powershell
# Docker läuft?
docker ps

# Testcontainers-Logs aktivieren
$env:TESTCONTAINERS_RYUK_DISABLED="false"
dotnet test --verbosity normal
```

### Port-Konflikte

Testcontainers verwendet automatisch freie Ports. Bei Konflikten:

```csharp
.WithPortBinding(27017, true) // true = random host port
```

### Connection String

```csharp
// Richtiger Connection String vom Container holen
var connectionString = _mongoContainer.GetConnectionString();
// Format: mongodb://localhost:<random-port>
```

## Architektur

```
Test-Ausführung:
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

## Weiterführende Informationen

- [Testcontainers Dokumentation](https://dotnet.testcontainers.org/)
- [Testcontainers MongoDB Module](https://dotnet.testcontainers.org/modules/mongodb/)
- [MongoDB C# Driver](https://www.mongodb.com/docs/drivers/csharp/current/)
