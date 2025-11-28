# Copilot Instructions - MongoDB Showcase Project

## Project Overview
Multi-stack demo showcasing MongoDB geospatial capabilities with 3 backend options (.NET, JEE, Spring) and 2 frontend options (Angular, ASP.NET MVC). All components communicate through a common REST API contract for managing Points of Interest (POIs).

## Architecture Patterns

### Backend: Clean Architecture with Entity/DTO Separation
All backends follow the same pattern:
- **Entities** (`*Entity.cs`, `*Entity.java`): MongoDB persistence layer with BSON/Morphia annotations
- **DTOs** (`*Dto.cs`, `*Resource.java`): REST API layer with JSON annotations and validation
- **Mappers** (`*Mapper.cs/java`): Bidirectional static conversion methods between Entity ↔ DTO
- **Services**: Business logic operates on Entities, Controllers consume/produce DTOs

Example (.NET): `PointOfInterestEntity` (BSON) → `PointOfInterestMapper.ToDto()` → `PointOfInterestDto` (JSON)

### Frontend: Dual View with Synchronized State
ASP.NET MVC frontend (`dotnet-maps-frontend/`):
- **Map View** (`Views/Map/Index.cshtml`): Leaflet.js interactive map, right-click context menu for CRUD
- **List View** (`Views/PointOfInterest/Index.cshtml`): Card/Table toggle, sortable columns
- **State Sync**: localStorage keys (`poi_latitude`, `poi_longitude`, `poi_radius`, `poi_map_zoom`) persist across pages
- **Filter Architecture**: 
  - Text filters (name/details) = client-side JavaScript (`site.js::applyFilters()`)
  - Category filter = backend query parameter
  - Rebuild pattern: Filter mutates `globalThis.currentTablePois` array, then calls `renderTableHtml()` to rebuild DOM (avoids CSS `:nth-child()` striping issues)

### MongoDB GeoJSON Schema
All backends use identical GeoJSON Point format:
```json
{
  "location": {
    "type": "Point",
    "coordinates": [longitude, latitude]  // ⚠️ Order matters: [lon, lat]
  }
}
```
- **2dsphere index** required for geospatial queries (`$near`, `$geoWithin`)
- Haversine distance calculations in service layer

## Critical Workflows

### Running the Stack
1. **Start MongoDB**: `cd MongoDB && docker-compose up -d` (creates `demo-campus` DB with 2dsphere indexes)
2. **Choose Backend** (runs on `localhost:8080/zdi-geo-service/api/`):
   - .NET: `cd dotnet-mongodb-backend/DotNetMongoDbBackend && dotnet run`
   - JEE: `cd jee-mongodb-backend && mvn clean package` (deploy WAR to server)
   - Spring: `cd spring-mongodb-backend && mvn spring-boot:run`
3. **Choose Frontend**:
   - ASP.NET MVC: `cd dotnet-maps-frontend && dotnet run` → `http://localhost:4200/Map`
   - Angular: `cd angular-maps-frontend && npm start` → `http://localhost:4200`

### Testing Strategy
- **Unit Tests**: Mock-based, test Mappers and business logic in isolation
- **Integration Tests**: Use **Testcontainers** (`MongoDbBuilder()` in .NET, `@Testcontainers` in Java) to spin up real MongoDB containers
  - Pattern: `MongoDbTestFixture` (xUnit) or `MongoDBContainer` (JUnit) with `@BeforeEach` cleanup
  - Tests skip automatically if Docker unavailable (`Skip.IfNot(_fixture.IsDockerAvailable)`)
- **Coverage**: Run with `dotnet test --collect:"XPlat Code Coverage"` or `mvn test jacoco:report`

### Data Validation Rules
**Backend** (enforce at DTO level with Data Annotations):
```csharp
[Required][StringLength(200)][RegularExpression(@"^[^<>]*$")]  // Name
[Required][StringLength(1000)][RegularExpression(@"^[^<>]*$")] // Details
[Range(-180, 180)] Longitude, [Range(-90, 90)] Latitude
```
**Frontend** (client-side validation in `savePoi()` functions):
```javascript
if (/<script|javascript:|onerror=/i.test(name)) { alert('Invalid characters'); return; }
```
**⚠️ Never HTML-encode before sending to API** - JSON serialization handles escaping, encoding causes double-encoding bugs (e.g., `Ä` → `&Auml;`)

## Project-Specific Conventions

### Categories System
- **Source of truth**: `CategoryConstants.cs` / `CategoryConstants.java` (17 predefined categories: restaurant, pharmacy, museum, etc.)
- **Always lowercase**: Category values stored/compared in lowercase across all layers
- **Icon mapping**: Bootstrap Icons (`bi-cup-hot`, `bi-bank`) defined in constants, synced to JavaScript via `@Html.Raw(JsonSerializer.Serialize())`

### API Compatibility Rules
JEE backend is reference implementation - maintain compatibility:
- `POST /poi` returns `201 Created` with `Location` header, **no body**
- `PUT /poi/{id}` returns `204 No Content` (not the updated POI)
- `DELETE /poi/{id}` returns `204 No Content` (idempotent, even if POI doesn't exist)
- Query params: `lat`, `lon` (not `lng`), `radius` (meters), `category[]` (array notation for ASP.NET Core)

### JavaScript Patterns
- **Global scope**: Use `globalThis` (not `window`) for cross-environment compatibility
- **Direct undefined checks**: `globalThis.allTablePois !== undefined` (not `typeof`)
- **LocalStorage sync**: All map state (coordinates, zoom, filters) persists in localStorage with prefix `poi_*`
- **Table striping fix**: Bootstrap `table-striped` breaks with `display:none` - solution is filter array + rebuild DOM, not CSS manipulation

### Exception Handling (.NET)
Always log with structured parameters before rethrowing:
```csharp
catch (HttpRequestException ex) {
    _logger.LogError(ex, "HTTP error while creating POI for category {Category}", poi.Category);
    throw new InvalidOperationException($"Failed due to HTTP error: {ex.Message}", ex);
}
```
Never catch without logging or rethrow without preserving inner exception.

## Key Files Reference
- API Contract: `dotnet-mongodb-backend/Controllers/PointOfInterestController.cs` (lines 198-280 POST/PUT logic)
- Mapper Pattern: `dotnet-mongodb-backend/Mappers/PointOfInterestMapper.cs`
- Frontend State Sync: `dotnet-maps-frontend/Views/Map/Index.cshtml` (lines 180-250 localStorage handlers)
- Test Setup: `dotnet-mongodb-backend/tests/Tests/Fixtures/MongoDbTestFixture.cs`
- Category System: `dotnet-maps-frontend/Constants/CategoryConstants.cs`

## Common Pitfalls
1. **Coordinate order**: Always `[longitude, latitude]` in MongoDB - reversed from typical lat/lon usage
2. **HTML entities**: Don't use `$('<div>').text(x).html()` pattern - causes double-encoding (old bug in `Map/Index.cshtml`)
3. **Filter rebuild**: Hiding rows with CSS breaks Bootstrap striping - must rebuild table HTML instead
4. **Testcontainers timeout**: Set `TESTCONTAINERS_RYUK_DISABLED=true` env var if cleanup hangs on Windows
5. **HTTPS warnings**: Set `Server.UseHttps: false` in `appsettings.json` to disable HTTPS redirection for local dev


## Language
chat language is always german, language of documentation and code comments is always english

