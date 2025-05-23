using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WarehouseController : BaseController
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // ---------------------- LIST ----------------------
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            return View(warehouses);
        }

        // --------------------- CREATE ---------------------
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return View(warehouse);

            var (success, error) = await _warehouseService.AddWithInitialStocksAsync(warehouse);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(warehouse);
            }

            ShowAlert("Warehouse Created", "Warehouse and initial stock records created.", "success");
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- EDIT ----------------------
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            if (warehouse == null)
            {
                ShowAlert("Not Found", "Warehouse could not be found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return View(warehouse);

            var (success, error) = await _warehouseService.UpdateAsync(warehouse);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(warehouse);
            }

            ShowAlert("Warehouse Updated", "Warehouse updated successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        // --------------------- DELETE ---------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            if (warehouse == null)
            {
                ShowAlert("Error", "Warehouse not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            await _warehouseService.DeleteAsync(id);
            ShowAlert("Deleted", "Warehouse deleted.", "warning");

            return RedirectToAction(nameof(Index));
        }

        // ----------------- SET MAIN WAREHOUSE -----------------
        [HttpPost]
        public async Task<IActionResult> SetMainWarehouse(int warehouseId, string type)
        {
            var warehouses = await _warehouseService.GetAllAsync();

            foreach (var warehouse in warehouses)
            {
                warehouse.IsMainForProduct = type == "product" && warehouse.Id == warehouseId;
                warehouse.IsMainForRawMaterial = type == "raw" && warehouse.Id == warehouseId;
                warehouse.IsMainForSalesProduct = type == "sales" && warehouse.Id == warehouseId;

                await _warehouseService.UpdateAsync(warehouse);
            }

            return Ok();
        }
    }
}
