using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.WebUI.Controllers;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IRawMaterialService _rawMaterialService;
        private readonly ILaborCostService _laborCostService;

        public HomeController(
            IWarehouseService warehouseService,
            IProductService productService,
            IRawMaterialService rawMaterialService,
            ILaborCostService laborCostService)
        {
            _warehouseService = warehouseService;
            _productService = productService;
            _rawMaterialService = rawMaterialService;
            _laborCostService = laborCostService;
        }

        public IActionResult Index() => View();

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var model = await GetSetupStatusAsync();

            if (!model.IsSetupComplete)
                return RedirectToAction(nameof(SetupDashboard));

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetupDashboard()
        {
            var model = await GetSetupStatusAsync();
            return View(model);
        }

        [Authorize(Roles = "Driver,WarehouseManager,ProductionManager")]
        public IActionResult PersonelDashboard() => View();

        private async Task<AdminDashboardViewModel> GetSetupStatusAsync()
        {
            var hasWarehouse = (await _warehouseService.GetAllAsync()).Any();
            var (mainProduct, mainRaw, mainSales) = await _warehouseService.CheckMainWarehousesAsync();
            var hasLaborCost = (await _laborCostService.GetAllAsync()).Any();
            var hasProduct = (await _productService.GetAllAsync()).Any();
            var hasRawMaterial = (await _rawMaterialService.GetAllAsync()).Any();

            return new AdminDashboardViewModel
            {
                HasWarehouse = hasWarehouse,
                HasMainProductWarehouse = mainProduct,
                HasMainRawMaterialWarehouse = mainRaw,
                HasMainSalesProductWarehouse = mainSales,
                HasLaborCost = hasLaborCost,
                HasProduct = hasProduct,
                HasRawMaterial = hasRawMaterial
            };
        }
    }
}
