using IndustryX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class ProductStockController : BaseController
    {
        private readonly IProductStockService _productStockService;

        public ProductStockController(IProductStockService productStockService)
        {
            _productStockService = productStockService;
        }

        public async Task<IActionResult> Index()
        {
            var stocks = await _productStockService.GetAllWithIncludesAsync();
            return View(stocks);
        }

        [HttpPost]
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
