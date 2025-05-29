using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels.ApiViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleLocationApiController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleLocationApiController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] VehicleLocationUpdateDto dto)
        {
            // DeviceId'den aracı bul
            var vehicle = await _vehicleService.GetByDeviceIdAsync(dto.DeviceId);
            if (vehicle == null)
                return NotFound("Vehicle not found");

            // Konum güncelle
            vehicle.LocationLog.Latitude = dto.Latitude;
            vehicle.LocationLog.Longitude = dto.Longitude;
            vehicle.LocationLog.Status = (VehicleStatus)dto.Status;
            vehicle.LocationLog.UpdatedAt = DateTime.UtcNow;

            await _vehicleService.UpdateAsync(vehicle);

            return Ok(new { message = "Location updated successfully." });
        }
    }
}
