using DotNetMongoDbBackend.Models.Entities;

namespace DotNetMongoDbBackend.Services;

public interface IPointOfInterestService
{
    Task<List<PointOfInterestEntity>> GetAllPoisAsync();
    Task<PointOfInterestEntity?> GetPoiByIdAsync(string id);
    Task<List<PointOfInterestEntity>> GetPoisByCategoryAsync(string category);
    Task<List<PointOfInterestEntity>> SearchPoisAsync(string searchTerm, int? limit = null);
    Task<List<PointOfInterestEntity>> GetNearbyPoisAsync(double longitude, double latitude, double radiusInKm = 10.0);
    
    /// <summary>
    /// Get nearby POIs filtered by multiple categories
    /// NEW: Category filtering at MongoDB level
    /// </summary>
    Task<List<PointOfInterestEntity>> GetNearbyPoisByCategoriesAsync(double longitude, double latitude, double radiusInKm, List<string> categories);
    
    Task<PointOfInterestEntity> CreatePoiAsync(PointOfInterestEntity poiEntity);
    Task<PointOfInterestEntity?> UpdatePoiAsync(string id, PointOfInterestEntity poiEntity);
    Task<bool> DeletePoiAsync(string id);
    Task<List<string>> GetAvailableCategoriesAsync();
    Task<long> CountByCategoryAsync(string category);
}
