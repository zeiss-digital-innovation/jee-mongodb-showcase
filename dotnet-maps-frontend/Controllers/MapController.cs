using DotNetMapsFrontend.Models;
using DotNetMapsFrontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetMapsFrontend.Controllers
{
    public class MapController : Controller
    {
        private readonly IPointOfInterestService _poiService;

        public MapController(IPointOfInterestService poiService)
        {
            _poiService = poiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPointsOfInterest()
        {
            try
            {
                var points = await _poiService.GetPointsOfInterestAsync();
                return Json(points);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}