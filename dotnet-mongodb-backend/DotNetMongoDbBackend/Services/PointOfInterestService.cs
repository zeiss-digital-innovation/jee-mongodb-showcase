using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DotNetMongoDbBackend.Services;

/// <summary>
/// Service for Point of Interest Business Logic
/// Compatible with JEE and Spring Boot Backend APIs
/// </summary>
public class PointOfInterestService : IPointOfInterestService
{
    private readonly IMongoCollection<PointOfInterest> _poisCollection;
    private readonly ILogger<PointOfInterestService> _logger;

    public PointOfInterestService(IMongoDatabase database, IOptions<MongoSettings> mongoSettings, ILogger<PointOfInterestService> logger)
    {
        // Use central configuration from MongoSettings
        var collectionName = mongoSettings.Value.Collections.Pois;
        _poisCollection = database.GetCollection<PointOfInterest>(collectionName);
        _logger = logger;

        _logger.LogInformation("PointOfInterestService initialized with Collection: {CollectionName}", collectionName);
        _logger.LogInformation("MongoDB Database: {DatabaseName}", database.DatabaseNamespace.DatabaseName);

        // Test collection connection immediately
        try
        {
            var testCount = _poisCollection.CountDocuments(FilterDefinition<PointOfInterest>.Empty);
            _logger.LogInformation("MongoDB test successful: {Count} documents found in Collection '{CollectionName}'", testCount, collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR during MongoDB connection test for Collection '{CollectionName}'", collectionName);
        }

        // Create 2dsphere index for geographic searches
        CreateIndexes();
    }

    /// <summary>
    /// Get all POIs
    /// </summary>
    public async Task<List<PointOfInterest>> GetAllPoisAsync()
    {
        try
        {
            return await _poisCollection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all POIs");
            throw;
        }
    }

    /// <summary>
    /// Find POI by ID
    /// </summary>
    public async Task<PointOfInterest?> GetPoiByIdAsync(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return null;
            }

            return await _poisCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving POI with ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Filter POIs by category
    /// </summary>
    public async Task<List<PointOfInterest>> GetPoisByCategoryAsync(string category)
    {
        try
        {
            var filter = Builders<PointOfInterest>.Filter.Regex(
                p => p.Category,
                new BsonRegularExpression(category, "i")
            );

            return await _poisCollection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving POIs for category: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// Search POIs (Name, Tags)
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

            var tagsFilter = Builders<PointOfInterest>.Filter.AnyEq(
                p => p.Tags,
                searchTerm
            );

            var combinedFilter = Builders<PointOfInterest>.Filter.Or(nameFilter, tagsFilter);

            var query = _poisCollection.Find(combinedFilter);

            if (limit.HasValue && limit.Value > 0)
            {
                query = query.Limit(limit.Value);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching POIs with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Find POIs near a geographic location
    /// </summary>
    public async Task<List<PointOfInterest>> GetNearbyPoisAsync(double longitude, double latitude, double radiusInKm)
    {
        try
        {
            // Use GeoWithin instead of Near for better 2dsphere index compatibility
            var radiusInRadians = radiusInKm / 6378.1; // Earth radius in km

            var geoWithinFilter = Builders<PointOfInterest>.Filter.GeoWithinCenterSphere(
                p => p.Location,
                longitude,
                latitude,
                radiusInRadians
            );

            return await _poisCollection.Find(geoWithinFilter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving POIs near ({Longitude}, {Latitude})", longitude, latitude);
            throw;
        }
    }

    /// <summary>
    /// Find POIs near a geographic location filtered by multiple categories
    /// NEW: MongoDB-based category filtering with $in operator
    /// </summary>
    public async Task<List<PointOfInterest>> GetNearbyPoisByCategoriesAsync(double longitude, double latitude, double radiusInKm, List<string> categories)
    {
        try
        {
            // Use GeoWithin for geographic search
            var radiusInRadians = radiusInKm / 6378.1; // Earth radius in km

            var geoWithinFilter = Builders<PointOfInterest>.Filter.GeoWithinCenterSphere(
                p => p.Location,
                longitude,
                latitude,
                radiusInRadians
            );

            // Category filter: case-insensitive match with $in operator
            // Convert all categories to lowercase for consistent matching
            var normalizedCategories = categories.Select(c => c.ToLower()).ToList();
            
            var categoryFilter = Builders<PointOfInterest>.Filter.In(
                p => p.Category,
                normalizedCategories
            );

            // Combine both filters with AND
            var combinedFilter = Builders<PointOfInterest>.Filter.And(geoWithinFilter, categoryFilter);

            _logger.LogInformation(
                "MongoDB Query: GeoWithin({Longitude}, {Latitude}, {RadiusKm}km) AND Category IN [{Categories}]",
                longitude, latitude, radiusInKm, string.Join(", ", normalizedCategories));

            return await _poisCollection.Find(combinedFilter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving POIs near ({Longitude}, {Latitude}) with categories: {Categories}",
                longitude, latitude, string.Join(", ", categories));
            throw;
        }
    }

    /// <summary>
    /// Create POI
    /// </summary>
    public async Task<PointOfInterest> CreatePoiAsync(PointOfInterest poi)
    {
        try
        {
            ValidatePoi(poi);

            // Clear client-provided fields that should be managed by backend
            poi.Id = null; // New ObjectId will be automatically generated
            poi.Href = null; // Href is not stored in DB, will be generated on retrieval
            
            await _poisCollection.InsertOneAsync(poi);

            _logger.LogInformation("POI created: {Name} (ID: {Id})", poi.Name, poi.Id);
            return poi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating POI: {Name}", poi?.Name);
            throw;
        }
    }

    /// <summary>
    /// Update POI
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

            _logger.LogInformation("POI updated: {Name} (ID: {Id})", poi.Name, poi.Id);
            return poi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating POI with ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete POI
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
                _logger.LogInformation("POI deleted with ID: {Id}", id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting POI with ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Get all available categories
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
            _logger.LogError(ex, "Error retrieving available categories");
            throw;
        }
    }

    /// <summary>
    /// Count POIs by category
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
            _logger.LogError(ex, "Error counting POIs for category: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// POI validation
    /// </summary>
    private static void ValidatePoi(PointOfInterest poi)
    {
        if (poi == null)
            throw new ArgumentNullException(nameof(poi), "POI must not be null.");

        if (string.IsNullOrWhiteSpace(poi.Details))
            throw new ArgumentException("POI Details required.");

        if (string.IsNullOrWhiteSpace(poi.Category))
            throw new ArgumentException("POI Category required.");

        if (poi.Location == null)
            throw new ArgumentException("POI Location required.");

        if (poi.Location.Latitude < -90 || poi.Location.Latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90.");

        if (poi.Location.Longitude < -180 || poi.Location.Longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180.");
    }

    /// <summary>
    /// Create required MongoDB indexes
    /// </summary>
    private void CreateIndexes()
    {
        try
        {
            // 2dsphere index for geographic searches
            var locationIndex = Builders<PointOfInterest>.IndexKeys.Geo2DSphere(p => p.Location);
            _poisCollection.Indexes.CreateOne(new CreateIndexModel<PointOfInterest>(locationIndex));

            // Text index for full-text search on name field
            var textIndex = Builders<PointOfInterest>.IndexKeys.Text(p => p.Name);
            _poisCollection.Indexes.CreateOne(new CreateIndexModel<PointOfInterest>(textIndex));

            // Index for category searches
            var categoryIndex = Builders<PointOfInterest>.IndexKeys.Ascending(p => p.Category);
            _poisCollection.Indexes.CreateOne(new CreateIndexModel<PointOfInterest>(categoryIndex));

            _logger.LogInformation("MongoDB indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error creating MongoDB indexes (they may already exist)");
        }
    }
}