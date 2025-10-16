using DotNetMongoDbBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotNetMongoDbBackend.Controllers;

/// <summary>
/// REST Controller for Statistics operations
/// Separated from PointOfInterestController for better organization
/// </summary>
[ApiController]
[Route("stats")]
public class StatsController : ControllerBase
{
    private readonly IPointOfInterestService _poiService;
    private readonly ILogger<StatsController> _logger;

    public StatsController(IPointOfInterestService poiService, ILogger<StatsController> logger)
    {
        _poiService = poiService;
        _logger = logger;
    }

    /// <summary>
    /// GET /stats/category/{category} - Statistics for category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <returns>Number of POIs in category</returns>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<long>> GetCategoryCount([Required] string category)
    {
        try
        {
            var count = await _poiService.CountByCategoryAsync(category);

            _logger.LogInformation("Category statistics retrieved: {Category} has {Count} POIs", category, count);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for category: {Category}", category);
            return StatusCode(500, "Internal server error retrieving category statistics");
        }
    }
}

