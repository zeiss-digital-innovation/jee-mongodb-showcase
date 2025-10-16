# MongoDB Configuration - Central Management

## Overview

To avoid inconsistencies in DB and collection names, a central configuration management has been implemented.

## Central Constants

### MongoConstants.cs
This class contains all important MongoDB-related constants:

```csharp
public static class MongoConstants
{
    public const string DatabaseName = "demo-campus";
    public const string PoiCollectionName = "point-of-interest";
    public const string DefaultConnectionString = "mongodb://localhost:27017";
}
```

### MongoSettings.cs
The configuration class uses the constants as default values:

```csharp
public class MongoSettings
{
    public string ConnectionString { get; set; } = MongoConstants.DefaultConnectionString;
    public string Database { get; set; } = MongoConstants.DatabaseName;
    public CollectionNames Collections { get; set; } = new();

    public class CollectionNames
    {
        public string Pois { get; set; } = MongoConstants.PoiCollectionName;
    }
}
```

## Correct Names

### Database
- **Correct**: `demo-campus` (with hyphen)
- **Wrong**: `demo_campus` (with underscore)

### Collection
- **Correct**: `point-of-interest` (singular, with hyphens)
- **Wrong**: `point_of_interest` (with underscores)
- **Wrong**: `points_of_interest` (plural)
- **Wrong**: `points-of-interest` (plural)

## Usage in Code

### In Service
```csharp
public PointOfInterestService(IMongoDatabase database, IOptions<MongoSettings> mongoSettings, ILogger<PointOfInterestService> logger)
{
    var collectionName = mongoSettings.Value.Collections.Pois;
    _poisCollection = database.GetCollection<PointOfInterest>(collectionName);
    // ...
}
```

### In Test Code
```csharp
var databaseName = MongoConstants.DatabaseName;
var collectionName = MongoConstants.PoiCollectionName;
var database = client.GetDatabase(databaseName);
var collection = database.GetCollection<BsonDocument>(collectionName);
```

### In Configuration Files (appsettings.json)
```json
{
  "MongoSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "demo-campus",
    "Collections": {
      "Pois": "point-of-interest"
    }
  }
}
```

### In Docker Compose Files
```yaml
environment:
  - MongoSettings__Database=demo-campus
  - MongoSettings__Collections__Pois=point-of-interest
```

## Benefits of Central Configuration

1. **Consistency**: All names are defined in one place
2. **Maintainability**: Changes only need to be made in one location
3. **Error Reduction**: No typos from copy-paste
4. **Documentation**: Clear comments on correct values
5. **Type Safety**: Compile-time checking when using constants

## Compatibility

This configuration is compatible with:
- JEE MongoDB Backend (uses the same names)
- Angular Frontend
- Docker Container Setups
- MongoDB Initialization Scripts

## Changed Files

### Main Files
- `Configurations/MongoConstants.cs` - **NEW**: Central constants
- `Configurations/MongoSettings.cs` - **UPDATED**: Now uses constants
- `Services/PointOfInterestService.cs` - **UPDATED**: Uses central configuration instead of hardcoded values

### Test Files
- `MongoDbTest/Program.cs` - **UPDATED**: Uses correct names
- `MongoTest.cs.backup` - **CLEANED UP**: Consistent names

### Configuration Files (already correct)
- `appsettings.json`
- `appsettings.Development.json`
- `docker-compose.yml`
- `docker-compose.local.yml`
- `docker-compose.external-mongo.yml`

## Migration of Existing Code

When writing MongoDB-related code in the future:

1. Always use `MongoConstants.DatabaseName` for the DB name
2. Always use `MongoConstants.PoiCollectionName` for the collection
3. Or use the `MongoSettings` dependency injection
4. Avoid hardcoded strings for DB/collection names

## Example for New Code

```csharp
// Good - uses constants
var database = client.GetDatabase(MongoConstants.DatabaseName);
var collection = database.GetCollection<PointOfInterest>(MongoConstants.PoiCollectionName);

// Better - uses dependency injection
public MyService(IOptions<MongoSettings> mongoSettings)
{
    var settings = mongoSettings.Value;
    var database = client.GetDatabase(settings.Database);
    var collection = database.GetCollection<PointOfInterest>(settings.Collections.Pois);
}

// Bad - avoid hardcoded strings
var database = client.GetDatabase("demo-campus"); // Don't do this!
```