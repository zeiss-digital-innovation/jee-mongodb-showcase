using DotNetMapsFrontend.Models;
using DotNetMapsFrontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetMapsFrontend.Controllers
{
    public class PointOfInterestController : Controller
    {
        private readonly IPointOfInterestService _poiService;

        public PointOfInterestController(IPointOfInterestService poiService)
        {
            _poiService = poiService;
        }

        [Route("poi")]
        [Route("PointOfInterest")]
        public async Task<IActionResult> Index(double? lat, double? lon, int? radius)
        {
            List<PointOfInterest> points;
            
            if (lat.HasValue && lon.HasValue)
            {
                // Use provided coordinates and radius
                var searchRadius = radius ?? 2000;
                points = await _poiService.GetPointsOfInterestAsync(lat.Value, lon.Value, searchRadius);
            }
            else
            {
                // Use default coordinates (Dresden)
                points = await _poiService.GetPointsOfInterestAsync();
            }
            
            return View(points);
        }

        [HttpGet]
        [Route("api/pointsofinterest")]
        public async Task<IActionResult> GetAll(double? lat, double? lon, int? radius, [FromQuery] List<string>? category = null)
        {
            try
            {
                List<PointOfInterest> points;
                
                if (lat.HasValue && lon.HasValue)
                {
                    // Use provided coordinates and radius (default 2000m if not specified)
                    var searchRadius = radius ?? 2000;
                    var categories = category ?? new List<string>();
                    
                    points = await _poiService.GetPointsOfInterestAsync(lat.Value, lon.Value, searchRadius, categories);
                }
                else
                {
                    // Use default coordinates (Dresden)
                    points = await _poiService.GetPointsOfInterestAsync();
                }
                
                return Json(points);
            }
            catch (Exception)
            {
                return Json(new List<PointOfInterest>());
            }
        }

        [HttpPost]
        [Route("api/pointsofinterest")]
        public async Task<IActionResult> Create([FromBody] PointOfInterest pointOfInterest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Input validation for XSS and injection attacks
                if (string.IsNullOrWhiteSpace(pointOfInterest.Category) || 
                    string.IsNullOrWhiteSpace(pointOfInterest.Details))
                {
                    return BadRequest("Category and Details are required.");
                }

                // Sanitize details to prevent XSS
                pointOfInterest.Details = System.Web.HttpUtility.HtmlEncode(pointOfInterest.Details);

                // Validate coordinates
                if (pointOfInterest.Location?.Coordinates == null || 
                    pointOfInterest.Location.Coordinates.Length != 2)
                {
                    return BadRequest("Valid coordinates are required.");
                }

                var latitude = pointOfInterest.Location.Coordinates[1];
                var longitude = pointOfInterest.Location.Coordinates[0];

                if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                {
                    return BadRequest("Coordinates are out of valid range.");
                }

                var createdPoi = await _poiService.CreatePointOfInterestAsync(pointOfInterest);
                return Json(createdPoi);
            }
            catch (Exception)
            {
                // Log the error but don't expose internal details
                return StatusCode(500, "Failed to create Point of Interest.");
            }
        }

        [HttpGet]
        [Route("poi/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var poi = await _poiService.GetPointOfInterestByIdAsync(id);
                if (poi == null)
                {
                    return NotFound();
                }
                return Json(poi);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to retrieve Point of Interest.");
            }
        }

        [HttpPut]
        [Route("poi/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] PointOfInterest pointOfInterest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Input validation
                if (string.IsNullOrWhiteSpace(pointOfInterest.Category))
                {
                    return BadRequest("Category is required.");
                }

                if (string.IsNullOrWhiteSpace(pointOfInterest.Details))
                {
                    return BadRequest("Details are required.");
                }

                // Validate location
                if (pointOfInterest.Location?.Coordinates == null || 
                    pointOfInterest.Location.Coordinates.Length != 2)
                {
                    return BadRequest("Valid location is required.");
                }

                var updatedPoi = await _poiService.UpdatePointOfInterestAsync(id, pointOfInterest);
                if (updatedPoi == null)
                {
                    return NotFound($"POI with ID '{id}' was not found");
                }

                return Json(updatedPoi);
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, $"Failed to update Point of Interest: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("poi/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _poiService.DeletePointOfInterestAsync(id);
                return NoContent(); // 204 No Content (idempotent per RFC 9110)
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to delete Point of Interest.");
            }
        }

        [HttpGet]
        [Route("api/categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _poiService.GetAvailableCategoriesAsync();
                return Json(categories);
            }
            catch (Exception)
            {
                // Return fallback categories if service fails
                var fallbackCategories = new List<string>
                {
                    "landmark", "museum", "castle", "cathedral", "park",
                    "restaurant", "hotel", "gasstation", "hospital", "pharmacy",
                    "shop", "bank", "school", "library", "theater"
                };
                return Json(fallbackCategories);
            }
        }
    }
}