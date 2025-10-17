using DotNetMongoDbBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotNetMongoDbBackend.Controllers;

/// <summary>
/// REST Controller for Category operations
/// Separated from PointOfInterestController for better organization
/// </summary>
[ApiController]
[Route("categories")]
public class CategoryController : ControllerBase
{
    private readonly IPointOfInterestService _poiService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(IPointOfInterestService poiService, ILogger<CategoryController> logger)
    {
        _poiService = poiService;
        _logger = logger;
    }

    /// <summary>
    /// GET /categories - Get all available categories
    /// </summary>
    /// <returns>List of available categories</returns>
    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAvailableCategories()
    {
        try
        {
            var categories = await _poiService.GetAvailableCategoriesAsync();

            _logger.LogInformation("Categories retrieved: {Count} available categories", categories.Count);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available categories");
            return StatusCode(500, "Internal server error retrieving categories");
        }
    }
}

