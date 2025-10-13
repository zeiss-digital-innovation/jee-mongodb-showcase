# MongoDB Konfiguration - Zentrale Verwaltung

## Übersicht

Um Inkonsistenzen bei DB- und Collection-Namen zu vermeiden, wurde eine zentrale Konfigurationsverwaltung implementiert.

## Zentrale Konstanten

### MongoConstants.cs
Diese Klasse enthält alle wichtigen MongoDB-bezogenen Konstanten:

```csharp
public static class MongoConstants
{
    public const string DatabaseName = "demo-campus";
    public const string PoiCollectionName = "point-of-interest";
    public const string DefaultConnectionString = "mongodb://localhost:27017";
}
```

### MongoSettings.cs
Die Konfigurationsklasse verwendet die Konstanten als Standard-Werte:

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

## Korrekte Namen

### Datenbank
- **Korrekt**: `demo-campus` (mit Bindestrich)
- **Falsch**: `demo_campus` (mit Unterstrich)

### Collection
- **Korrekt**: `point-of-interest` (singular, mit Bindestrichen)
- **Falsch**: `point_of_interest` (mit Unterstrichen)
- **Falsch**: `points_of_interest` (plural)
- **Falsch**: `points-of-interest` (plural)

## Verwendung in Code

### Im Service
```csharp
public PointOfInterestService(IMongoDatabase database, IOptions<MongoSettings> mongoSettings, ILogger<PointOfInterestService> logger)
{
    var collectionName = mongoSettings.Value.Collections.Pois;
    _poisCollection = database.GetCollection<PointOfInterest>(collectionName);
    // ...
}
```

### In Test-Code
```csharp
var databaseName = MongoConstants.DatabaseName;
var collectionName = MongoConstants.PoiCollectionName;
var database = client.GetDatabase(databaseName);
var collection = database.GetCollection<BsonDocument>(collectionName);
```

### In Konfigurationsdateien (appsettings.json)
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

### In Docker Compose Dateien
```yaml
environment:
  - MongoSettings__Database=demo-campus
  - MongoSettings__Collections__Pois=point-of-interest
```

## Vorteile der zentralen Konfiguration

1. **Konsistenz**: Alle Namen werden an einer Stelle definiert
2. **Wartbarkeit**: Änderungen müssen nur an einer Stelle vorgenommen werden
3. **Fehlerreduzierung**: Keine Tippfehler durch copy-paste
4. **Dokumentation**: Klare Kommentare zu den korrekten Werten
5. **Typsicherheit**: Compile-Zeit-Prüfung bei Verwendung der Konstanten

## Kompatibilität

Diese Konfiguration ist kompatibel mit:
- JEE MongoDB Backend (verwendet dieselben Namen)
- Angular Frontend
- Docker Container Setups
- MongoDB Initialisierungsskripts

## Geänderte Dateien

### Hauptdateien
- `Configurations/MongoConstants.cs` - **NEU**: Zentrale Konstanten
- `Configurations/MongoSettings.cs` - **AKTUALISIERT**: Verwendet jetzt Konstanten
- `Services/PointOfInterestService.cs` - **AKTUALISIERT**: Verwendet zentrale Konfiguration statt hardcoded values

### Test-Dateien
- `MongoDbTest/Program.cs` - **AKTUALISIERT**: Verwendet korrekte Namen
- `MongoTest.cs.backup` - **BEREINIGT**: Konsistente Namen

### Konfigurationsdateien (bereits korrekt)
- `appsettings.json`
- `appsettings.Development.json`
- `docker-compose.yml`
- `docker-compose.local.yml`
- `docker-compose.external-mongo.yml`

## Migration bestehender Code

Wenn Sie in Zukunft MongoDB-bezogenen Code schreiben:

1. Verwenden Sie immer `MongoConstants.DatabaseName` für den DB-Namen
2. Verwenden Sie immer `MongoConstants.PoiCollectionName` für die Collection
3. Oder nutzen Sie die `MongoSettings` dependency injection
4. Vermeiden Sie hardcoded Strings für DB/Collection-Namen

## Beispiel für neuen Code

```csharp
// Gut - verwendet Konstanten
var database = client.GetDatabase(MongoConstants.DatabaseName);
var collection = database.GetCollection<PointOfInterest>(MongoConstants.PoiCollectionName);

// Besser - verwendet dependency injection
public MyService(IOptions<MongoSettings> mongoSettings)
{
    var settings = mongoSettings.Value;
    var database = client.GetDatabase(settings.Database);
    var collection = database.GetCollection<PointOfInterest>(settings.Collections.Pois);
}

// Schlecht - hardcoded strings vermeiden
var database = client.GetDatabase("demo-campus"); // Nicht machen!
```