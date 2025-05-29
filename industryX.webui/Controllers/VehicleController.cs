using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehicleController : BaseController
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.GetAllAsync();
            var drivers = await _vehicleService.GetAvailableDriversAsync();
            ViewBag.Drivers = drivers;
            return View(vehicles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                await _vehicleService.AddAsync(vehicle);
                ShowAlert("Success", "Vehicle created successfully.", "success");
                return RedirectToAction(nameof(Index));
            }

            ShowAlert("Error", "Please check the form for errors.", "danger");
            return View(vehicle);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);
            if (vehicle == null)
            {
                ShowAlert("Error", "Vehicle not found.", "danger");
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                await _vehicleService.UpdateAsync(vehicle);
                ShowAlert("Success", "Vehicle updated successfully.", "success");
                return RedirectToAction(nameof(Index));
            }

            ShowAlert("Error", "Please check the form for errors.", "danger");
            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _vehicleService.DeleteAsync(id);
            ShowAlert("Success", "Vehicle deleted successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int vehicleId, string driverUserId)
        {
            var result = await _vehicleService.AssignDriverAsync(vehicleId, driverUserId);
            if (result)
                ShowAlert("Success", "Driver assigned successfully.", "success");
            else
                ShowAlert("Error", "Driver assignment failed.", "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDriver(int vehicleId)
        {
            await _vehicleService.RemoveDriverAsync(vehicleId);
            ShowAlert("Success", "Driver assignment removed.", "success");
            return RedirectToAction(nameof(Index));
        }
    }
}
