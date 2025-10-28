using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotNetMongoDbBackend.Controllers;

/// <summary>
/// REST Controller for Points of Interest
/// Compatible API with JEE and Spring Boot Backend
/// </summary>
[ApiController]
[Route("poi")]
public class PointOfInterestController : ControllerBase
{
    private readonly IPointOfInterestService _poiService;
    private readonly ILogger<PointOfInterestController> _logger;
    private readonly LinkGenerator? _linkGenerator;

    public PointOfInterestController(IPointOfInterestService poiService, ILogger<PointOfInterestController> logger, LinkGenerator? linkGenerator = null)
    {
        _poiService = poiService;
        _logger = logger;
        _linkGenerator = linkGenerator;
    }

    /// <summary>
    /// Encapsulates URL generation for a POI so tests can override behavior without mocking LinkGenerator.
    /// Default implementation sets the Href property using LinkGenerator when available.
    /// </summary>
    protected virtual void GenerateHref(PointOfInterest p)
    {
        try
        {
            if (p != null && !string.IsNullOrWhiteSpace(p.Id))
            {
                var uri = _linkGenerator?.GetUriByAction(HttpContext, action: nameof(GetPoiById), controller: "PointOfInterest", values: new { id = p.Id });
                p.Href = uri;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating href for POI with ID: {Id}", p?.Id);
        }
    }

    /// <summary>
    /// GET /poi - Get all POIs with optional filters
    /// BACKWARD COMPATIBLE: Old parameters still work
    /// NEW: Multiple category filtering via repeated 'category' parameter
    /// </summary>
    /// <param name="category">Filter by single category OR multiple categories (repeated parameter)</param>
    /// <param name="search">Full-text search in name and tags</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <param name="lat">Latitude for geographic search</param>
    /// <param name="lng">Longitude for geographic search</param>
    /// <param name="lon">Alternative longitude parameter (frontend compatibility)</param>
    /// <param name="radius">Radius for geographic search in meters</param>
    /// <returns>List of POIs</returns>
    [HttpGet]
    public async Task<ActionResult<List<PointOfInterest>>> GetAllPois(
        [FromQuery] List<string>? category = null,  // CHANGED: Now accepts multiple categories
        [FromQuery] string? search = null,
        [FromQuery] int? limit = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double? lon = null,
        [FromQuery] double? radius = null)
    {
        try
        {
            List<PointOfInterest> pois;

            // Normalize lon/lng parameter
            double? longitude = lng ?? lon;

            _logger.LogInformation("GetAllPois called: lat={Lat}, lng={Lng}, lon={Lon}, radius={Radius}, categories={Categories}",
                lat, lng, lon, radius, category != null ? string.Join(", ", category) : "none");

            // PRIORITY 1: Geographic search with category filter (NEW!)
            if (lat.HasValue && longitude.HasValue && category != null && category.Count > 0)
            {
                var radiusMeters = radius ?? 10000.0; // Default 10km
                var radiusKm = radiusMeters / 1000.0;

                _logger.LogInformation("Performing geographic search WITH category filter: lat={Lat}, lng={Lng}, radius={Radius}m, categories=[{Categories}]",
                    lat, longitude, radiusMeters, string.Join(", ", category));

                pois = await _poiService.GetNearbyPoisByCategoriesAsync(longitude.Value, lat.Value, radiusKm, category);

                _logger.LogInformation("Geographic search with categories completed: {Count} POIs found", pois.Count);
            }
            // PRIORITY 2: Geographic search WITHOUT category filter (BACKWARD COMPATIBLE)
            else if (lat.HasValue && longitude.HasValue)
            {
                var radiusMeters = radius ?? 10000.0;
                var radiusKm = radiusMeters / 1000.0;

                _logger.LogInformation("Performing geographic search: lat={Lat}, lng={Lng}, radius={Radius}m ({RadiusKm}km)",
                    lat, longitude, radiusMeters, radiusKm);

                pois = await _poiService.GetNearbyPoisAsync(longitude.Value, lat.Value, radiusKm);

                _logger.LogInformation("Geographic search completed: {Count} POIs found within radius {Radius}m",
                    pois.Count, radiusMeters);
            }
            // PRIORITY 3: Category filter only (BACKWARD COMPATIBLE - single category)
            else if (category != null && category.Count == 1 && !string.IsNullOrWhiteSpace(category[0]))
            {
                _logger.LogInformation("Performing single category filter: {Category}", category[0]);
                pois = await _poiService.GetPoisByCategoryAsync(category[0]);
            }
            // PRIORITY 4: Full-text search
            else if (!string.IsNullOrWhiteSpace(search))
            {
                _logger.LogInformation("Performing full-text search: {Search}, limit={Limit}", search, limit);
                pois = await _poiService.SearchPoisAsync(search, limit);
            }
            // PRIORITY 5: All POIs (fallback)
            else
            {
                _logger.LogWarning("Fallback: All POIs retrieved (no filters specified) - this can be very slow!");
                pois = await _poiService.GetAllPoisAsync();
            }

            // Set absolute href for each poi
            foreach (var p in pois)
            {
                GenerateHref(p);
            }

            _logger.LogInformation("POIs retrieved: {Count} results", pois.Count);
            return Ok(pois);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving POIs");
            return StatusCode(500, "Internal server error retrieving POIs");
        }
    }

    /// <summary>
    /// GET /poi/{id} - Get POI by ID
    /// RFC 9110 Section 9.3.1 (GET) and Section 15.5.5 (404 Not Found)
    /// Compatible with JEE Backend: Returns 404 without body when POI not found
    /// </summary>
    /// <param name="id">MongoDB ObjectId of the POI</param>
    /// <returns>POI or 404 if not found</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PointOfInterest>> GetPoiById([Required] string id)
    {
        try
        {
            var poi = await _poiService.GetPoiByIdAsync(id);

            if (poi == null)
            {
                _logger.LogWarning("POI not found with ID: {Id}", id);
                // JEE-compatible: Return 404 without body (throws NotFoundException in JEE)
                return NotFound();
            }

            _logger.LogInformation("POI retrieved: {Name} (ID: {Id})", poi.Name, poi.Id);
            GenerateHref(poi);
            return Ok(poi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving POI with ID: {Id}", id);
            return StatusCode(500, "Internal server error retrieving POI");
        }
    }

    /// <summary>
    /// POST /poi - Create new POI
    /// Compatible with JEE Backend: Returns HTTP 201 with Location header, but NO body
    /// Implements RFC 9110 Section 10.2.2 (Location header)
    /// </summary>
    /// <param name="poi">POI data</param>
    /// <returns>HTTP 201 Created with Location header</returns>
    [HttpPost]
    public async Task<ActionResult> CreatePoi([FromBody] PointOfInterest poi)
    {
        try
        {
            // NOTE: ModelState validation is automatically performed by [ApiController]
            // Manual check here would cause problems in unit tests

            var createdPoi = await _poiService.CreatePoiAsync(poi);

            _logger.LogInformation("POI created: {Name} (ID: {Id})", createdPoi.Name, createdPoi.Id);

            // RFC 9110 Section 10.2.2: Location header can be absolute or relative URI
            // Prefer absolute URI for better interoperability (matching JEE implementation)
            string? locationUri = null;

            if (Url != null && HttpContext?.Request != null)
            {
                try
                {
                    // Try to generate absolute URI using Url.Action
                    locationUri = Url.Action(
                        action: nameof(GetPoiById),
                        controller: null,
                        values: new { id = createdPoi.Id },
                        protocol: HttpContext.Request.Scheme,
                        host: HttpContext.Request.Host.ToString()
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error generating absolute Location URI with Url.Action");
                }
            }

            // Fallback: construct absolute URI manually if Url helper failed
            if (string.IsNullOrEmpty(locationUri) && HttpContext?.Request != null)
            {
                var request = HttpContext.Request;
                var baseUri = $"{request.Scheme}://{request.Host}{request.PathBase}";
                locationUri = $"{baseUri}/poi/{createdPoi.Id}";
            }
            // Last resort fallback: relative URI (valid per RFC 9110, but less preferred)
            else if (string.IsNullOrEmpty(locationUri))
            {
                locationUri = $"/poi/{createdPoi.Id}";
            }

            Response.Headers.Location = locationUri;
            return StatusCode(201);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid POI data during creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating POI: {Name}", poi?.Name);
            return StatusCode(500, "Internal server error creating POI");
        }
    }

    /// <summary>
    /// PUT /geoservice/poi/{id} - Update POI
    /// </summary>
    /// <param name="id">MongoDB ObjectId of the POI</param>
    /// <param name="poi">Updated POI data</param>
    /// <returns>Updated POI or 404 if not found</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<PointOfInterest>> UpdatePoi([Required] string id, [FromBody] PointOfInterest poi)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedPoi = await _poiService.UpdatePoiAsync(id, poi);

            if (updatedPoi == null)
            {
                _logger.LogWarning("POI not found for update with ID: {Id}", id);
                return NotFound($"POI with ID '{id}' was not found");
            }

            _logger.LogInformation("POI updated: {Name} (ID: {Id})", updatedPoi.Name, updatedPoi.Id);
            return Ok(updatedPoi);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid POI data during update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating POI with ID: {Id}", id);
            return StatusCode(500, "Internal server error updating POI");
        }
    }

    /// <summary>
    /// DELETE /poi/{id} - Delete POI
    /// RFC 9110 Section 9.3.5 (DELETE) - Returns 204 No Content regardless of resource existence
    /// Compatible with JEE Backend: Always returns 204, even if POI doesn't exist (idempotent)
    /// </summary>
    /// <param name="id">MongoDB ObjectId of the POI</param>
    /// <returns>204 No Content</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePoi([Required] string id)
    {
        try
        {
            var deleted = await _poiService.DeletePoiAsync(id);

            if (deleted)
            {
                _logger.LogInformation("POI deleted with ID: {Id}", id);
            }
            else
            {
                _logger.LogWarning("POI not found for deletion with ID: {Id}, returning 204 anyway (idempotent)", id);
            }

            // JEE-compatible: Always return 204 No Content (idempotent DELETE)
            // RFC 9110: DELETE should be idempotent, same result for multiple calls
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting POI with ID: {Id}", id);
            return StatusCode(500, "Internal server error deleting POI");
        }
    }


    /// <summary>
    /// GET /api/health - Health Check Endpoint
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    public ActionResult<string> HealthCheck()
    {
        return Ok(".NET MongoDB Backend is running");
    }

    /// <summary>
    /// GET /geoservice/debug - Debug endpoint for MongoDB connection
    /// </summary>
    [HttpGet("debug")]
    public async Task<ActionResult<object>> DebugMongoConnection()
    {
        try
        {
            var debugInfo = new
            {
                ServiceInjected = _poiService != null,
                CollectionTest = "being tested...",
                TotalCount = 0,
                ToiletCount = 0,
                Error = (string?)null
            };

            if (_poiService != null)
            {
                try
                {
                    var allPois = await _poiService.GetAllPoisAsync();
                    var toiletPois = await _poiService.GetPoisByCategoryAsync("toilet");

                    debugInfo = new
                    {
                        ServiceInjected = true,
                        CollectionTest = "successful",
                        TotalCount = allPois.Count,
                        ToiletCount = toiletPois.Count,
                        Error = (string?)null
                    };
                }
                catch (Exception ex)
                {
                    debugInfo = new
                    {
                        ServiceInjected = true,
                        CollectionTest = "failed",
                        TotalCount = 0,
                        ToiletCount = 0,
                        Error = (string?)ex.Message
                    };
                }
            }

            return Ok(debugInfo);
        }
        catch (Exception ex)
        {
            return Ok(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    
}