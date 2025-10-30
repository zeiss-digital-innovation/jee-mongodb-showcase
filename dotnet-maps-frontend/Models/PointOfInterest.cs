using System.Text.Json.Serialization;

namespace DotNetMapsFrontend.Models
{
    public class PointOfInterest
    {
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public string Details { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public Location Location { get; set; } = new Location();
    }

    public class Location
    {
        [JsonPropertyName("coordinates")]
        public double[] Coordinates { get; set; } = new double[2];

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Point";

        public double Longitude => Coordinates.Length > 0 ? Coordinates[0] : 0;
        public double Latitude => Coordinates.Length > 1 ? Coordinates[1] : 0;
    }
}