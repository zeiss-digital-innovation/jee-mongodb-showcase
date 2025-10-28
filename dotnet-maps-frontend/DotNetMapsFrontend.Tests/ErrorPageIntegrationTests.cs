#nullable disable

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace DotNetMapsFrontend.Tests;

/// <summary>
/// Integration tests for Error handling and Error view
/// </summary>
[TestFixture]
public class ErrorPageIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task ErrorPage_Returns200StatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Home/Error");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ErrorPage_ReturnsHtmlContent()
    {
        // Act
        var response = await _client.GetAsync("/Home/Error");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
        Assert.That(content, Is.Not.Empty);
    }

    [Test]
    public async Task ErrorPage_ContainsErrorMessage()
    {
        // Act
        var response = await _client.GetAsync("/Home/Error");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(content, Does.Contain("Error"));
    }

    [Test]
    public async Task InvalidRoute_RedirectsToError()
    {
        // Act - Try to access a non-existent route
        var response = await _client.GetAsync("/NonExistentRoute/Something");

        // Assert - Should either get 404 or redirect to error page
        Assert.That(
            response.StatusCode == HttpStatusCode.NotFound || 
            response.StatusCode == HttpStatusCode.OK,
            Is.True
        );
    }
}
