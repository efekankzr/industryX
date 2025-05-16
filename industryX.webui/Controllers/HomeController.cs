using System.Security.Claims;
using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IProductTransferService _productTransferService;
        private readonly IRawMaterialService _rawMaterialService;
        private readonly ILaborCostService _laborCostService;

        public HomeController(
            IWarehouseService warehouseService,
            IProductService productService,
            IRawMaterialService rawMaterialService,
            ILaborCostService laborCostService,
            IProductTransferService productTransferService)
        {
            _warehouseService = warehouseService;
            _productService = productService;
            _rawMaterialService = rawMaterialService;
            _laborCostService = laborCostService;
            _productTransferService = productTransferService;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> RedirectDasboards()
        {
            if (User.IsInRole("Admin"))
            {
                var model = await GetSetupStatusAsync();
                if (!model.IsSetupComplete)
                    return RedirectToAction(nameof(SetupDashboard));
                return RedirectToAction(nameof(AdminDashboard));
            }

            if (User.IsInRole("Driver"))
                return RedirectToAction(nameof(DriverDashboard));

            if (User.IsInRole("WarehouseManager"))
                return RedirectToAction(nameof(WarehouseManagerDashboard));

            if (User.IsInRole("ProductionManager"))
                return RedirectToAction(nameof(ProductionManagerDashboard));

            return Forbid();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetupDashboard()
        {
            var model = await GetSetupStatusAsync();
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard() => View();

        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> DriverDashboard()
        {
            var createdTransfers = await _productTransferService.GetAllCreatedTransfersAsync();
            return View(createdTransfers);
        }


        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> WarehouseManagerDashboard()
        {
            var inTransitTransfers = await _productTransferService.GetAllInTransitTransfersAsync();
            return View(inTransitTransfers);
        }

        [Authorize(Roles = "ProductionManager")]
        public IActionResult ProductionManagerDashboard() => View();

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
