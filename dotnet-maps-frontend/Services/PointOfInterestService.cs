using DotNetMapsFrontend.Models;
using System.Text.Json;
using System.Globalization;

namespace DotNetMapsFrontend.Services
{
    public interface IPointOfInterestService
    {
        Task<List<PointOfInterest>> GetPointsOfInterestAsync();
        Task<List<PointOfInterest>> GetPointsOfInterestAsync(double latitude, double longitude, int radiusInMeters);
        Task<List<PointOfInterest>> GetPointsOfInterestAsync(double latitude, double longitude, int radiusInMeters, List<string> categories);
        Task<PointOfInterest?> GetPointOfInterestByIdAsync(string id);
        Task<PointOfInterest> CreatePointOfInterestAsync(PointOfInterest pointOfInterest);
        Task<PointOfInterest?> UpdatePointOfInterestAsync(string id, PointOfInterest pointOfInterest);
        Task DeletePointOfInterestAsync(string id);
        Task<List<string>> GetAvailableCategoriesAsync();
    }

    public class PointOfInterestService : IPointOfInterestService
    {
        /// <summary>
        /// Default categories for fallback when backend does not provide a /categories endpoint.
        /// Compatible with JEE-Backend + Angular-Frontend reference implementation.
        /// </summary>
        private static readonly List<string> DEFAULT_CATEGORIES = new()
        {
            "bank",
            "cash",
            "castle",
            "coffee",
            "company",
            "gasstation",
            "hotel",
            "landmark",
            "lodging",
            "museum",
            "parking",
            "pharmacy",
            "police",
            "post",
            "Restaurant",
            "restaurant",
            "supermarket",
            "toilet"
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PointOfInterestService> _logger;

        public PointOfInterestService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PointOfInterestService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<PointOfInterest>> GetPointsOfInterestAsync()
        {
            // Default values from Angular Frontend (Dresden, Zoom 13)
            const double defaultLatitude = 51.0504;
            const double defaultLongitude = 13.7373;
            const int defaultRadius = 2000; // Radius for zoom level 13
            
            return await GetPointsOfInterestAsync(defaultLatitude, defaultLongitude, defaultRadius);
        }

        public async Task<List<PointOfInterest>> GetPointsOfInterestAsync(double latitude, double longitude, int radiusInMeters)
        {
            // Call overload without category filter (backward compatible)
            return await GetPointsOfInterestAsync(latitude, longitude, radiusInMeters, new List<string>());
        }

        public async Task<List<PointOfInterest>> GetPointsOfInterestAsync(double latitude, double longitude, int radiusInMeters, List<string> categories)
        {
            try
            {
                var apiBaseUrl = _configuration["PointOfInterestApi:BaseUrl"];
                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    _logger.LogWarning("API Base URL not configured, using mock data");
                    return GetMockData();
                }

                var httpClient = _httpClientFactory.CreateClient();
                
                // Build URL with category parameters
                var urlBuilder = new System.Text.StringBuilder();
                urlBuilder.Append($"{apiBaseUrl}/poi?lat={latitude.ToString("F6", CultureInfo.InvariantCulture)}");
                urlBuilder.Append($"&lon={longitude.ToString("F6", CultureInfo.InvariantCulture)}");
                urlBuilder.Append($"&radius={radiusInMeters}");
                
                // Add category parameters (repeated parameter pattern: ?category=x&category=y)
                if (categories != null && categories.Count > 0)
                {
                    foreach (var category in categories)
                    {
                        urlBuilder.Append($"&category={Uri.EscapeDataString(category)}");
                    }
                    _logger.LogInformation("Requesting POIs with {Count} category filters: {Categories}", 
                        categories.Count, string.Join(", ", categories));
                }
                
                var url = urlBuilder.ToString();
                _logger.LogInformation("Fetching POIs from: {Url}", url);
                
                var response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var points = JsonSerializer.Deserialize<List<PointOfInterest>>(jsonString, options);
                    
                    // Convert all categories to lowercase for consistency
                    if (points != null)
                    {
                        foreach (var point in points)
                        {
                            if (!string.IsNullOrEmpty(point.Category))
                            {
                                point.Category = point.Category.ToLower();
                            }
                        }
                    }
                    
                    _logger.LogInformation("Successfully loaded {Count} POIs for coordinates ({Lat}, {Lon}) with radius {Radius}m and {CategoryCount} categories", 
                        points?.Count ?? 0, latitude, longitude, radiusInMeters, categories?.Count ?? 0);
                    return points ?? new List<PointOfInterest>();
                }
                
                _logger.LogWarning("API call failed with status: {StatusCode}", response.StatusCode);
                return GetMockData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Points of Interest API");
                return GetMockData();
            }
        }

        public async Task<PointOfInterest> CreatePointOfInterestAsync(PointOfInterest pointOfInterest)
        {
            try
            {
                var apiBaseUrl = _configuration["PointOfInterestApi:BaseUrl"];
                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    throw new InvalidOperationException("API Base URL not configured");
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{apiBaseUrl}/poi";
                
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var jsonContent = JsonSerializer.Serialize(pointOfInterest, jsonOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Creating POI: {Category} at coordinates ({Lat}, {Lon})", 
                    pointOfInterest.Category, pointOfInterest.Location.Latitude, pointOfInterest.Location.Longitude);
                
                var response = await httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    // Backend returns 201 Created without body, only Location header
                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        // Set Href from Location header if available
                        if (response.Headers.Location != null)
                        {
                            pointOfInterest.Href = response.Headers.Location.ToString();
                        }
                        _logger.LogInformation("Successfully created POI (empty response body, Location: {Location})", 
                            response.Headers.Location?.ToString());
                        return pointOfInterest;
                    }
                    
                    // If backend returns a body, deserialize it
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var createdPoi = JsonSerializer.Deserialize<PointOfInterest>(jsonString, options);
                    
                    // Convert category to lowercase for consistency
                    if (createdPoi != null && !string.IsNullOrEmpty(createdPoi.Category))
                    {
                        createdPoi.Category = createdPoi.Category.ToLower();
                    }
                    
                    _logger.LogInformation("Successfully created POI with ID: {Id}", createdPoi?.Href);
                    return createdPoi ?? pointOfInterest;
                }
                
                _logger.LogWarning("POI creation failed with status: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to create POI: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Point of Interest");
                throw;
            }
        }

        public async Task<PointOfInterest?> GetPointOfInterestByIdAsync(string id)
        {
            try
            {
                var apiBaseUrl = _configuration["PointOfInterestApi:BaseUrl"];
                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    throw new InvalidOperationException("API Base URL not configured");
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{apiBaseUrl}/poi/{id}";
                
                _logger.LogInformation("Fetching POI by ID from: {Url}", url);
                
                var response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var poi = JsonSerializer.Deserialize<PointOfInterest>(jsonString, options);
                    
                    // Convert category to lowercase for consistency
                    if (poi != null && !string.IsNullOrEmpty(poi.Category))
                    {
                        poi.Category = poi.Category.ToLower();
                    }
                    
                    _logger.LogInformation("Successfully loaded POI with ID: {Id}", id);
                    return poi;
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("POI not found with ID: {Id}", id);
                    return null;
                }
                
                _logger.LogWarning("Failed to fetch POI with status: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to fetch POI: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Point of Interest by ID: {Id}", id);
                throw;
            }
        }

        public async Task<PointOfInterest?> UpdatePointOfInterestAsync(string id, PointOfInterest pointOfInterest)
        {
            try
            {
                var apiBaseUrl = _configuration["PointOfInterestApi:BaseUrl"];
                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    throw new InvalidOperationException("API Base URL not configured");
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{apiBaseUrl}/poi/{id}";
                
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var jsonContent = JsonSerializer.Serialize(pointOfInterest, jsonOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Updating POI with ID: {Id}", id);
                
                var response = await httpClient.PutAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var updatedPoi = JsonSerializer.Deserialize<PointOfInterest>(jsonString, options);
                    
                    // Convert category to lowercase for consistency
                    if (updatedPoi != null && !string.IsNullOrEmpty(updatedPoi.Category))
                    {
                        updatedPoi.Category = updatedPoi.Category.ToLower();
                    }
                    
                    _logger.LogInformation("Successfully updated POI with ID: {Id}", id);
                    return updatedPoi;
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("POI not found for update with ID: {Id}", id);
                    return null;
                }
                
                _logger.LogWarning("POI update failed with status: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to update POI: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Point of Interest with ID: {Id}", id);
                throw;
            }
        }

        public async Task DeletePointOfInterestAsync(string id)
        {
            try
            {
                var apiBaseUrl = _configuration["PointOfInterestApi:BaseUrl"];
                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    throw new InvalidOperationException("API Base URL not configured");
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{apiBaseUrl}/poi/{id}";
                
                _logger.LogInformation("Deleting POI with ID: {Id}", id);
                
                var response = await httpClient.DeleteAsync(url);
                
                // Per RFC 9110, DELETE is idempotent and should return 204 even if resource doesn't exist
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent || 
                    response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Successfully deleted POI with ID: {Id}", id);
                    return;
                }
                
                _logger.LogWarning("POI deletion returned unexpected status: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to delete POI: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Point of Interest with ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<string>> GetAvailableCategoriesAsync()
        {
            try
            {
                var apiBaseUrl = _configuration["PointOfInterestApi:BaseUrl"];
                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    _logger.LogWarning("API Base URL not configured, using fallback categories");
                    return GetFallbackCategories();
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{apiBaseUrl}/categories";
                
                _logger.LogInformation("Fetching categories from: {Url}", url);
                
                var response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<string>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    // Convert all categories to lowercase for consistency
                    var lowercaseCategories = categories?.Select(c => c.ToLower()).ToList();
                    
                    _logger.LogInformation("Successfully loaded {Count} categories from backend", lowercaseCategories?.Count ?? 0);
                    return lowercaseCategories ?? GetFallbackCategories();
                }
                
                _logger.LogWarning("Categories API call failed with status: {StatusCode}, using fallback (JEE-Backend may not have /categories endpoint)", response.StatusCode);
                return GetFallbackCategories();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories, using fallback");
                return GetFallbackCategories();
            }
        }

        /// <summary>
        /// Returns the default fallback categories.
        /// Used when backend does not have a /categories endpoint (e.g., JEE-Backend).
        /// </summary>
        private List<string> GetFallbackCategories()
        {
            _logger.LogInformation("Using default fallback categories ({Count} categories, JEE-Backend compatible)", 
                DEFAULT_CATEGORIES.Count);
            return new List<string>(DEFAULT_CATEGORIES);
        }

        private List<PointOfInterest> GetMockData()
        {
            return new List<PointOfInterest>
            {
                new PointOfInterest
                {
                    Category = "landmark",
                    Details = "Brandenburger Tor\nBerlin, Deutschland\nPariser Platz",
                    Href = "http://localhost/poi/1",
                    Location = new Location
                    {
                        Coordinates = new double[] { 13.3777, 52.5163 },
                        Type = "Point"
                    }
                },
                new PointOfInterest
                {
                    Category = "museum",
                    Details = "Deutsches Museum\nMünchen, Deutschland\nMuseumsinsel 1",
                    Href = "http://localhost/poi/2",
                    Location = new Location
                    {
                        Coordinates = new double[] { 11.5836, 48.1298 },
                        Type = "Point"
                    }
                },
                new PointOfInterest
                {
                    Category = "castle",
                    Details = "Schloss Neuschwanstein\nFüssen, Deutschland\nNeuschwansteinstraße 20",
                    Href = "http://localhost/poi/3",
                    Location = new Location
                    {
                        Coordinates = new double[] { 10.7498, 47.5576 },
                        Type = "Point"
                    }
                },
                new PointOfInterest
                {
                    Category = "cathedral",
                    Details = "Kölner Dom\nKöln, Deutschland\nDomkloster 4",
                    Href = "http://localhost/poi/4",
                    Location = new Location
                    {
                        Coordinates = new double[] { 6.9583, 50.9413 },
                        Type = "Point"
                    }
                },
                new PointOfInterest
                {
                    Category = "park",
                    Details = "Englischer Garten\nMünchen, Deutschland\nSchönfeldstraße",
                    Href = "http://localhost/poi/5",
                    Location = new Location
                    {
                        Coordinates = new double[] { 11.5896, 48.1642 },
                        Type = "Point"
                    }
                }
            };
        }
    }
}