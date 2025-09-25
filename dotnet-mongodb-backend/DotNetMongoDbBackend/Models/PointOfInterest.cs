using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNetMongoDbBackend.Models;

/// <summary>
/// Point of Interest Model für MongoDB
/// Kompatibel mit dem JEE und Spring Boot Backend Schema
/// </summary>
public class PointOfInterest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [BsonElement("href")]
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, ErrorMessage = "Name darf maximal 100 Zeichen lang sein")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("category")]
    [JsonPropertyName("category")]
    [Required(ErrorMessage = "Kategorie ist erforderlich")]
    [StringLength(50, ErrorMessage = "Kategorie darf maximal 50 Zeichen lang sein")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("location")]
    [JsonPropertyName("location")]
    [Required(ErrorMessage = "Location ist erforderlich")]
    public Location Location { get; set; } = new();

    [BsonElement("address")]
    [JsonPropertyName("address")]
    [StringLength(200, ErrorMessage = "Adresse darf maximal 200 Zeichen lang sein")]
    public string? Address { get; set; }

    [BsonElement("tags")]
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Generiert die HREF-URL für diesen POI (kompatibel mit JEE Backend)
    /// </summary>
    public void GenerateHref()
    {
        if (!string.IsNullOrEmpty(Id))
        {
            Href = $"/api/pois/{Id}";
        }
    }
}

/// <summary>
/// Location (Koordinaten) für einen Point of Interest
/// </summary>
public class Location
{
    [BsonElement("longitude")]
    [JsonPropertyName("longitude")]
    [Required(ErrorMessage = "Longitude ist erforderlich")]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude muss zwischen -180 und 180 liegen")]
    public double Longitude { get; set; }

    [BsonElement("latitude")]
    [JsonPropertyName("latitude")]
    [Required(ErrorMessage = "Latitude ist erforderlich")]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude muss zwischen -90 und 90 liegen")]
    public double Latitude { get; set; }

    public Location() { }

    public Location(double longitude, double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }

    /// <summary>
    /// Berechnet die Entfernung zu einer anderen Location in Kilometern
    /// Verwendet die Haversine-Formel
    /// </summary>
    public double DistanceTo(Location other)
    {
        if (other == null) return double.MaxValue;

        const double earthRadiusKm = 6371.0;
        
        var dLat = DegreesToRadians(other.Latitude - Latitude);
        var dLon = DegreesToRadians(other.Longitude - Longitude);
        
        var lat1 = DegreesToRadians(Latitude);
        var lat2 = DegreesToRadians(other.Latitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    public override string ToString()
    {
        return $"Location(Longitude: {Longitude}, Latitude: {Latitude})";
    }
}