using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DotNetMongoDbBackend.Services;

/// <summary>
/// Service für Point of Interest Business Logic
/// Kompatibel mit JEE und Spring Boot Backend APIs
/// </summary>
public class PointOfInterestService : IPointOfInterestService
{
    private readonly IMongoCollection<PointOfInterest> _poisCollection;
    private readonly ILogger<PointOfInterestService> _logger;

    public PointOfInterestService(IMongoDatabase database, IOptions<MongoSettings> mongoSettings, ILogger<PointOfInterestService> logger)
    {
        _poisCollection = database.GetCollection<PointOfInterest>(mongoSettings.Value.Collections.Pois);
        _logger = logger;
        
        _logger.LogInformation("PointOfInterestService initialisiert mit Collection: {CollectionName}", mongoSettings.Value.Collections.Pois);

        // Erstelle 2dsphere Index für geografische Suchen
        CreateIndexes();
    }

    /// <summary>
    /// Alle POIs abrufen
    /// </summary>
    public async Task<List<PointOfInterest>> GetAllPoisAsync()
    {
        try
        {
            var pois = await _poisCollection.Find(_ => true).ToListAsync();
            pois.ForEach(poi => poi.GenerateHref());
            return pois;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen aller POIs");
            throw;
        }
    }

    /// <summary>
    /// POI nach ID suchen
    /// </summary>
    public async Task<PointOfInterest?> GetPoiByIdAsync(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return null;
            }

            var poi = await _poisCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            poi?.GenerateHref();
            return poi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des POI mit ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// POIs nach Kategorie filtern
    /// </summary>
    public async Task<List<PointOfInterest>> GetPoisByCategoryAsync(string category)
    {
        try
        {
            var filter = Builders<PointOfInterest>.Filter.Regex(
                p => p.Category, 
                new BsonRegularExpression(category, "i")
            );

            var pois = await _poisCollection.Find(filter).ToListAsync();
            pois.ForEach(poi => poi.GenerateHref());
            return pois;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der POIs für Kategorie: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// POIs suchen (Name, Address, Tags)
    /// </summary>
    public async Task<List<PointOfInterest>> SearchPoisAsync(string searchTerm, int? limit = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllPoisAsync();
            }

            var nameFilter = Builders<PointOfInterest>.Filter.Regex(
                p => p.Name, 
                new BsonRegularExpression(searchTerm, "i")
            );

            var addressFilter = Builders<PointOfInterest>.Filter.Regex(
                p => p.Address, 
                new BsonRegularExpression(searchTerm, "i")
            );

            var tagsFilter = Builders<PointOfInterest>.Filter.AnyEq(
                p => p.Tags, 
                searchTerm
            );

            var combinedFilter = Builders<PointOfInterest>.Filter.Or(nameFilter, addressFilter, tagsFilter);

            var query = _poisCollection.Find(combinedFilter);

            if (limit.HasValue && limit.Value > 0)
            {
                query = query.Limit(limit.Value);
            }

            var pois = await query.ToListAsync();
            pois.ForEach(poi => poi.GenerateHref());
            return pois;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Suchen der POIs mit Term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// POIs in der Nähe einer geografischen Position finden
    /// </summary>
    public async Task<List<PointOfInterest>> GetNearbyPoisAsync(double longitude, double latitude, double radiusInKm)
    {
        try
        {
            // Verwende GeoWithin statt Near für bessere 2dsphere Index Kompatibilität
            var radiusInRadians = radiusInKm / 6378.1; // Erdradius in km

            var geoWithinFilter = Builders<PointOfInterest>.Filter.GeoWithinCenterSphere(
                p => p.Location,
                longitude,
                latitude, 
                radiusInRadians
            );

            var pois = await _poisCollection.Find(geoWithinFilter).ToListAsync();
            pois.ForEach(poi => poi.GenerateHref());
            return pois;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen von POIs in der Nähe von ({Longitude}, {Latitude})", longitude, latitude);
            throw;
        }
    }

    /// <summary>
    /// POI erstellen
    /// </summary>
    public async Task<PointOfInterest> CreatePoiAsync(PointOfInterest poi)
    {
        try
        {
            ValidatePoi(poi);
            
            poi.Id = null; // Neue ObjectId wird automatisch generiert
            await _poisCollection.InsertOneAsync(poi);
            poi.GenerateHref();
            
            _logger.LogInformation("POI erstellt: {Name} (ID: {Id})", poi.Name, poi.Id);
            return poi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des POI: {Name}", poi?.Name);
            throw;
        }
    }

    /// <summary>
    /// POI aktualisieren
    /// </summary>
    public async Task<PointOfInterest?> UpdatePoiAsync(string id, PointOfInterest poi)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return null;
            }

            ValidatePoi(poi);
            poi.Id = id;

            var result = await _poisCollection.ReplaceOneAsync(p => p.Id == id, poi);

            if (result.MatchedCount == 0)
            {
                return null;
            }

            poi.GenerateHref();
            _logger.LogInformation("POI aktualisiert: {Name} (ID: {Id})", poi.Name, poi.Id);
            return poi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des POI mit ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// POI löschen
    /// </summary>
    public async Task<bool> DeletePoiAsync(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return false;
            }

            var result = await _poisCollection.DeleteOneAsync(p => p.Id == id);
            
            if (result.DeletedCount > 0)
            {
                _logger.LogInformation("POI gelöscht mit ID: {Id}", id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des POI mit ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Alle verfügbaren Kategorien abrufen
    /// </summary>
    public async Task<List<string>> GetAvailableCategoriesAsync()
    {
        try
        {
            var categories = await _poisCollection
                .Distinct<string>("category", FilterDefinition<PointOfInterest>.Empty)
                .ToListAsync();

            return categories.Where(c => !string.IsNullOrWhiteSpace(c))
                           .OrderBy(c => c)
                           .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der verfügbaren Kategorien");
            throw;
        }
    }

    /// <summary>
    /// Anzahl POIs nach Kategorie
    /// </summary>
    public async Task<long> CountByCategoryAsync(string category)
    {
        try
        {
            var filter = Builders<PointOfInterest>.Filter.Regex(
                p => p.Category, 
                new BsonRegularExpression(category, "i")
            );

            return await _poisCollection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Zählen der POIs für Kategorie: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// POI-Validierung
    /// </summary>
    private static void ValidatePoi(PointOfInterest poi)
    {
        if (poi == null)
            throw new ArgumentNullException(nameof(poi), "POI darf nicht null sein");

        if (string.IsNullOrWhiteSpace(poi.Name))
            throw new ArgumentException("POI Name ist erforderlich");

        if (string.IsNullOrWhiteSpace(poi.Category))
            throw new ArgumentException("POI Kategorie ist erforderlich");

        if (poi.Location == null)
            throw new ArgumentException("POI Location ist erforderlich");

        if (poi.Location.Latitude < -90 || poi.Location.Latitude > 90)
            throw new ArgumentException("Latitude muss zwischen -90 und 90 liegen");

        if (poi.Location.Longitude < -180 || poi.Location.Longitude > 180)
            throw new ArgumentException("Longitude muss zwischen -180 und 180 liegen");
    }

    /// <summary>
    /// Erstellt notwendige MongoDB-Indizes
    /// </summary>
    private void CreateIndexes()
    {
        try
        {
            // 2dsphere Index für geografische Suchen
            var locationIndex = Builders<PointOfInterest>.IndexKeys.Geo2DSphere(p => p.Location);
            _poisCollection.Indexes.CreateOne(new CreateIndexModel<PointOfInterest>(locationIndex));

            // Text-Index für Volltext-Suche
            var textIndex = Builders<PointOfInterest>.IndexKeys.Combine(
                Builders<PointOfInterest>.IndexKeys.Text(p => p.Name),
                Builders<PointOfInterest>.IndexKeys.Text(p => p.Address)
            );
            _poisCollection.Indexes.CreateOne(new CreateIndexModel<PointOfInterest>(textIndex));

            // Index für Kategorie-Suchen
            var categoryIndex = Builders<PointOfInterest>.IndexKeys.Ascending(p => p.Category);
            _poisCollection.Indexes.CreateOne(new CreateIndexModel<PointOfInterest>(categoryIndex));

            _logger.LogInformation("MongoDB-Indizes erfolgreich erstellt");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fehler beim Erstellen der MongoDB-Indizes (können bereits existieren)");
        }
    }
}