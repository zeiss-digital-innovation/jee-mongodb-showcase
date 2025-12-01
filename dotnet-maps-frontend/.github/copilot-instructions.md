# ASP.NET Core Maps Frontend - AI Developer Guide

## Project Overview
ASP.NET Core 9.0 MVC application providing interactive Point of Interest (POI) mapping with Leaflet.js. Built as a .NET alternative to an Angular reference implementation, maintaining full compatibility with a JEE backend service.

## Architecture & Data Flow

### Service Layer Pattern
- **Single source of truth**: `PointOfInterestService` handles all backend communication
- **Mock data fallback**: Service automatically uses mock data when backend URL is unconfigured
- **Category normalization**: All categories converted to lowercase for consistency with JEE backend
- **JEE compatibility quirks**: PUT requests return `204 No Content` (not the updated object), GET requests require `?expand=details` parameter

### State Management via sessionStorage
Critical pattern for cross-page synchronization (Map ↔ List views):
- **Storage keys** defined in JavaScript constants: `poi_latitude`, `poi_longitude`, `poi_radius`, `poi_mapZoom`, `poi_filter_text`, `poi_filter_category`
- **Session timeout**: 30-minute inactivity clears all session data (see `STORAGE_KEYS.sessionStart`)
- **Navigation sync**: `pageshow` and `focus` events reload state from sessionStorage
- **Implementation**: See `Views/Map/Index.cshtml` lines 125-255 for canonical pattern

### Controller Conventions
- **PointOfInterestController**: Handles both MVC views (`/poi`, `/PointOfInterest`) and REST API (`/api/pointsofinterest`)
- **MapController**: Serves interactive map view (`/Map`)
- **Default coordinates**: Dresden, Germany (51.0504, 13.7373) with 2000m radius - defined in `Constants/MapDefaults.cs`
- **Category filtering**: Use repeated query parameters: `?category=bank&category=restaurant` (JEE backend pattern)

### View Architecture
- **Partial views for DRY**: `_PoiControls.cshtml` (lat/lon/radius controls), `_PoiFilter.cshtml` (filter UI)
- **Global usings**: `GlobalUsings.cs` includes `DotNetMapsFrontend.Services` and `Models` automatically
- **Bootstrap 5** + Bootstrap Icons for UI components

## Development Workflows

### Running the Application
```powershell
dotnet run --project DotNetMapsFrontend.csproj
# Default: http://localhost:4200 (matches Angular frontend port)
```

### Configuration
Edit `appsettings.json`:
- **Backend URL**: `PointOfInterestApi:BaseUrl` (defaults to JEE service at `:8080/zdi-geo-service/api`)
- **HTTPS toggle**: `Server:UseHttps` (false = HTTP only, no redirection/warnings)
- **Ports**: `Server:HttpPort` (4200), `Server:HttpsPort` (7225)

### Testing
**Primary method**: Use `run-tests.ps1` script (not `dotnet test` directly)
```powershell
.\run-tests.ps1
# Runs: build → unit tests → code coverage → generates HTML report
```

**Test structure**:
- All tests in `DotNetMapsFrontend.Tests/` using **NUnit** (not xUnit)
- **Moq** for service mocking
- **Microsoft.AspNetCore.Mvc.Testing** for integration tests
- **Coverage reports**: Generated to `Tests/CoverageReport/index.html` via ReportGenerator

**Key test patterns**:
```csharp
// Setup pattern (NUnit, not xUnit)
[SetUp]
public void SetUp() { 
    _mockService = new Mock<IPointOfInterestService>();
}

// Cleanup pattern - always dispose controllers
[TearDown]
public void TearDown() {
    _controller?.Dispose();
}
```

### Build & Deployment
```powershell
dotnet build --configuration Release
dotnet publish --configuration Release -o ./publish
```

## Project-Specific Patterns

### Category Management
- **Predefined categories**: 17 default categories in `Constants/CategoryConstants.cs`
- **Icon mapping**: `GetCategoryIcon()` maps categories to Bootstrap icon classes
- **Always lowercase**: Categories normalized in service layer before returning to views
- **Fallback icon**: `bi-geo` for unknown categories

### API Response Handling
```csharp
// Standard pattern for controller actions
try {
    var result = await _poiService.SomeOperation();
    return Json(result);
} catch (Exception ex) {
    return Json(new { error = ex.Message });
}
```

### Coordinate Format
- **GeoJSON standard**: Location coordinates as `[longitude, latitude]` (note order!)
- **Model properties**: `Location.Longitude` and `Location.Latitude` provide convenient access
- **Invariant culture**: Always use `CultureInfo.InvariantCulture` for lat/lon formatting

### Logging Configuration
Custom console logging format in `Program.cs`:
```csharp
// Timestamp format: "dd.MM.yyyy HH:mm:ss "
builder.Logging.AddSimpleConsole(options => {
    options.TimestampFormat = "dd.MM.yyyy HH:mm:ss ";
});
```

## Integration Points

### Backend API Contract
**Base URL**: `{baseUrl}/poi`
- **GET** `?lat={lat}&lon={lon}&radius={radius}&category={cat}&expand=details` - List POIs
- **GET** `/{id}` - Get single POI
- **POST** - Create POI
- **PUT** `/{id}` - Update POI (returns 204 No Content, not object)
- **DELETE** `/{id}` - Delete POI

### Frontend JavaScript Integration
- **Leaflet.js**: Map rendering with OpenStreetMap tiles
- **jQuery**: AJAX calls to controller API endpoints
- **Bootstrap modals**: POI create/edit/delete dialogs
- **Right-click context menu**: Custom implementation for map marker interactions

## Common Pitfalls

1. **PUT response handling**: JEE backend returns 204, not the updated object - refresh data separately
2. **Category case sensitivity**: Always normalize to lowercase before comparisons
3. **Coordinate order**: GeoJSON uses [lon, lat], not [lat, lon]
4. **Session timeout**: 30 minutes - implement activity tracking when adding features
5. **Test disposal**: Always dispose controllers in `[TearDown]` to prevent resource leaks
6. **Mock data**: Service returns mock data silently if backend URL is empty - check logs

## Key Files Reference
- `Services/PointOfInterestService.cs` (481 lines) - All backend communication logic
- `Views/Map/Index.cshtml` (938 lines) - Complete map UI with sessionStorage patterns
- `Constants/CategoryConstants.cs` - Category definitions and icon mappings
- `run-tests.ps1` - Comprehensive test execution workflow
- `Program.cs` - Application bootstrap with HTTPS configuration
