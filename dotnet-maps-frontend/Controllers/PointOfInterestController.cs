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
        public async Task<IActionResult> Index()
        {
            var points = await _poiService.GetPointsOfInterestAsync();
            return View(points);
        }

        [HttpGet]
        [Route("api/pointsofinterest")]
        public async Task<IActionResult> GetAll(double? lat, double? lon, int? radius)
        {
            try
            {
                List<PointOfInterest> points;
                
                if (lat.HasValue && lon.HasValue)
                {
                    // Use provided coordinates and radius (default 2000m if not specified)
                    var searchRadius = radius ?? 2000;
                    points = await _poiService.GetPointsOfInterestAsync(lat.Value, lon.Value, searchRadius);
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