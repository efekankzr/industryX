using IndustryX.Application.Interfaces;
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
        private readonly ISalesProductService _salesProductService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;

        public HomeController(
            IWarehouseService warehouseService,
            IProductService productService,
            IRawMaterialService rawMaterialService,
            ILaborCostService laborCostService,
            IProductTransferService productTransferService,
            ISalesProductService salesProductService,
            IUserService userService,
            ICategoryService categoryService)
        {
            _warehouseService = warehouseService;
            _productService = productService;
            _rawMaterialService = rawMaterialService;
            _laborCostService = laborCostService;
            _productTransferService = productTransferService;
            _salesProductService = salesProductService;
            _userService = userService;
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated && !User.IsInRole("Customer"))
            {
                return await RedirectDasboards();
            }

            var salesProducts = await _salesProductService.GetActiveListAsync();

            var model = salesProducts.Select(p => new SalesProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.SalePrice,
                Url = p.Url,
                ImagePath = p.Images.FirstOrDefault()?.ImagePath
            }).ToList();

            return View(model);
        }

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

        [Authorize(Roles = "SalesManager")]
        public IActionResult SalesManagerDashboard() => View();

        private async Task<AdminDashboardViewModel> GetSetupStatusAsync()
        {
            var hasWarehouse = (await _warehouseService.GetAllAsync()).Any();
            var (mainProduct, mainRaw, mainSales) = await _warehouseService.CheckMainWarehousesAsync();
            var hasLaborCost = (await _laborCostService.GetAllAsync()).Any();
            var hasProduct = (await _productService.GetAllAsync()).Any();
            var hasRawMaterial = (await _rawMaterialService.GetAllAsync()).Any();
            var hasSalesProduct = (await _salesProductService.GetAllAsync()).Any();
            var hasCategory = (await _categoryService.GetAllAsync()).Any();

            var users = (await _userService.GetAllAsync()).ToList();
            var roles = new[] { "SalesManager", "WarehouseManager", "ProductionManager", "Driver" };

            var roleCheckResults = new Dictionary<string, bool>();

            foreach (var role in roles)
            {
                var usersInRole = await _userService.GetByRoleAsync(role);
                roleCheckResults[role] = usersInRole.Any();
            }

            var warehouses = await _warehouseService.GetAllAsync();
            bool allWarehousesHaveManager = true;

            foreach (var warehouse in warehouses)
            {
                var warehouseManagers = await _userService.GetByRoleAsync("WarehouseManager");
                var hasManager = warehouseManagers.Any(u => u.WarehouseId == warehouse.Id);

                if (!hasManager)
                {
                    allWarehousesHaveManager = false;
                    break;
                }
            }

            return new AdminDashboardViewModel
            {
                HasWarehouse = hasWarehouse,
                HasMainProductWarehouse = mainProduct,
                HasMainRawMaterialWarehouse = mainRaw,
                HasMainSalesProductWarehouse = mainSales,
                HasLaborCost = hasLaborCost,
                HasProduct = hasProduct,
                HasCategory = hasCategory,
                HasSalesProduct = hasSalesProduct,
                HasRawMaterial = hasRawMaterial,
                HasSalesManager = roleCheckResults["SalesManager"],
                HasWarehouseManager = roleCheckResults["WarehouseManager"],
                HasProductionManager = roleCheckResults["ProductionManager"],
                HasDriver = roleCheckResults["Driver"],
                AllWarehousesHaveManager = allWarehousesHaveManager
            };
        }
    }
}
