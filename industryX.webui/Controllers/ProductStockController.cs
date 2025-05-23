using System.Security.Claims;
using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin,WarehouseManager")]
    public class ProductStockController : BaseController
    {
        private readonly IProductStockService _productStockService;
        private readonly IUserService _userService;

        public ProductStockController(
            IProductStockService productStockService,
            IUserService userService)
        {
            _productStockService = productStockService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var stocks = await _productStockService.GetAllWithIncludesAsync();

            if (User.IsInRole("Admin"))
                return View(stocks);

            if (User.IsInRole("WarehouseManager"))
            {
                var userId = GetUserId();
                var user = await _userService.GetByIdAsync(userId);

                if (user?.WarehouseId == null)
                {
                    ShowAlert("Error", "No warehouse is assigned to your profile.", "danger");
                    return View(Enumerable.Empty<ProductStock>());
                }

                var filteredStocks = stocks
                    .Where(s => s.WarehouseId == user.WarehouseId.Value)
                    .ToList();

                return View(filteredStocks);
            }

            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetCriticalStock(int id, int critical)
        {
            if (critical < 0)
            {
                ShowAlert("Invalid Input", "Critical stock must be zero or more.", "warning");
                return RedirectToAction(nameof(Index));
            }

            var success = await _productStockService.SetCriticalStockAsync(id, critical);

            ShowAlert(
                success ? "Success" : "Error",
                success ? "Critical stock updated." : "Failed to update critical stock.",
                success ? "success" : "danger"
            );

            return RedirectToAction(nameof(Index));
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}
