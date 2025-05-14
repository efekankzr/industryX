using IndustryX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class RawMaterialStockController : BaseController
    {
        private readonly IRawMaterialStockService _stockService;

        public RawMaterialStockController(IRawMaterialStockService stockService)
        {
            _stockService = stockService;
        }

        public async Task<IActionResult> Index()
        {
            var stocks = await _stockService.GetAllWithIncludesAsync();
            return View(stocks);
        }

        [HttpPost]
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
