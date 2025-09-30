using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DotNetMongoDbBackend.Controllers;

/// <summary>
/// REST Controller für Points of Interest
/// Kompatible API mit JEE und Spring Boot Backend
/// </summary>
[ApiController]
[Route("geoservice")]
public class PointOfInterestController : ControllerBase
{
    private readonly IPointOfInterestService _poiService;
    private readonly ILogger<PointOfInterestController> _logger;

    public PointOfInterestController(IPointOfInterestService poiService, ILogger<PointOfInterestController> logger)
    {
        _poiService = poiService;
        _logger = logger;
    }

    /// <summary>
    /// GET /geoservice/poi - Alle POIs abrufen
    /// </summary>
    /// <param name="category">Filtert nach Kategorie</param>
    /// <param name="search">Volltext-Suche in Name, Adresse und Tags</param>
    /// <param name="limit">Maximal zurückzugebende Ergebnisse</param>
    /// <param name="lat">Latitude für geografische Suche</param>
    /// <param name="lng">Longitude für geografische Suche</param>
    /// <param name="radius">Radius in Kilometern für geografische Suche</param>
    /// <returns>Liste der POIs</returns>
    [HttpGet("poi")]
    public async Task<ActionResult<List<PointOfInterest>>> GetAllPois(
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] int? limit = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double? radius = null)
    {
        try
        {
            List<PointOfInterest> pois;

            // Geografische Suche
            if (lat.HasValue && lng.HasValue)
            {
                pois = await _poiService.GetNearbyPoisAsync(lng.Value, lat.Value, radius ?? 10.0);
            }
            // Kategorie-Filter
            else if (!string.IsNullOrWhiteSpace(category))
            {
                pois = await _poiService.GetPoisByCategoryAsync(category);
            }
            // Volltext-Suche
            else if (!string.IsNullOrWhiteSpace(search))
            {
                pois = await _poiService.SearchPoisAsync(search, limit);
            }
            // Alle POIs
            else
            {
                pois = await _poiService.GetAllPoisAsync();
            }

            _logger.LogInformation("POIs abgerufen: {Count} Ergebnisse", pois.Count);
            return Ok(pois);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der POIs");
            return StatusCode(500, "Interner Serverfehler beim Abrufen der POIs");
        }
    }

    /// <summary>
    /// GET /geoservice/poi/{id} - POI nach ID abrufen
    /// </summary>
    /// <param name="id">MongoDB ObjectId des POI</param>
    /// <returns>POI oder 404 wenn nicht gefunden</returns>
    [HttpGet("poi/{id}")]
    public async Task<ActionResult<PointOfInterest>> GetPoiById([Required] string id)
    {
        try
        {
            var poi = await _poiService.GetPoiByIdAsync(id);
            
            if (poi == null)
            {
                _logger.LogWarning("POI nicht gefunden mit ID: {Id}", id);
                return NotFound($"POI mit ID '{id}' wurde nicht gefunden");
            }

            _logger.LogInformation("POI abgerufen: {Name} (ID: {Id})", poi.Name, poi.Id);
            return Ok(poi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des POI mit ID: {Id}", id);
            return StatusCode(500, "Interner Serverfehler beim Abrufen des POI");
        }
    }

    /// <summary>
    /// POST /geoservice/poi - Neuen POI erstellen
    /// </summary>
    /// <param name="poi">POI-Daten</param>
    /// <returns>Erstellter POI mit generierter ID</returns>
    [HttpPost("poi")]
    public async Task<ActionResult<PointOfInterest>> CreatePoi([FromBody] PointOfInterest poi)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdPoi = await _poiService.CreatePoiAsync(poi);
            
            _logger.LogInformation("POI erstellt: {Name} (ID: {Id})", createdPoi.Name, createdPoi.Id);
            return CreatedAtAction(nameof(GetPoiById), new { id = createdPoi.Id }, createdPoi);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige POI-Daten beim Erstellen");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des POI: {Name}", poi?.Name);
            return StatusCode(500, "Interner Serverfehler beim Erstellen des POI");
        }
    }

    /// <summary>
    /// PUT /geoservice/poi/{id} - POI aktualisieren
    /// </summary>
    /// <param name="id">MongoDB ObjectId des POI</param>
    /// <param name="poi">Aktualisierte POI-Daten</param>
    /// <returns>Aktualisierter POI oder 404 wenn nicht gefunden</returns>
    [HttpPut("poi/{id}")]
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
                _logger.LogWarning("POI nicht gefunden für Update mit ID: {Id}", id);
                return NotFound($"POI mit ID '{id}' wurde nicht gefunden");
            }

            _logger.LogInformation("POI aktualisiert: {Name} (ID: {Id})", updatedPoi.Name, updatedPoi.Id);
            return Ok(updatedPoi);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige POI-Daten beim Aktualisieren");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des POI mit ID: {Id}", id);
            return StatusCode(500, "Interner Serverfehler beim Aktualisieren des POI");
        }
    }

    /// <summary>
    /// DELETE /geoservice/poi/{id} - POI löschen
    /// </summary>
    /// <param name="id">MongoDB ObjectId des POI</param>
    /// <returns>204 bei Erfolg oder 404 wenn nicht gefunden</returns>
    [HttpDelete("poi/{id}")]
    public async Task<ActionResult> DeletePoi([Required] string id)
    {
        try
        {
            var deleted = await _poiService.DeletePoiAsync(id);
            
            if (!deleted)
            {
                _logger.LogWarning("POI nicht gefunden zum Löschen mit ID: {Id}", id);
                return NotFound($"POI mit ID '{id}' wurde nicht gefunden");
            }

            _logger.LogInformation("POI gelöscht mit ID: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des POI mit ID: {Id}", id);
            return StatusCode(500, "Interner Serverfehler beim Löschen des POI");
        }
    }

    /// <summary>
    /// GET /api/categories - Alle verfügbaren Kategorien abrufen
    /// </summary>
    /// <returns>Liste der verfügbaren Kategorien</returns>
    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetAvailableCategories()
    {
        try
        {
            var categories = await _poiService.GetAvailableCategoriesAsync();
            
            _logger.LogInformation("Kategorien abgerufen: {Count} verfügbare Kategorien", categories.Count);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der verfügbaren Kategorien");
            return StatusCode(500, "Interner Serverfehler beim Abrufen der Kategorien");
        }
    }

    /// <summary>
    /// GET /api/stats/category/{category} - Statistiken für Kategorie
    /// </summary>
    /// <param name="category">Kategorie-Name</param>
    /// <returns>Anzahl POIs in der Kategorie</returns>
    [HttpGet("stats/category/{category}")]
    public async Task<ActionResult<long>> GetCategoryCount([Required] string category)
    {
        try
        {
            var count = await _poiService.CountByCategoryAsync(category);
            
            _logger.LogInformation("Kategorie-Statistik abgerufen: {Category} hat {Count} POIs", category, count);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Statistiken für Kategorie: {Category}", category);
            return StatusCode(500, "Interner Serverfehler beim Abrufen der Kategorie-Statistiken");
        }
    }

    /// <summary>
    /// GET /api/health - Health Check Endpoint
    /// </summary>
    /// <returns>Service-Status</returns>
    [HttpGet("health")]
    public ActionResult<string> HealthCheck()
    {
        return Ok(".NET MongoDB Backend is running");
    }
}