using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin,ProductionManager")]
    public class ProductionController : BaseController
    {
        private readonly IProductionService _productionService;
        private readonly IProductService _productService;

        public ProductionController(IProductionService productionService, IProductService productService)
        {
            _productionService = productionService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");

            var productions = await _productionService.GetAllAsync();

            if (!isAdmin)
            {
                productions = productions
                    .Where(p => p.Status != ProductionStatus.Completed)
                    .ToList();
            }

            return View(productions);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var production = await _productionService.GetByIdAsync(id);
            if (production == null)
            {
                ShowAlert("Error", "Production not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return View(production);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ProductionFormViewModel
            {
                Products = (await _productService.GetAllAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionFormViewModel model)
        {
            model.Products = (await _productService.GetAllAsync()).ToList();

            if (!ModelState.IsValid)
                return View(model);

            var production = new Production
            {
                ProductId = model.ProductId,
                BoxQuantity = model.BoxQuantity,
                WorkerCount = model.WorkerCount,
                Notes = model.Notes ?? string.Empty
            };

            var (success, error) = await _productionService.CreateAsync(production);
            if (!success)
            {
                ShowAlert("Error", error ?? "Production could not be created.", "danger");
                return View(model);
            }

            ShowAlert("Success", "Production created successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Start(int id)
        {
            var success = await _productionService.StartProductionAsync(id);
            ShowAlert(
                success ? "Started" : "Error",
                success ? "Production started successfully." : "Failed to start production.",
                success ? "success" : "danger"
            );
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Pause(int id)
        {
            var success = await _productionService.PauseProductionAsync(id);
            ShowAlert(
                success ? "Paused" : "Error",
                success ? "Production paused successfully." : "Failed to pause production.",
                success ? "warning" : "danger"
            );
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Resume(int id)
        {
            var success = await _productionService.ResumeProductionAsync(id);
            ShowAlert(
                success ? "Resumed" : "Error",
                success ? "Production resumed successfully." : "Failed to resume production.",
                success ? "info" : "danger"
            );
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int id)
        {
            var success = await _productionService.FinishProductionAsync(id);
            ShowAlert(
                success ? "Completed" : "Error",
                success ? "Production completed successfully." : "Failed to finish production.",
                success ? "success" : "danger"
            );
            return RedirectToAction(nameof(Index));
        }        
    }
}
