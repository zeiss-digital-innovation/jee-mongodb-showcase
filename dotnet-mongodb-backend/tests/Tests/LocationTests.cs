using DotNetMongoDbBackend.Models;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests;

/// <summary>
/// Unit tests for Location model
/// Tests constructors, properties, and utility methods
/// </summary>
public class LocationTests
{
    #region Constructor Tests

    [Fact]
    public void Location_DefaultConstructor_InitializesCoordinates()
    {
        // Act
        var location = new Location();

        // Assert
        Assert.NotNull(location.Coordinates);
        Assert.Equal(2, location.Coordinates.Length);
        Assert.Equal("Point", location.Type);
        Assert.Equal(0, location.Longitude);
        Assert.Equal(0, location.Latitude);
    }

    [Fact]
    public void Location_ParameterizedConstructor_SetsCoordinatesCorrectly()
    {
        // Arrange & Act
        var location = new Location(13.7373, 51.0504);

        // Assert
        Assert.Equal("Point", location.Type);
        Assert.Equal(13.7373, location.Longitude);
        Assert.Equal(51.0504, location.Latitude);
        Assert.Equal(13.7373, location.Coordinates[0]);
        Assert.Equal(51.0504, location.Coordinates[1]);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Location_SetLongitude_UpdatesCoordinates()
    {
        // Arrange
        var location = new Location();

        // Act
        location.Longitude = 8.4;

        // Assert
        Assert.Equal(8.4, location.Longitude);
        Assert.Equal(8.4, location.Coordinates[0]);
    }

    [Fact]
    public void Location_SetLatitude_UpdatesCoordinates()
    {
        // Arrange
        var location = new Location();

        // Act
        location.Latitude = 49.0;

        // Assert
        Assert.Equal(49.0, location.Latitude);
        Assert.Equal(49.0, location.Coordinates[1]);
    }

    [Fact]
    public void Location_SetCoordinates_UpdatesProperties()
    {
        // Arrange
        var location = new Location();

        // Act
        location.Coordinates = new double[] { 13.7373, 51.0504 };

        // Assert
        Assert.Equal(13.7373, location.Longitude);
        Assert.Equal(51.0504, location.Latitude);
    }

    #endregion

    #region DistanceTo Tests

    [Fact]
    public void DistanceTo_WithNullLocation_ReturnsMaxValue()
    {
        // Arrange
        var location = new Location(13.7373, 51.0504);

        // Act
        var distance = location.DistanceTo(null!);

        // Assert
        Assert.Equal(double.MaxValue, distance);
    }

    [Fact]
    public void DistanceTo_WithSameLocation_ReturnsZero()
    {
        // Arrange
        var location1 = new Location(13.7373, 51.0504);
        var location2 = new Location(13.7373, 51.0504);

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert
        Assert.True(distance < 0.001, $"Expected distance near 0, but got {distance}");
    }

    [Fact]
    public void DistanceTo_WithKnownDistance_ReturnsCorrectValue()
    {
        // Arrange - Dresden to Berlin (approx. 165 km)
        var dresden = new Location(13.7373, 51.0504);
        var berlin = new Location(13.4050, 52.5200);

        // Act
        var distance = dresden.DistanceTo(berlin);

        // Assert - Distance should be approximately 165 km (±10 km tolerance)
        Assert.True(distance > 155 && distance < 175, 
            $"Expected distance between Dresden and Berlin ~165 km, but got {distance:F2} km");
    }

    [Fact]
    public void DistanceTo_WithOppositeHemispheres_ReturnsLargeDistance()
    {
        // Arrange - New York to Sydney
        var newYork = new Location(-74.0060, 40.7128);
        var sydney = new Location(151.2093, -33.8688);

        // Act
        var distance = newYork.DistanceTo(sydney);

        // Assert - Distance should be approximately 16,000 km
        Assert.True(distance > 15000, 
            $"Expected distance > 15,000 km, but got {distance:F2} km");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var location = new Location(13.7373, 51.0504);

        // Act
        var result = location.ToString();

        // Assert - Check for culture-independent parts
        Assert.Contains("Longitude", result);
        Assert.Contains("Latitude", result);
        Assert.Contains("Location", result);
        Assert.Contains("13", result); // Culture-independent check
        Assert.Contains("51", result); // Culture-independent check
    }

    [Fact]
    public void ToString_WithZeroCoordinates_ReturnsFormattedString()
    {
        // Arrange
        var location = new Location(0, 0);

        // Act
        var result = location.ToString();

        // Assert
        Assert.Contains("0", result);
        Assert.Contains("Longitude", result);
        Assert.Contains("Latitude", result);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Location_WithExtremeCoordinates_HandlesCorrectly()
    {
        // Arrange & Act - Date Line and Equator
        var location = new Location(180, 0);

        // Assert
        Assert.Equal(180, location.Longitude);
        Assert.Equal(0, location.Latitude);
    }

    [Fact]
    public void Location_WithPoleCoordinates_HandlesCorrectly()
    {
        // Arrange & Act - North Pole
        var northPole = new Location(0, 90);

        // Assert
        Assert.Equal(0, northPole.Longitude);
        Assert.Equal(90, northPole.Latitude);
    }

    [Fact]
    public void Location_WithNegativeCoordinates_HandlesCorrectly()
    {
        // Arrange & Act - South America
        var location = new Location(-60, -30);

        // Assert
        Assert.Equal(-60, location.Longitude);
        Assert.Equal(-30, location.Latitude);
    }

    #endregion
}
