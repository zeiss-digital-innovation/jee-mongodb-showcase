using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DotNetMongoDbBackend.Models.Entities;


public class PointOfInterestEntity
/// <summary>
/// Point of Interest Model für MongoDB
/// Kompatibel mit dem JEE und Spring Boot Backend Schema
/// </summary>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("category")]
    public string? Category { get; set; }

    [BsonElement("details")]
    public string? Details { get; set; }

    [BsonElement("location")]
    public LocationEntity? Location { get; set; }

    // Optional fields that may not be present in all documents
    [BsonElement("name")]
    [BsonIgnoreIfNull]
    public string? Name { get; set; }

    [BsonElement("tags")]
    [BsonIgnoreIfNull]
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Location (Koordinaten) für einen Point of Interest - MongoDB GeoJSON kompatibel
/// </summary>
public class LocationEntity
{
    [BsonElement("type")]
    public string Type { get; set; } = "Point";

    [BsonElement("coordinates")]
    public double[] Coordinates { get; set; } = new double[2];
}