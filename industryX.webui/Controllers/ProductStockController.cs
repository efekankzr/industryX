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

        public ProductStockController(IProductStockService productStockService,
            IUserService userService)
        {
            _productStockService = productStockService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var allStocks = await _productStockService.GetAllWithIncludesAsync();

            if (User.IsInRole("Admin"))
            {
                return View(allStocks);
            }

            if (User.IsInRole("WarehouseManager"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = await _userService.GetByIdAsync(userId);
                Console.WriteLine($"UserId: {userId}, WarehouseId: {user?.WarehouseId}");
                if (user == null || user.WarehouseId == null)
                {
                    ShowAlert("Error", "No warehouse is assigned to your user profile.", "danger");
                    return View(Enumerable.Empty<ProductStock>());
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
        public async Task<IActionResult> SetCriticalStock(int id, int critical)
        {
            if (critical < 0)
            {
                ShowAlert("Invalid Input", "Critical stock must be zero or more.", "warning");
                return RedirectToAction(nameof(Index));
            }

            var success = await _productStockService.SetCriticalStockAsync(id, critical);

            if (!success)
                ShowAlert("Error", "Failed to set critical stock.", "danger");
            else
                ShowAlert("Success", "Critical stock updated.", "success");

            return RedirectToAction(nameof(Index));
        }
    }
}
