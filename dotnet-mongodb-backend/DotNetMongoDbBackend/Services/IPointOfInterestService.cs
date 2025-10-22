using DotNetMongoDbBackend.Models;

namespace DotNetMongoDbBackend.Services;

public interface IPointOfInterestService
{
    Task<List<PointOfInterest>> GetAllPoisAsync();
    Task<PointOfInterest?> GetPoiByIdAsync(string id);
    Task<List<PointOfInterest>> GetPoisByCategoryAsync(string category);
    Task<List<PointOfInterest>> SearchPoisAsync(string searchTerm, int? limit = null);
    Task<List<PointOfInterest>> GetNearbyPoisAsync(double longitude, double latitude, double radiusInKm = 10.0);
    
    /// <summary>
    /// Get nearby POIs filtered by multiple categories
    /// NEW: Category filtering at MongoDB level
    /// </summary>
    Task<List<PointOfInterest>> GetNearbyPoisByCategoriesAsync(double longitude, double latitude, double radiusInKm, List<string> categories);
    
    Task<PointOfInterest> CreatePoiAsync(PointOfInterest poi);
    Task<PointOfInterest?> UpdatePoiAsync(string id, PointOfInterest poi);
    Task<bool> DeletePoiAsync(string id);
    Task<List<string>> GetAvailableCategoriesAsync();
    Task<long> CountByCategoryAsync(string category);
}
