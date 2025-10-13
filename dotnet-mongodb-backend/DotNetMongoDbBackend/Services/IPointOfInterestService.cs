using DotNetMongoDbBackend.Models;

namespace DotNetMongoDbBackend.Services;

public interface IPointOfInterestService
{
    Task<List<PointOfInterest>> GetAllPoisAsync();
    Task<PointOfInterest?> GetPoiByIdAsync(string id);
    Task<List<PointOfInterest>> GetPoisByCategoryAsync(string category);
    Task<List<PointOfInterest>> SearchPoisAsync(string searchTerm, int? limit = null);
    Task<List<PointOfInterest>> GetNearbyPoisAsync(double longitude, double latitude, double radiusInKm = 10.0);
    Task<PointOfInterest> CreatePoiAsync(PointOfInterest poi);
    Task<PointOfInterest?> UpdatePoiAsync(string id, PointOfInterest poi);
    Task<bool> DeletePoiAsync(string id);
    Task<List<string>> GetAvailableCategoriesAsync();
    Task<long> CountByCategoryAsync(string category);
}
