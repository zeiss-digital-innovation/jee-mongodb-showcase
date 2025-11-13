using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNetMongoDbBackend.Models.DTOs;

/// <summary>
/// Point of Interest Model für MongoDB
/// Kompatibel mit dem JEE und Spring Boot Backend Schema
/// </summary>
public class PointOfInterestDto
{
    [JsonPropertyName("_id")]
    [JsonIgnore]
    public string? Id { get; set; }

    // NOTE: href is NOT stored in MongoDB (like JEE reference implementation)
    // It is generated dynamically by the backend when returning POIs
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    [JsonPropertyName("category")]
    [Required(ErrorMessage = "Category is required")]
    public string? Category { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [JsonPropertyName("location")]
    public LocationDto? Location { get; set; }

    // Optional fields that may not be present in all documents
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Location (Koordinaten) für einen Point of Interest - MongoDB GeoJSON kompatibel
/// </summary>
public class LocationDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Point";

    [JsonPropertyName("coordinates")]
    [Required(ErrorMessage = "Coordinates are required")]
    public double[] Coordinates { get; set; } = new double[2];

    // Convenience Properties für bessere API-Kompatibilität
    [JsonIgnore]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude
    {
        get => Coordinates.Length > 0 ? Coordinates[0] : 0;
        set
        {
            if (Coordinates.Length < 2) Coordinates = new double[2];
            Coordinates[0] = value;
        }
    }

    [JsonIgnore]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude
    {
        get => Coordinates.Length > 1 ? Coordinates[1] : 0;
        set
        {
            if (Coordinates.Length < 2) Coordinates = new double[2];
            Coordinates[1] = value;
        }
    }

    /// <summary>
    /// Calculates the distance to another location using the Haversine formula
    /// </summary>
    /// <param name="other">The other location</param>
    /// <returns>Distance in kilometers</returns>
    public double DistanceTo(LocationDto? other)
    {
        if (other == null) return double.MaxValue;

        const double earthRadiusKm = 6371.0;
        var lat1Rad = Latitude * Math.PI / 180.0;
        var lat2Rad = other.Latitude * Math.PI / 180.0;
        var deltaLat = (other.Latitude - Latitude) * Math.PI / 180.0;
        var deltaLon = (other.Longitude - Longitude) * Math.PI / 180.0;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }
}