#nullable disable

using Moq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using DotNetMapsFrontend.Services;
using DotNetMapsFrontend.Controllers;
using DotNetMapsFrontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Html.Dom;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Tests for the POI filtering functionality on both List and Map pages
/// </summary>
[TestFixture]
public class PointOfInterestFilterTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private Mock<IPointOfInterestService> _mockService;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IPointOfInterestService>();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing service registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IPointOfInterestService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add mock service
                    services.AddSingleton(_mockService.Object);
                });
            });

        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldContainFilterInput()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("poiFilterInput"));
        Assert.That(content, Does.Contain("Filter POIs by Details"));
        Assert.That(content, Does.Contain("Enter text to filter POIs..."));
    }

    [Test]
    public async Task MapIndex_ShouldContainFilterInput()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("poiFilterInput"));
        Assert.That(content, Does.Contain("Filter POIs by Details"));
        Assert.That(content, Does.Contain("Enter text to filter POIs..."));
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldContainFilterFunctionality()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check for filter JavaScript functions
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("setupFilterFunctionality"));
        Assert.That(content, Does.Contain("applyFilter"));
        Assert.That(content, Does.Contain("localStorage.getItem('poi_filter')"));
        Assert.That(content, Does.Contain("localStorage.setItem('poi_filter'"));
    }

    [Test]
    public async Task MapIndex_ShouldContainFilterFunctionality()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check for filter JavaScript functions
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("setupFilterFunctionality"));
        Assert.That(content, Does.Contain("applyMapFilter"));
        Assert.That(content, Does.Contain("localStorage.getItem('poi_filter')"));
        Assert.That(content, Does.Contain("localStorage.setItem('poi_filter'"));
    }

    [Test]
    public async Task PointOfInterestIndex_FilterInput_ShouldBeProperlyLabeled()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();
        
        // Parse HTML
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(content));

        // Assert
        var filterInput = document.QuerySelector("#poiFilterInput");
        Assert.That(filterInput, Is.Not.Null, "Filter input field should exist");
        
        var label = document.QuerySelector("label[for='poiFilterInput']");
        Assert.That(label, Is.Not.Null, "Label for filter input should exist");
        Assert.That(label.TextContent.Trim(), Does.Contain("Filter POIs by Details"));
    }

    [Test]
    public async Task MapIndex_FilterInput_ShouldBeProperlyLabeled()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();
        
        // Parse HTML
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(content));

        // Assert
        var filterInput = document.QuerySelector("#poiFilterInput");
        Assert.That(filterInput, Is.Not.Null, "Filter input field should exist");
        
        var label = document.QuerySelector("label[for='poiFilterInput']");
        Assert.That(label, Is.Not.Null, "Label for filter input should exist");
        Assert.That(label.TextContent.Trim(), Does.Contain("Filter POIs by Details"));
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldRenderAllPOIsWithoutFilter()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();
        
        // Parse HTML
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(content));

        // Assert - Check that all POIs are present in the rendered HTML
        Assert.That(content, Does.Contain("Best Italian Restaurant"));
        Assert.That(content, Does.Contain("Coffee shop with great atmosphere"));
        Assert.That(content, Does.Contain("ATM with low fees"));
    }

    [Test]
    public async Task PointOfInterestIndex_FilterLogic_ShouldBeCaseInsensitive()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that the filter uses toLowerCase()
        Assert.That(content, Does.Contain(".toLowerCase()"));
        Assert.That(content, Does.Contain("filterLower"));
    }

    [Test]
    public async Task MapIndex_FilterLogic_ShouldBeCaseInsensitive()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that the filter uses toLowerCase()
        Assert.That(content, Does.Contain(".toLowerCase()"));
        Assert.That(content, Does.Contain("filterLower"));
    }

    [Test]
    public async Task PointOfInterestIndex_ShouldHaveFilterInputEventListener()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that input event listener is attached
        Assert.That(content, Does.Contain("filterInput.on('input'"));
    }

    [Test]
    public async Task MapIndex_ShouldHaveFilterInputEventListener()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that input event listener is attached
        Assert.That(content, Does.Contain("filterInput.on('input'"));
    }

    [Test]
    public async Task PointOfInterestIndex_FilterFunction_ShouldFilterCardsView()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that filter function handles both cards and table
        Assert.That(content, Does.Contain("#poiCardsContainer .col"));
        Assert.That(content, Does.Contain(".card-text"));
        Assert.That(content, Does.Contain(".show()"));
        Assert.That(content, Does.Contain(".hide()"));
    }

    [Test]
    public async Task PointOfInterestIndex_FilterFunction_ShouldFilterTableView()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that filter function handles table rows
        Assert.That(content, Does.Contain("#poiTableBody tr"));
        Assert.That(content, Does.Contain(".details-cell"));
    }

    [Test]
    public async Task MapIndex_FilterFunction_ShouldFilterMarkers()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that filter function handles map markers
        Assert.That(content, Does.Contain("markersLayer.clearLayers()"));
        Assert.That(content, Does.Contain("poi.details"));
        Assert.That(content, Does.Contain("details.includes(filterLower)"));
    }

    [Test]
    public async Task PointOfInterestIndex_FilterShouldApplyAfterPOIUpdate()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that filter is applied after updating POI cards/table
        Assert.That(content, Does.Contain("// Apply current filter after updating cards"));
        Assert.That(content, Does.Contain("// Apply current filter after updating table"));
        Assert.That(content, Does.Contain("const currentFilter = $('#poiFilterInput').val()"));
    }

    [Test]
    public async Task MapIndex_FilterShouldApplyAfterPOIUpdate()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that filter is applied when POIs are displayed
        Assert.That(content, Does.Contain("// Apply filter if exists"));
        Assert.That(content, Does.Contain("const currentFilter = $('#poiFilterInput').val()"));
        Assert.That(content, Does.Contain("applyMapFilter(currentFilter)"));
    }

    [Test]
    public async Task PointOfInterestIndex_EmptyFilter_ShouldShowAllPOIs()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act
        var response = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that empty filter shows all POIs
        Assert.That(content, Does.Contain("filterLower === ''"));
    }

    [Test]
    public async Task MapIndex_EmptyFilter_ShouldShowAllMarkers()
    {
        // Act
        var response = await _client.GetAsync("/Map");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Check that empty filter shows all markers
        Assert.That(content, Does.Contain("filterLower === ''"));
    }

    [Test]
    public async Task FilterInput_ShouldHavePlaceholderText()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act - Check Point of Interest List page
        var response1 = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content1 = await response1.Content.ReadAsStringAsync();
        
        // Parse HTML
        var context1 = BrowsingContext.New(Configuration.Default);
        var document1 = await context1.OpenAsync(req => req.Content(content1));
        var filterInput1 = document1.QuerySelector("#poiFilterInput") as IHtmlInputElement;
        
        // Act - Check Map page
        var response2 = await _client.GetAsync("/Map");
        var content2 = await response2.Content.ReadAsStringAsync();
        
        var context2 = BrowsingContext.New(Configuration.Default);
        var document2 = await context2.OpenAsync(req => req.Content(content2));
        var filterInput2 = document2.QuerySelector("#poiFilterInput") as IHtmlInputElement;

        // Assert
        Assert.That(filterInput1, Is.Not.Null);
        Assert.That(filterInput1.Placeholder, Is.EqualTo("Enter text to filter POIs..."));
        
        Assert.That(filterInput2, Is.Not.Null);
        Assert.That(filterInput2.Placeholder, Is.EqualTo("Enter text to filter POIs..."));
    }

    [Test]
    public async Task FilterStorage_ShouldUseSameLocalStorageKey()
    {
        // Arrange
        var testPois = GetTestPointsOfInterest();
        _mockService.Setup(s => s.GetPointsOfInterestAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(testPois);

        // Act - Check both pages use the same localStorage key
        var response1 = await _client.GetAsync("/PointOfInterest?lat=51.0504&lon=13.7373&radius=2000");
        var content1 = await response1.Content.ReadAsStringAsync();
        
        var response2 = await _client.GetAsync("/Map");
        var content2 = await response2.Content.ReadAsStringAsync();

        // Assert - Both pages should use 'poi_filter' key
        Assert.That(content1, Does.Contain("'poi_filter'"));
        Assert.That(content2, Does.Contain("'poi_filter'"));
    }

    /// <summary>
    /// Helper method to create test POIs
    /// </summary>
    private List<PointOfInterest> GetTestPointsOfInterest()
    {
        return new List<PointOfInterest>
        {
            new PointOfInterest
            {
                Category = "restaurant",
                Details = "Best Italian Restaurant in town",
                Location = new Location
                {
                    Type = "Point",
                    Coordinates = new[] { 13.7373, 51.0504 }
                },
                Href = "/poi/1"
            },
            new PointOfInterest
            {
                Category = "coffee",
                Details = "Coffee shop with great atmosphere",
                Location = new Location
                {
                    Type = "Point",
                    Coordinates = new[] { 13.7383, 51.0514 }
                },
                Href = "/poi/2"
            },
            new PointOfInterest
            {
                Category = "cash",
                Details = "ATM with low fees",
                Location = new Location
                {
                    Type = "Point",
                    Coordinates = new[] { 13.7393, 51.0524 }
                },
                Href = "/poi/3"
            }
        };
    }
}
