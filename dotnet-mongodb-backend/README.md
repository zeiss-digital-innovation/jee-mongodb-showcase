# ZDI - MongoDB Workshop - .NET Backend

## 🚀 Overview

This **.NET Backend** is part of the MongoDB Workshop project and provides a high-performance, modern REST API for managing Points of Interest (POIs). It is fully compatible with the Angular Frontend and offers the same API structure as the JEE Backend.

## 🛠 Technology Stack

- **.NET 9** - Latest .NET Version
- **ASP.NET Core** - High-Performance Web Framework
- **MongoDB.Driver 3.5.0** - Official MongoDB C# Driver
- **Newtonsoft.Json 13.0.3** - JSON Serialization
- **Microsoft.AspNetCore.OpenApi** - OpenAPI/Swagger Integration
- **Built-in Dependency Injection** - .NET DI Container
- **xUnit** - Testing Framework

## 📋 Features

### Core Functionalities
- ✅ **Async/Await Pattern** - Fully asynchronous API
- ✅ **CRUD Operations** for Points of Interest
- ✅ **Geographic Search** with MongoDB Geo-Queries
- ✅ **Full-Text Search** with Regex-Pattern Matching
- ✅ **Category Filter** with case-insensitive search
- ✅ **Distance Calculation** with Haversine Formula
- ✅ **Clean Architecture** - Entity/DTO separation with Mapper layer
- ✅ **Data Annotations** Validation
- ✅ **Structured Logging** with ILogger
- ✅ **Auto-Index Creation** for optimal performance

### API Endpoints
```
GET  /zdi-geo-service/api/poi                    - All POIs (with query parameters)
GET  /zdi-geo-service/api/poi/{id}              - POI by ID
POST /zdi-geo-service/api/poi                   - Create new POI
PUT  /zdi-geo-service/api/poi/{id}              - Update POI
DELETE /zdi-geo-service/api/poi/{id}            - Delete POI
GET  /zdi-geo-service/api/categories            - All available categories
GET  /zdi-geo-service/api/stats/category/{cat}  - Statistics for category
GET  /zdi-geo-service/api/health                - Health Check
GET  /zdi-geo-service/api/debug                 - Debug information
```

### Query Parameters for /zdi-geo-service/api/poi
- `category` - Filter by category
- `search` - Full-text search in name, address, tags
- `limit` - Maximum number of results to return
- `lat` & `lng` (or `lon`) - Geographic search (coordinates)
- `radius` - Radius in meters (automatically converted to km)

## 🚀 Installation & Start

### Prerequisites
- .NET 9 SDK or higher
- MongoDB running on localhost:27017

### 🎯 Quick Start (Recommended)

#### Automatic Docker Deployment
```bash
# Windows - Intelligent MongoDB Detection
.\deploy.bat

# Linux/macOS - Intelligent MongoDB Detection  
chmod +x deploy.sh
./deploy.sh
```

The deploy scripts automatically detect:
- ✅ Existing MongoDB containers
- ✅ External MongoDB installations
- ✅ Network configurations
- ✅ Optimal docker-compose file

### 🐳 Docker Deployment Options

#### 1. Complete System (Backend + MongoDB)
```bash
# Starts own MongoDB + Backend
docker-compose up --build -d
```

#### 2. Backend Only (external MongoDB)
```bash
# Uses existing MongoDB
docker-compose -f docker-compose.external-mongo.yml up --build -d
```

#### 3. Development Mode
```bash
# Development with Hot Reload
docker-compose -f docker-compose.local.yml up --build
```

### 💻 Local Development (without Docker)

#### Start Project
```bash
# Navigate to project directory
cd dotnet-mongodb-backend/DotNetMongoDbBackend

# Restore dependencies
dotnet restore

# Start project (Development)
dotnet run

# Or Release Build
dotnet build -c Release
dotnet run -c Release
```

### Server URLs
- **API Base URL**: http://localhost:8080/zdi-geo-service/api
- **Health Check**: http://localhost:8080/zdi-geo-service/api/health
- **Debug Information**: http://localhost:8080/zdi-geo-service/api/debug
- **Swagger UI**: http://localhost:8080/zdi-geo-service/swagger (only active in development mode)

## 📊 Data Models

### MongoDB Entity (Persistence Layer)
```json
{
  "_id": "ObjectId",
  "name": "POI Name",
  "category": "restaurant|pharmacy|parking|etc",
  "location": {
    "type": "Point",
    "coordinates": [13.7373, 51.0504]
  },
  "details": "Street 123, 01067 Dresden",
  "tags": ["tag1", "tag2"]
}
```

### API DTO (REST API Layer)
```json
{
  "href": "/zdi-geo-service/api/poi/{id}",
  "name": "POI Name",
  "category": "restaurant|pharmacy|parking|etc",
  "location": {
    "type": "Point",
    "coordinates": [13.7373, 51.0504],
    "longitude": 13.7373,
    "latitude": 51.0504
  },
  "details": "Street 123, 01067 Dresden",
  "tags": ["tag1", "tag2"]
}
```

**Architecture:**
- **Entity classes** (`Models/Entities/`): BSON attributes for MongoDB persistence
- **DTO classes** (`Models/DTOs/`): JSON attributes and validation for REST API
- **Mapper** (`Mappers/`): Bidirectional conversion (Entity ↔ DTO)

## 🔧 Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "Console": {
      "TimestampFormat": "ddMMyy HH:mm ",
      "IncludeScopes": false
    }
  },
  "AllowedHosts": "*",
  "MongoSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "demo-campus",
    "Collections": {
      "Pois": "point-of-interest"
    }
  }
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Tests with Coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch Mode for Development
dotnet watch test
```

### Test Coverage

✅ **121 Unit Tests** - All passing (100% pass rate)
- **Backend API Tests**: 85.6% line coverage, 85.2% branch coverage
- **Controller Tests**: CRUD operations, validation, error handling
- **Service Tests**: MongoDB operations and business logic
- **Integration Tests**: End-to-end API workflow

The backend maintains high code quality with comprehensive test coverage and follows testing best practices.

## 📚 API Examples

### Get All POIs
```bash
curl http://localhost:8080/zdi-geo-service/api/poi
```

### Filter POIs by Category
```bash
curl "http://localhost:8080/zdi-geo-service/api/poi?category=restaurant"
```

### Geographic Search
```bash
curl "http://localhost:8080/zdi-geo-service/api/poi?lat=51.0504&lng=13.7373&radius=2000"
```

### Full-Text Search
```bash
curl "http://localhost:8080/zdi-geo-service/api/poi?search=Pharmacy&limit=10"
```

### Create New POI
```bash
curl -X POST http://localhost:8080/zdi-geo-service/api/poi \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Restaurant",
    "category": "restaurant",
    "location": {
      "longitude": 13.7373,
      "latitude": 51.0504
    },
    "address": "Test Street 123",
    "tags": ["test", "restaurant"]
  }'
```

## 🏗 Architecture

```
DotNetMongoDbBackend/
├── Controllers/         # API Controllers (use DTOs)
├── Services/           # Business Logic Layer (use Entities)
├── Mappers/            # Entity ↔ DTO Conversion
├── Models/
│   ├── Entities/       # MongoDB persistence models (BSON attributes)
│   └── DTOs/           # API data transfer objects (JSON attributes, validation)
├── Configurations/     # Configuration classes
├── Program.cs          # Application Configuration
└── appsettings.json    # Configuration
```

### Design Patterns
- **Clean Architecture** - Entity/DTO separation with Mapper layer
- **Repository Pattern** - Implemented in Service Layer
- **Dependency Injection** - Native .NET DI
- **Async Pattern** - Task-based asynchronous operations
- **Builder Pattern** - MongoDB Filter Building
- **Option Pattern** - Configuration Management
- **Mapper Pattern** - Static methods for Entity ↔ DTO conversion

## 🔗 Integration

### With Angular Frontend
```typescript
// Environment Configuration
export const environment = {
  apiBaseUrl: 'http://localhost:8080/zdi-geo-service/api'
};
```

### With Other Backends
- Port 8080: JEE Backend
- Port 8080: **.NET Backend** (this one)

## 📈 Performance Highlights

### MongoDB Optimizations
- **2dsphere Index** - Geographic searches
- **Text Index** - Full-text search
- **Category Index** - Category filtering
- **Connection Pooling** - Efficient DB connections

### .NET Performance Features
- **Async/Await** - Non-blocking I/O Operations
- **System.Text.Json** - High-Performance JSON
- **Kestrel Server** - Cross-platform web server
- **Memory Optimization** - Minimal allocations

### Benchmarks (typical values)
- **Startup Time**: < 2 seconds
- **Memory Usage**: ~30MB baseline
- **Response Time**: < 50ms (local DB)
- **Throughput**: > 10k requests/sec

## 🛡 Security

- **Data Annotations** - Input validation
- **CORS Policy** - Configured origins
- **Error Handling** - No stack trace exposure
- **Logging** - Security event tracking
- **MongoDB ObjectId** - Secure ID validation

## 📝 Development

### Code Conventions
- **C# Coding Standards**
- **Async/Await Best Practices**
- **RESTful API Design**
- **Clean Architecture Principles**

### Debugging
```bash
# Debug Mode with Hot Reload
dotnet watch run

# Attach Debugger
dotnet run --launch-profile https
```

## 🆚 Backend Comparison

| Feature | JEE Backend | **.NET Backend** |
|---------|-------------|------------------|
| Framework | Java EE 8 | **ASP.NET Core** |
| Language | Java 17 | **C# 12** |
| Port | 8080 | **8080** |
| Runtime | JVM | **.NET Runtime** |
| Startup | ~5-10s | **~1-2s** |
| Memory | ~100MB | **~30MB** |
| Performance | Good | **Excellent** |
| Async Support | Limited | **Native** |

## 🎯 C# Specific Features

### Modern C# Features used
```csharp
// Entity/DTO Separation with Mapper
public static class PointOfInterestMapper
{
    public static PointOfInterestDto ToDto(PointOfInterestEntity entity) { /*...*/ }
    public static PointOfInterestEntity ToEntity(PointOfInterestDto dto) { /*...*/ }
    public static List<PointOfInterestDto> ToDtoList(List<PointOfInterestEntity> entities) { /*...*/ }
}

// Pattern Matching
return poi switch
{
    null => NotFound(),
    _ => Ok(poi)
};

// Nullable Reference Types
public string? Details { get; set; }

// Local Functions
double CalculateDistance() => location.DistanceTo(other);

// Init-only properties in DTOs
public class PointOfInterestDto
{
    [Required] public string Category { get; init; } = string.Empty;
    public LocationDto? Location { get; init; }
}
```

### MongoDB.Driver Advanced Features
```csharp
// Fluent Filter Building with Entity types
var filter = Builders<PointOfInterestEntity>.Filter.And(
    Builders<PointOfInterestEntity>.Filter.Eq(p => p.Category, category),
    Builders<PointOfInterestEntity>.Filter.Near(p => p.Location, lng, lat, radius)
);

// Service Layer uses Entities
public async Task<List<PointOfInterestEntity>> GetAllPoisAsync()
{
    return await _collection.Find(FilterDefinition<PointOfInterestEntity>.Empty).ToListAsync();
}

// Controller Layer uses DTOs with Mapper
var entities = await _poiService.GetAllPoisAsync();
var dtos = PointOfInterestMapper.ToDtoList(entities);
return Ok(dtos);
```

## 🐳 Docker Support

```dockerfile
# Dockerfile creation
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY bin/Release/net9.0/publish/ .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DotNetMongoDbBackend.dll"]
```

```bash
# Docker Build & Run
dotnet publish -c Release
docker build -t dotnet-mongodb-backend .
docker run -p 8080:8080 dotnet-mongodb-backend
```

## 🎯 Next Steps

- [ ] **Docker Container** - Multi-stage build
- [ ] **Azure Container Apps** - Cloud deployment
- [ ] **Entity Framework** - Alternative ORM option
- [ ] **gRPC Services** - High-performance APIs
- [ ] **Health Checks** - Advanced monitoring
- [ ] **Rate Limiting** - API protection
- [ ] **Background Services** - Data processing

---

**Created for the ZDI MongoDB Workshop** 🚀

*Powered by .NET & MongoDB* ⚡

## ⚠️ Hosting behind a reverse proxy / forwarded headers

If you run this app behind a reverse proxy (NGINX, Traefik, Apache, cloud load balancer, etc.), the original scheme (http/https) and the original client IP are typically forwarded via X-Forwarded-For and X-Forwarded-Proto headers. To ensure absolute URL generation (LinkGenerator) and PathBase handling use the original scheme/host, the app config includes forwarded headers middleware.

Important notes:
- The app config enables X-Forwarded-For and X-Forwarded-Proto handling. For security, restrict forwarding sources in production by setting KnownProxies or KnownNetworks (see comments in `Program.cs`).
- If your proxy terminates TLS, make sure the proxy sets X-Forwarded-Proto to `https` so generated links use https.
- Example (NGINX):
  proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
  proxy_set_header X-Forwarded-Proto $scheme;

