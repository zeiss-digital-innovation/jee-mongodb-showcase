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
    }
}