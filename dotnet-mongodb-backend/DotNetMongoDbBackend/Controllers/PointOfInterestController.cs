using DotNetMongoDbBackend.Mappers;
using DotNetMongoDbBackend.Models.DTOs;
using DotNetMongoDbBackend.Models.Entities;
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
    protected virtual void GenerateHref(PointOfInterestDto poiDto)
    {
        try
        {
            if (poiDto != null && !string.IsNullOrWhiteSpace(poiDto.Id))
            {
                var uri = _linkGenerator?.GetUriByAction(HttpContext, action: nameof(GetPoiById), controller: "PointOfInterest", values: new { id = poiDto.Id });
                poiDto.Href = uri;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating href for POI with ID: {Id}", poiDto?.Id);
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
    public async Task<ActionResult<List<PointOfInterestDto>>> GetAllPois(
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
            // service sends entities
            List<PointOfInterestEntity> entities;
            
            // method returns DTOs
            List<PointOfInterestDto> dtos;

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

                entities = await _poiService.GetNearbyPoisByCategoriesAsync(longitude.Value, lat.Value, radiusKm, category);

                _logger.LogInformation("Geographic search with categories completed: {Count} POIs found", entities.Count);
            }
            // PRIORITY 2: Geographic search WITHOUT category filter (BACKWARD COMPATIBLE)
            else if (lat.HasValue && longitude.HasValue)
            {
                var radiusMeters = radius ?? 10000.0;
                var radiusKm = radiusMeters / 1000.0;

                _logger.LogInformation("Performing geographic search: lat={Lat}, lng={Lng}, radius={Radius}m ({RadiusKm}km)",
                    lat, longitude, radiusMeters, radiusKm);

                entities = await _poiService.GetNearbyPoisAsync(longitude.Value, lat.Value, radiusKm);

                _logger.LogInformation("Geographic search completed: {Count} POIs found within radius {Radius}m",
                    entities.Count, radiusMeters);
            }
            // PRIORITY 3: Category filter only (BACKWARD COMPATIBLE - single category)
            else if (category != null && category.Count == 1 && !string.IsNullOrWhiteSpace(category[0]))
            {
                _logger.LogInformation("Performing single category filter: {Category}", category[0]);
                entities = await _poiService.GetPoisByCategoryAsync(category[0]);
            }
            // PRIORITY 4: Full-text search
            else if (!string.IsNullOrWhiteSpace(search))
            {
                _logger.LogInformation("Performing full-text search: {Search}, limit={Limit}", search, limit);
                entities = await _poiService.SearchPoisAsync(search, limit);
            }
            // PRIORITY 5: All POIs (fallback)
            else
            {
                _logger.LogWarning("Fallback: All POIs retrieved (no filters specified) - this can be very slow!");
                entities = await _poiService.GetAllPoisAsync();
            }
            
            _logger.LogInformation("POIs retrieved: {Count} results", entities.Count);

            dtos = PointOfInterestMapper.ToDtoList(entities);
            // Set absolute href for each poi
            foreach (var dto in dtos)
            {
                GenerateHref(dto);
            }
            // use limit if exists
            if (limit.HasValue && limit.Value > 0)
            {
                dtos = [.. dtos.Take(limit.Value)];
            }

            return Ok(dtos);
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
    public async Task<ActionResult<PointOfInterestDto>> GetPoiById([Required] string id)
    {
        try
        {
            var poiEntity = await _poiService.GetPoiByIdAsync(id);

            if (poiEntity == null)
            {
                _logger.LogWarning("POI not found with ID: {Id}", id);
                // JEE-compatible: Return 404 without body (throws NotFoundException in JEE)
                return NotFound();
            }

            _logger.LogInformation("POI retrieved: {Name} (ID: {Id})", poiEntity.Name, poiEntity.Id);
            // Entity to DTO transformation
            PointOfInterestDto poiDto = PointOfInterestMapper.ToDto(poiEntity);
            // href generation
            GenerateHref(poiDto);
            return Ok(poiDto);
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
    /// <param name="poiDto">POI data</param>
    /// <returns>HTTP 201 Created with Location header</returns>
    [HttpPost]
    public async Task<ActionResult> CreatePoi([FromBody] PointOfInterestDto poiDto)
    {
        try
        {
            // validation happens automatically over DTO-attributes
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // additional business-check
            if (poiDto.Location?.Coordinates == null || poiDto.Location.Coordinates.Length != 2)
            {
                return BadRequest(new { message = "Valid coordinates [longitude, lattitude] are required" });
            }

            // NOTE: ModelState validation is automatically performed by [ApiController]
            // Manual check here would cause problems in unit tests
            var createdPoiDto = PointOfInterestMapper.ToDto(await _poiService.CreatePoiAsync(PointOfInterestMapper.ToEntity(poiDto)));
            GenerateHref(createdPoiDto);


            _logger.LogInformation("POI created: {Name} (ID: {Id})", createdPoiDto.Name, createdPoiDto.Id);

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
                        values: new { id = createdPoiDto.Id },
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
                locationUri = $"{baseUri}/poi/{createdPoiDto.Id}";
            }
            // Last resort fallback: relative URI (valid per RFC 9110, but less preferred)
            else if (string.IsNullOrEmpty(locationUri))
            {
                locationUri = $"/poi/{createdPoiDto.Id}";
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
            _logger.LogError(ex, "Error creating POI: {Name}", poiDto?.Name);
            return StatusCode(500, "Internal server error creating POI");
        }
    }

    /// <summary>
    /// PUT /geoservice/poi/{id} - Update POI
    /// </summary>
    /// <param name="id">MongoDB ObjectId of the POI</param>
    /// <param name="poiDto">Updated POI data</param>
    /// <returns>Updated POI or 404 if not found</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<PointOfInterestDto>> UpdatePoi([Required] string id, [FromBody] PointOfInterestDto poiDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(poiDto.Id) && poiDto.Id != id)
            {
                return BadRequest(new { message = "ID in URL and body do not match" });
            }
            // check if DTO has right ID
            poiDto.Id = id;

            // Map DTO to entity
            var poiEntity = PointOfInterestMapper.ToEntity(poiDto);
            // Service does entity update
            var updatedPoiEntity = await _poiService.UpdatePoiAsync(id, poiEntity);

            if (updatedPoiEntity == null)
            {
                _logger.LogWarning("POI not found for update with ID: {Id}", id);
                return NotFound($"POI with ID '{id}' was not found");
            }

            // Mapper entity to DTO
            var updatedPoiDto = PointOfInterestMapper.ToDto(updatedPoiEntity);

            // Generate href
            GenerateHref(updatedPoiDto);

            _logger.LogInformation("POI updated: {Name} (ID: {Id})", updatedPoiDto.Name, id);
            return Ok(updatedPoiDto);
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

    [HttpPatch("{id}")]
    public async Task<ActionResult<PointOfInterestDto>> PatchPoi(string id, [FromBody] PointOfInterestDto dto)
    {
        try
        {
            // Existierenden POI laden
            var existingEntity = await _poiService.GetPoiByIdAsync(id);

            if (existingEntity == null)
            {
                return NotFound(new { message = $"POI with ID {id} not found" });
            }

            // Nur nicht-null Felder aktualisieren
            if (dto.Category != null) existingEntity.Category = dto.Category;
            if (dto.Name != null) existingEntity.Name = dto.Name;
            if (dto.Details != null) existingEntity.Details = dto.Details;
            if (dto.Tags != null) existingEntity.Tags = dto.Tags;
            if (dto.Location != null)
            {
                existingEntity.Location = new LocationEntity
                {
                    Type = dto.Location.Type,
                    Coordinates = dto.Location.Coordinates
                };
            }

            // Service aktualisiert Entity
            var updatedEntity = await _poiService.UpdatePoiAsync(id, existingEntity);

            if (updatedEntity == null)
            {
                _logger.LogWarning("POI update failed for ID: {Id}", id);
                return StatusCode(500, "Failed to update POI");
            }

            // Mapper: Entity → DTO
            var updatedDto = PointOfInterestMapper.ToDto(updatedEntity);

            // Href generieren
            GenerateHref(updatedDto);

            _logger.LogInformation("POI patched successfully: {Id}", id);

            return Ok(updatedDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching POI: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}