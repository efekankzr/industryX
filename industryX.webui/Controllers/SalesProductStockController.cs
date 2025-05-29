using System.Security.Claims;
using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin, SalesManager")]
    public class SalesProductStockController : BaseController
    {
        private readonly ISalesProductStockService _stockService;
        private readonly IUserService _userService;

        public SalesProductStockController(ISalesProductStockService stockService, IUserService userService)
        {
            _stockService = stockService;
            _userService = userService;
        }

        // -------------------- List --------------------
        public async Task<IActionResult> Index()
        {
            var allStocks = await _stockService.GetAllWithIncludesAsync();

            if (User.IsInRole("Admin"))
                return View(allStocks);

            if (User.IsInRole("SalesManager"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userService.GetByIdAsync(userId);

                if (user?.WarehouseId == null)
                {
                    ShowAlert("Error", "No warehouse is assigned to your user profile.", "danger");
                    return View(Enumerable.Empty<SalesProductStock>());
                }

                var filtered = allStocks.Where(s => s.WarehouseId == user.WarehouseId).ToList();
                return View(filtered);
            }

            return Forbid();
        }

        // -------------------- Stock Adjustment --------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdjustStock(int id, int amount, string operation)
        {
            if (amount <= 0)
            {
                ShowAlert("Invalid Input", "Amount must be greater than zero.", "warning");
                return RedirectToAction(nameof(Index));
            }

            bool success = operation.ToLower() switch
            {
                "increase" => await _stockService.IncreaseStockAsync(id, amount),
                "decrease" => await _stockService.DecreaseStockAsync(id, amount),
                _ => false
            };

            ShowAlert(
                success ? "Success" : "Error",
                success ? "Stock updated successfully." : "Failed to adjust stock.",
                success ? "success" : "danger"
            );

            return RedirectToAction(nameof(Index));
        }

        // -------------------- Set Critical Level --------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetCriticalStock(int id, int critical)
        {
            if (critical < 0)
            {
                ShowAlert("Invalid Input", "Critical stock must be zero or more.", "warning");
                return RedirectToAction(nameof(Index));
            }

            var success = await _stockService.SetCriticalStockAsync(id, critical);

            ShowAlert(
                success ? "Success" : "Error",
                success ? "Critical stock updated." : "Failed to set critical stock.",
                success ? "success" : "danger"
            );

            return RedirectToAction(nameof(Index));
        }
    }
}
