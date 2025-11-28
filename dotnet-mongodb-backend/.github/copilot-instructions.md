# .NET MongoDB Backend - AI Coding Agent Guide

## Project Overview
This is a **high-performance REST API** built with **.NET 9 + ASP.NET Core** for managing Points of Interest (POIs) with MongoDB. The backend is **API-compatible** with legacy JEE/Spring Boot backends and designed to work with an Angular frontend.

**Base URL Pattern**: `http://localhost:8080/zdi-geo-service/api/poi`

## Architecture Pattern: Clean Architecture with Entity/DTO Separation

### Critical Three-Layer Pattern
1. **Entity Layer** (`Models/Entities/`) - MongoDB persistence models with `[BsonElement]` attributes
2. **DTO Layer** (`Models/DTOs/`) - REST API models with `[JsonPropertyName]` attributes and validation
3. **Mapper Layer** (`Mappers/`) - Static conversion methods between Entity ↔ DTO

**Rule**: Services work with `Entities`, Controllers work with `DTOs`. Always use `PointOfInterestMapper` to convert between layers.

```csharp
// Controller pattern (always DTO → Entity → DTO)
var entities = await _poiService.GetAllPoisAsync();  // Service returns Entities
var dtos = PointOfInterestMapper.ToDtoList(entities); // Convert to DTOs
return Ok(dtos);
```

## MongoDB Integration Specifics

### Singleton Service Registration
`PointOfInterestService` is registered as **Singleton** (not Scoped) in `Program.cs` to ensure MongoDB indexes are created **only once** at startup:

```csharp
builder.Services.AddSingleton<IPointOfInterestService, PointOfInterestService>();
```

### GeoJSON Location Format
MongoDB uses **GeoJSON Point format** with `[longitude, latitude]` order:

```csharp
public class LocationEntity {
    [BsonElement("type")] public string Type { get; set; } = "Point";
    [BsonElement("coordinates")] public double[] Coordinates { get; set; } // [lng, lat]
}
```

**Critical**: Always store as `[longitude, latitude]` in MongoDB, but the DTO exposes separate `Longitude` and `Latitude` properties for API consumers.

### Index Creation
The service automatically creates these indexes in the constructor:
- **2dsphere index** on `location` for geographic queries
- **Text index** on `name` and `tags` for full-text search
- **Category index** for category filtering

## Key Configuration Files

### appsettings.json
```json
{
  "ServiceBase": "zdi-geo-service",  // URL base path
  "MongoSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "demo-campus",
    "Collections": { "Pois": "point-of-interest" }
  }
}
```

### Program.cs Special Features
- **PathBase routing**: App uses `UsePathBase("/zdi-geo-service")` then maps controllers under `/api`
- **Forwarded headers**: Configured for reverse proxy support (X-Forwarded-For/Proto)
- **System.Text.Json**: Uses camelCase with null value ignoring (NOT Newtonsoft.Json for controllers)
- **Container detection**: Binds to `0.0.0.0:8080` in containers, `localhost:8080` locally

## API Endpoint Patterns

### Multiple Category Filtering
The API supports **multiple categories** via repeated query parameters:
```
GET /poi?category=restaurant&category=pharmacy
```

Controller uses `List<string>? category` parameter to handle this.

### Geographic Search Parameters
- `lat` and `lng` (or `lon`) for coordinates
- `radius` in **meters** (converted to kilometers internally)
- Backend uses MongoDB's `$near` operator with 2dsphere index

### Href Generation
DTOs include `Href` property (not stored in DB) generated via `LinkGenerator`:
```csharp
protected virtual void GenerateHref(PointOfInterestDto dto) {
    var uri = _linkGenerator?.GetUriByAction(HttpContext, 
        action: nameof(GetPoiById), values: new { id = dto.Id });
    dto.Href = uri;
}
```

## Testing Strategy

### Test Framework Stack
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **Testcontainers** - Spins up real MongoDB in Docker for integration tests
- **Microsoft.AspNetCore.Mvc.Testing** - WebApplicationFactory for integration tests

### Test Organization
```
tests/Tests/
├── *ControllerTests.cs        # Unit tests with mocked services
├── *IntegrationTests.cs       # End-to-end tests with Testcontainers
├── *ServiceTests.cs           # Service layer tests
└── Fixtures/MongoDbTestFixture.cs  # Shared MongoDB container setup
```

### Running Tests
```bash
dotnet test                                    # Run all tests
dotnet test --collect:"XPlat Code Coverage"   # With coverage
dotnet watch test                             # Watch mode
```

**Coverage targets**: 85%+ line coverage, 85%+ branch coverage

## Development Workflows

### Local Development
```bash
cd DotNetMongoDbBackend
dotnet restore
dotnet run              # Development mode with Swagger
dotnet watch run        # Hot reload enabled
```

### Docker Deployment Options
1. **Complete system** (includes MongoDB): `docker-compose up --build -d`
2. **Backend only** (external MongoDB): `docker-compose -f docker-compose.external-mongo.yml up --build -d`
3. **Smart deployment**: Use `deploy.bat` (Windows) or `deploy.sh` (Linux/macOS) - auto-detects MongoDB setup

### Important URLs
- API: http://localhost:8080/zdi-geo-service/api/poi
- Health: http://localhost:8080/zdi-geo-service/api/health
- Swagger: http://localhost:8080/zdi-geo-service/swagger (dev only)

## Common Patterns & Conventions

### Async/Await Everywhere
All data access methods use `async Task<T>` - never block on `.Result` or `.Wait()`.

### Validation Pattern
Use Data Annotations on DTOs:
```csharp
[Required(ErrorMessage = "Name is required!")]
[StringLength(200, MinimumLength = 1)]
[RegularExpression(@"^[^<>]*$", ErrorMessage = "No < or > characters")]
public string? Name { get; set; }
```

### Error Handling in Controllers
```csharp
try {
    var entity = await _service.GetPoiByIdAsync(id);
    return entity == null ? NotFound() : Ok(PointOfInterestMapper.ToDto(entity));
} catch (Exception ex) {
    _logger.LogError(ex, "Error retrieving POI {Id}", id);
    return StatusCode(500, new { error = "Internal server error" });
}
```

### MongoDB Filter Building
Use strongly-typed filter builders with Entity types:
```csharp
var filter = Builders<PointOfInterestEntity>.Filter.And(
    Builders<PointOfInterestEntity>.Filter.Eq(p => p.Category, category),
    Builders<PointOfInterestEntity>.Filter.Near(p => p.Location, lng, lat, radius)
);
```

## Project-Specific Quirks

1. **Port 8080 is fixed** - Both JEE and .NET backends use the same port for API compatibility
2. **German comments in models** - Legacy compatibility, keep existing German comments in Entity/DTO classes
3. **ServiceBase configuration** - All route configurations reference `ServiceBase` from appsettings.json
4. **No Entity Framework** - This project uses MongoDB.Driver directly, not EF Core
5. **Logging timestamp format** - Custom format `"dd.MM.yy HH:mm "` in console output

## When Adding New Features

1. **New entity field**: Add to both Entity class (BSON) and DTO class (JSON), update Mapper
2. **New endpoint**: Add to controller (DTOs), add business logic to service (Entities)
3. **New MongoDB query**: Add to service layer with proper index considerations
4. **New tests**: Add both unit tests (mocked) and integration tests (Testcontainers)
5. **Configuration changes**: Update `appsettings.json` and `MongoSettings.cs`

## API Compatibility Note
This backend maintains **API compatibility** with existing JEE/Spring Boot implementations. When modifying endpoints, ensure response formats match the Angular frontend expectations (camelCase JSON, specific field names like `href`, `_id`, etc.).
