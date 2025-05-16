using System.Security.Claims;
using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin, ProductionManager")]
    public class RawMaterialStockController : BaseController
    {
        private readonly IRawMaterialStockService _stockService;
        private readonly IUserService _userService;

        public RawMaterialStockController(IRawMaterialStockService stockService, IUserService userService)
        {
            _stockService = stockService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var allStocks = await _stockService.GetAllWithIncludesAsync();

            if (User.IsInRole("Admin"))
            {
                return View(allStocks);
            }

            if (User.IsInRole("ProductionManager"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = await _userService.GetByIdAsync(userId);
                Console.WriteLine($"UserId: {userId}, WarehouseId: {user?.WarehouseId}");
                if (user == null || user.WarehouseId == null)
                {
                    ShowAlert("Error", "No warehouse is assigned to your user profile.", "danger");
                    return View(Enumerable.Empty<RawMaterialStock>());
                }

                var userWarehouseId = user.WarehouseId.Value;
                var filtered = allStocks
                    .Where(s => s.WarehouseId == userWarehouseId)
                    .ToList();

                return View(filtered);
            }

            return Forbid();
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdjustStock(int id, decimal amount, string operation)
        {
            if (amount <= 0)
            {
                ShowAlert("Invalid Input", "Amount must be greater than zero.", "warning");
                return RedirectToAction(nameof(Index));
            }

            var success = operation.ToLower() switch
            {
                "increase" => await _stockService.IncreaseStockAsync(id, amount),
                "decrease" => await _stockService.DecreaseStockAsync(id, amount),
                _ => false
            };

            if (!success)
                ShowAlert("Error", "Failed to adjust stock.", "danger");
            else
                ShowAlert("Success", "Stock updated.", "success");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetCriticalStock(int id, decimal critical)
        {
            if (critical < 0)
            {
                ShowAlert("Invalid Input", "Critical stock must be zero or more.", "warning");
                return RedirectToAction(nameof(Index));
            }

            var success = await _stockService.SetCriticalStockAsync(id, critical);

            if (!success)
                ShowAlert("Error", "Failed to set critical stock.", "danger");
            else
                ShowAlert("Success", "Critical stock updated.", "success");

            return RedirectToAction(nameof(Index));
        }
    }
}
