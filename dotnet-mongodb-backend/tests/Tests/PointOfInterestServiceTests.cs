public class PointOfInterestTests
{
    [Fact]
    public async Task GetAllPoisAsync_ShouldReturnPois()
    {
        // Arrange
        var mockDatabase = CreateMockDatabase();
        var service = new PointOfInterestService(mockDatabase, Mock.of<ILogger<PointOfInterestService>>());

        // Act
        var result = await service.GetAllPoisAsync();

        // Assert
        Assert.NotNull(result)
    }
}