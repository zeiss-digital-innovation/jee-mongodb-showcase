# ASP.NET Core Maps Frontend

This is an ASP.NET Core MVC application that provides the same functionality as the Angular frontend for displaying Points of Interest on an interactive map.

## Features

- **Interactive Map View**: Uses Leaflet.js to display POIs on an OpenStreetMap
- **POI Creation**: Create new POIs directly on the map by clicking
- **POI Editing**: Edit existing POI categories and details with real-time validation
- **POI Deletion**: Delete POIs with confirmation dialog
- **Right-Click Context Menu**: Edit and delete POIs directly from map markers
- **List View**: Displays POIs in card and table views with toggle
- **POI Filter**: Real-time text-based filtering across all views (cards, table, map markers)
  - **Name Filter**: Filter by POI name
  - **Details Filter**: Filter by POI details/description
  - **Category Filter**: Filter by category using dropdown
- **Filter Synchronization**: Filter value synchronized between Map and List pages via localStorage
- **Zoom Persistence**: Map zoom level persists when navigating between pages
- **Synchronized Controls**: Latitude, Longitude, and Radius controls synchronized between Map and List pages via localStorage
- **Fixed Headers**: Scrollable content with pinned headers and controls for better UX
- **Category Display**: All categories displayed in lowercase for consistency
- **Bootstrap Navigation**: Clean navigation between map and list views
- **REST API Integration**: Fetches POI data from the backend service
- **JEE Backend Compatibility**: Full compatibility with JEE reference implementation (PUT returns 204 No Content)
- **Responsive Design**: Works on desktop and mobile devices
- **Mock Data Fallback**: Uses mock data when backend is unavailable
- **Partial Views**: Reusable UI components for maintainability (DRY principle)

## Technology Stack

- **ASP.NET Core 9.0 MVC**
- **Leaflet.js** - Interactive mapping
- **Bootstrap 5** - UI components and responsive design
- **Bootstrap Icons** - Category icons
- **jQuery** - DOM manipulation and AJAX calls

## Project Structure

```
├── Controllers/
│   ├── MapController.cs           # Handles map view and API calls
│   ├── PointOfInterestController.cs # Handles list view
│   └── HomeController.cs          # Default home page
├── Models/
│   └── PointOfInterest.cs         # POI data models
├── Services/
│   └── PointOfInterestService.cs  # API service for backend calls
├── Views/
│   ├── Map/
│   │   └── Index.cshtml           # Interactive map view with filter
│   ├── PointOfInterest/
│   │   └── Index.cshtml           # List view with cards/table and filter
│   ├── Home/
│   │   └── Index.cshtml           # Welcome page
│   └── Shared/
│       ├── _Layout.cshtml         # Common layout with navigation
│       ├── _PoiControls.cshtml    # Reusable Lat/Lon/Radius controls
│       └── _PoiFilter.cshtml      # Reusable filter input (prepared)
├── DotNetMapsFrontend.Tests/      # Unit tests
│   ├── PointOfInterestControllerTests.cs
│   ├── PointOfInterestFilterTests.cs  # 24 filter & zoom tests
│   └── ...
└── wwwroot/                       # Static files (CSS, JS, images)
```

## Configuration

The application can be configured via `appsettings.json`:

```json
{
  "PointOfInterestApi": {
    "BaseUrl": "http://localhost:8080/zdi-geo-service/api"
  },
  "Server": {
    "UseHttps": false,
    "HttpPort": 4200,
    "HttpsPort": 7225
  }
}
```

### Server Configuration

**HTTPS Support**: The application supports both HTTP and HTTPS modes:

- **`UseHttps`** (boolean, default: `false`):
  - `false`: Run on HTTP only, no HTTPS redirection, no warnings
  - `true`: Enable HTTPS with automatic HTTP to HTTPS redirection
  
- **`HttpPort`** (integer, default: `4200`):
  - Port for HTTP traffic (matches Angular frontend port)
  
- **`HttpsPort`** (integer, default: `7225`):
  - Port for HTTPS traffic (only used when `UseHttps` is `true`)

**Examples:**

HTTP Only (Default):
```json
{
  "Server": {
    "UseHttps": false,
    "HttpPort": 4200
  }
}
```
→ Application runs on: `http://localhost:4200`

HTTPS Enabled:
```json
{
  "Server": {
    "UseHttps": true,
    "HttpPort": 4200,
    "HttpsPort": 7225
  }
}
```
→ Application runs on: `https://localhost:7225` (HTTP on 4200 redirects to HTTPS)

**Development Certificate**: For HTTPS in development, ensure you have a valid development certificate:
```powershell
dotnet dev-certs https --trust
```

**Backend Integration**: Connects to MongoDB .NET/JEE Backend on `/zdi-geo-service/api/poi` endpoint.

## Running the Application

1. **Prerequisites**: 
   - .NET 9.0 SDK
   - Backend service running (JEE or .NET MongoDB backend)

2. **Start the application**:
   ```bash
   dotnet run
   ```

3. **Access the application**:
   - Map View: `http://localhost:4200/Map` (same port as Angular frontend)
   - List View: `http://localhost:4200/poi
   - List View with Parameters: `http://localhost:4200/poi?lat=51.0504&lon=13.7373&radius=3900`
   - Home: `http://localhost:4200/` (redirects to Map)
   - API Endpoint: `http://localhost:4200/api/poi`
   - API Endpoint with Parameters: `http://localhost:4200/api/poi?lat=51.0504&lon=13.7373&radius=2000`

## API Integration

The application connects to the MongoDB backend REST API with optimized parameter usage:

- **Backend Endpoint**: `http://localhost:8080/zdi-geo-service/api/poi` (MongoDB .NET/JEE Backend)
- **API Parameters**: 
  - `lat` - Latitude coordinate (required for location-based search)
  - `lon` - Longitude coordinate (required for location-based search)  
  - `radius` - Search radius in meters (optimizes performance)
  - `expand=details` - Include detailed POI information
- **Configuration**: Set via `PointOfInterestApi:BaseUrl` in appsettings.json
- **Performance Optimization**: Uses zoom-level based radius (1000-20000 meters)
- **Fallback**: Automatically uses mock data when external API is unavailable

### Radius Logic (Zoom-based):
- **Zoom ≤ 8**: 20,000m radius (city level)
- **Zoom ≤ 11**: 10,000m radius (district level)
- **Zoom = 12**: 5,000m radius (neighborhood level)
- **Zoom = 13**: 2,000m radius (street level)
- **Zoom ≥ 14**: 1,000m radius (detailed view)

### Backend Compatibility:
- ✅ **MongoDB .NET Backend**: Full parameter support (`/zdi-geo-service/api/poi?lat=X&lon=Y&radius=Z&expand=details`)
- ✅ **JEE Backend**: Full parameter support (`/zdi-geo-service/api/poi?lat=X&lon=Y&radius=Z&expand=details`)

### Category Loading with Fallback:
The frontend attempts to load available categories from the backend at startup:
- **Success**: Uses categories provided by backend via `/categories` endpoint
- **Backend has no /categories endpoint** (e.g., JEE-Backend): Uses **DEFAULT_CATEGORIES** constant as fallback
- **Fallback Categories**: `landmark`, `museum`, `castle`, `cathedral`, `park`, `restaurant`, `hotel`, `gasstation`, `hospital`, `pharmacy`, `shop`, `bank`, `school`, `library`, `theater`
- **Compatibility**: Ensures the application works with older backend versions (JEE reference implementation)

### Performance Benefits:
- **Reduced Data Transfer**: Only loads POIs within visible area
- **Faster Response**: Server-side filtering by location  
- **Optimized UX**: Dynamic loading based on map movement and zoom
- **Coordinate Precision**: Uses proper decimal formatting with InvariantCulture

### Example API Calls:
```
GET /api/pointsofinterest?lat=51.0504&lon=13.7373&radius=2000
# Returns ~177 POIs for Dresden with 2km radius

GET /api/pointsofinterest?lat=51.0504&lon=13.7373&radius=1000  
# Returns ~88 POIs for Dresden with 1km radius
```

## Features Comparison with Angular Frontend

| Feature | Angular Frontend | ASP.NET Frontend | Status |
|---------|------------------|------------------|--------|
| Interactive Map | ✅ Leaflet.js | ✅ Leaflet.js | ✅ Implemented |
| POI Markers | ✅ | ✅ | ✅ Implemented |
| POI Creation | ✅ Map Page | ✅ Map Page | ✅ Implemented |
| POI Editing | ✅ List Page | ✅ List & Map Pages | ✅ Implemented |
| POI Deletion | ✅ List Page | ✅ List & Map Pages | ✅ Implemented |
| Right-Click Context Menu | ❌ | ✅ Map Markers | ✅ Implemented |
| Map Movement API Calls | ✅ | ✅ | ✅ Implemented |
| List View (Table) | ✅ | ✅ | ✅ Implemented |
| List View (Cards) | ❌ | ✅ | ✅ Implemented |
| View Toggle (Cards/Table) | ❌ | ✅ | ✅ Implemented |
| Fixed Headers & Scrolling | ✅ | ✅ | ✅ Implemented |
| Synchronized Controls | ✅ | ✅ localStorage | ✅ Implemented |
| Query Parameters | ❌ | ✅ URL Parameters | ✅ Implemented |
| Bootstrap Navigation | ✅ | ✅ | ✅ Implemented |
| Category Icons | ✅ Bootstrap Icons | ✅ Bootstrap Icons | ✅ Implemented |
| Category Format | ✅ TitleCase | ✅ lowercase | ✅ Implemented |
| Responsive Design | ✅ | ✅ | ✅ Implemented |
| Mock Data Fallback | ✅ | ✅ | ✅ Implemented |
| **POI Name Filter** | ✅ | ✅ All Views | ✅ **Implemented** |
| **POI Details Filter** | ❌ | ✅ All Views | ✅ **Implemented** |
| **Category Filter** | ✅ | ✅ Dropdown | ✅ **Implemented** |
| **Filter Synchronization** | ✅ | ✅ localStorage | ✅ **Implemented** |
| **Zoom Persistence** | ❌ | ✅ localStorage | ✅ **Implemented** |
| **Partial Views (DRY)** | ❌ | ✅ Reusable Components | ✅ **Implemented** |
| **JEE Backend Compat** | ✅ | ✅ PUT 204 No Content | ✅ **Implemented** |
| **138 Unit Tests** | ❌ | ✅ 100% Pass Rate | ✅ **Implemented** |

## Development Notes

- The application uses the same Leaflet.js library as the Angular version for consistency
- Bootstrap 5 and Bootstrap Icons provide the same visual styling  
- Uses IHttpClientFactory for efficient HTTP client management
- Implements structured logging with ILogger and custom timestamp format (dd.MM.yyyy HH:mm:ss)
- Coordinate precision using InvariantCulture for decimal formatting
- Mock data automatically loads when external API is unavailable
- Service layer uses dependency injection for testability
- Follows ASP.NET Core MVC patterns with separation of concerns
- Performance-optimized API calls with zoom-level based radius parameters

## Testing

The project includes comprehensive unit tests using NUnit, Moq, and AngleSharp:

```bash
cd DotNetMapsFrontend.Tests
dotnet test
```

### Test Coverage

✅ **138 Unit Tests** - All passing (100% pass rate)
- **62 Controller Tests**: CRUD operations, validation, error handling
- **12 Integration Tests**: End-to-end workflow testing
- **18 Filter Tests**: Client-side filtering functionality
- **24 Service Tests**: API integration and mock data
- **22 Model & UI Tests**: Data models and view rendering

### Test Files

- `PointOfInterestControllerTests.cs`: Controller logic and API integration tests
- `PointOfInterestControllerCrudTests.cs`: CRUD operation tests with validation
- `PointOfInterestFilterTests.cs`: Filter UI tests for Map and List pages
  - JavaScript inclusion verification
  - Category dropdown presence
  - Name and Details filter inputs
  - HTTP status validation
- `PointOfInterestServiceTests.cs`: Service layer and backend communication
- `ComprehensiveTests.cs`: End-to-end integration tests
- `PointOfInterestEditTests.cs`: POI editing workflow tests

### Testing Technologies

- **NUnit 4.6.0**: Testing framework
- **Moq**: Service mocking for dependency injection
- **AngleSharp**: HTML parsing and DOM manipulation
- **WebApplicationFactory**: Integration testing with in-memory server

The project follows testability best practices:
- Service layer separated for easy unit testing
- Dependency injection for mocking services
- Controller logic isolated from business logic
- Mock data available for integration testing
- HTML content validation with AngleSharp

## Current Status

✅ **Fully Functional** - All features implemented and working
- Interactive map with POI markers from MongoDB backend
- **Full CRUD Operations**: Create, Read, Update, Delete POIs
- **Right-Click Context Menu**: Edit and delete POIs directly from map markers
- **Edit & Delete**: Real-time validation, change detection, confirmation dialogs
- **Dual List Views**: Card view and Table view with toggle
- **Advanced Filtering**: 
  - Name filter (real-time search)
  - Details filter (real-time search)
  - Category dropdown filter
  - All filters work across cards, table, and map markers
- **Filter Synchronization**: Filter values persist between Map and List pages
- **Zoom Persistence**: Map zoom level persists across page navigation
- **Fixed Layout**: Scrollable content with pinned headers for better UX
- **Synchronized Settings**: Lat/Lon/Radius synchronized between pages via localStorage
- **URL Parameters**: Support for `?lat=X&lon=Y&radius=Z` query parameters
- **Category Normalization**: All categories displayed in lowercase
- **Partial Views**: Reusable UI components (`_PoiControls.cshtml`) for DRY code
- **JEE Backend Compatibility**: PUT returns 204 No Content (matches JEE reference)
- Navigation between views
- Responsive design for mobile/desktop
- Live MongoDB backend integration
- Error handling and fallback data
- **138 Unit Tests**: Comprehensive test coverage with 100% pass rate
- Clean, maintainable code structure
- The application follows ASP.NET Core MVC best practices with separation of concerns

**Data Source**: Currently connected to MongoDB backend with live POI data (152,578+ entries).

### Latest Updates (November 2025)

1. ✅ **Right-Click Context Menu**: Edit and delete POIs from map markers
2. ✅ **Advanced Filtering**: Name, Details, and Category filters implemented
3. ✅ **Test Suite Completion**: All 138 tests passing (100% pass rate)
4. ✅ **JEE Backend Compatibility**: PUT endpoint returns 204 No Content
5. ✅ **Bug Fixes**: Context menu styling, navbar overlap, coordinate handling
6. ✅ **Code Quality**: Test modernization and obsolete test removal

## localStorage Keys

The application uses localStorage for state persistence across pages:

| Key | Purpose | Scope |
|-----|---------|-------|
| `poi_latitude` | Latitude coordinate | Session |
| `poi_longitude` | Longitude coordinate | Session |
| `poi_radius` | Search radius in meters | Session |
| `poi_name_filter` | Name filter value | Session |
| `poi_details_filter` | Details filter value | Session |
| `poi_category_filter` | Selected category filter | Session |
| `poi_map_zoom` | Map zoom level | Session |
| `poi_view` | View preference (cards/list) | Persistent |
| `poi_session_start` | Session timestamp | Session |
| `poi_needs_reload` | Reload trigger flag | Session |

**Session Timeout**: 30 minutes of inactivity automatically clears session data and resets to defaults.

## Future Enhancements

- [ ] Real-time updates with SignalR
- [ ] Caching for better performance
- [ ] User authentication & preferences
- [ ] Enhanced error handling UI
- [ ] POI favorites/bookmarks
- [ ] Export POIs to CSV/JSON
- [ ] Advanced search with multiple criteria
- [ ] POI clustering for better map performance