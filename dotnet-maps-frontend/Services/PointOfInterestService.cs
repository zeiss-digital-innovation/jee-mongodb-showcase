using DotNetMapsFrontend.Models;
using System.Text.Json;
using System.Globalization;

namespace DotNetMapsFrontend.Services
{
    public interface IPointOfInterestService
    {
        Task<List<PointOfInterest>> GetPointsOfInterestAsync();
        Task<List<PointOfInterest>> GetPointsOfInterestAsync(double latitude, double longitude, int radiusInMeters);
    }

    public class PointOfInterestService : IPointOfInterestService
    {
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
            try
            {
                var apiBaseUrl = _configuration["PointOfInterestApi:BaseUrl"];
                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    _logger.LogWarning("API Base URL not configured, using mock data");
                    return GetMockData();
                }

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{apiBaseUrl}/poi?lat={latitude.ToString("F6", CultureInfo.InvariantCulture)}&lon={longitude.ToString("F6", CultureInfo.InvariantCulture)}&radius={radiusInMeters}&expand=details";
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
                    _logger.LogInformation("Successfully loaded {Count} POIs for coordinates ({Lat}, {Lon}) with radius {Radius}m", 
                        points?.Count ?? 0, latitude, longitude, radiusInMeters);
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