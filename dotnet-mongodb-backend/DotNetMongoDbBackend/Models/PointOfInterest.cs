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
    [JsonIgnore]
    public string? Id { get; set; }

    [BsonElement("href")]
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    [BsonElement("category")]
    [JsonPropertyName("category")]
    [Required(ErrorMessage = "Category is required")]
    public string? Category { get; set; }

    [BsonElement("details")]
    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [BsonElement("location")]
    [JsonPropertyName("location")]
    public Location? Location { get; set; }

    // Optionale Felder die möglicherweise nicht in allen Dokumenten vorhanden sind
    [BsonElement("name")]
    [JsonPropertyName("name")]
    [BsonIgnoreIfNull]
    public string? Name { get; set; }

    [BsonElement("address")]
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [BsonElement("tags")]
    [JsonPropertyName("tags")]
    [BsonIgnoreIfNull]
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Location (Koordinaten) für einen Point of Interest - MongoDB GeoJSON kompatibel
/// </summary>
public class Location
{
    [BsonElement("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Point";

    [BsonElement("coordinates")]
    [JsonPropertyName("coordinates")]
    [Required(ErrorMessage = "Coordinates are required")]
    public double[] Coordinates { get; set; } = new double[2];

    // Convenience Properties für bessere API-Kompatibilität
    [BsonIgnore]
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

    [BsonIgnore]
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

    public Location()
    {
        Coordinates = new double[2];
    }

    public Location(double longitude, double latitude)
    {
        Type = "Point";
        Coordinates = new double[] { longitude, latitude };
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