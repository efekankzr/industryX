using IndustryX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehicleLocationController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehicleLocationController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.GetAllAsync();
            return View(vehicles);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            var vehicles = await _vehicleService.GetAllWithLocationAsync();
            var data = vehicles.Select(v => new
            {
                PlateNumber = v.PlateNumber,
                Latitude = v.LocationLog?.Latitude,
                Longitude = v.LocationLog?.Longitude
            }).ToList();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleLocation(int id)
        {
            var vehicle = await _vehicleService.GetByIdWithLocationAsync(id);
            if (vehicle == null || vehicle.LocationLog == null)
                return NotFound();

            return Json(new
            {
                PlateNumber = vehicle.PlateNumber,
                Latitude = vehicle.LocationLog.Latitude,
                Longitude = vehicle.LocationLog.Longitude
            });
        }
    }
}
