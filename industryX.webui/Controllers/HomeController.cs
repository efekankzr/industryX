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
        private readonly IRawMaterialService _rawMaterialService;
        private readonly ILaborCostService _laborCostService;
        private readonly IProductTransferService _productTransferService;
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

        // -------------------------------
        // Homepage for Visitors & Users
        // -------------------------------
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Customer"))
                return await RedirectDasboards();

            var bestSellers = await _salesProductService.GetBestSellersAsync();
            var populars = await _salesProductService.GetPopularAsync();

            var bestSellerModels = bestSellers.Select(p => new ProductCardViewModel
            {
                Product = new SalesProductListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.SalePrice,
                    Url = p.Url,
                    ImagePath = p.Images.FirstOrDefault()?.ImagePath
                },
                IsWishlist = false
            }).ToList();

            var popularModels = populars.Select(p => new ProductCardViewModel
            {
                Product = new SalesProductListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.SalePrice,
                    Url = p.Url,
                    ImagePath = p.Images.FirstOrDefault()?.ImagePath
                },
                IsWishlist = false
            }).ToList();

            var model = new HomePageViewModel
            {
                BestSellers = bestSellerModels,
                PopularProducts = popularModels
            };

            return View(model);
        }

        // -------------------------------
        // Redirect to Role-Based Dashboards
        // -------------------------------
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

        // -------------------------------
        // Admin Dashboards
        // -------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetupDashboard()
        {
            var model = await GetSetupStatusAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard() => View();

        // -------------------------------
        // Role-Specific Dashboards
        // -------------------------------
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> DriverDashboard()
        {
            var transfers = await _productTransferService.GetAllCreatedTransfersAsync();
            return View(transfers);
        }

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> WarehouseManagerDashboard()
        {
            var transfers = await _productTransferService.GetAllInTransitTransfersAsync();
            return View(transfers);
        }

        [Authorize(Roles = "ProductionManager")]
        public IActionResult ProductionManagerDashboard() => View();

        [Authorize(Roles = "SalesManager")]
        public IActionResult SalesManagerDashboard() => View();

        // -------------------------------
        // Setup Check Logic
        // -------------------------------
        private async Task<AdminDashboardViewModel> GetSetupStatusAsync()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            var (hasMainProduct, hasMainRaw, hasMainSales) = await _warehouseService.CheckMainWarehousesAsync();

            var model = new AdminDashboardViewModel
            {
                HasWarehouse = warehouses.Any(),
                HasMainProductWarehouse = hasMainProduct,
                HasMainRawMaterialWarehouse = hasMainRaw,
                HasMainSalesProductWarehouse = hasMainSales,
                HasLaborCost = (await _laborCostService.GetAllAsync()).Any(),
                HasProduct = (await _productService.GetAllAsync()).Any(),
                HasRawMaterial = (await _rawMaterialService.GetAllAsync()).Any(),
                HasSalesProduct = (await _salesProductService.GetAllAsync()).Any(),
                HasCategory = (await _categoryService.GetAllAsync()).Any(),
                AllWarehousesHaveManager = await AllWarehousesAssignedAsync()
            };

            var roles = new[] { "SalesManager", "WarehouseManager", "ProductionManager", "Driver" };

            foreach (var role in roles)
            {
                var usersInRole = await _userService.GetByRoleAsync(role);
                model.SetRolePresence(role, usersInRole.Any());
            }

            return model;
        }

        private async Task<bool> AllWarehousesAssignedAsync()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            var managers = await _userService.GetByRoleAsync("WarehouseManager");

            return warehouses.All(w => managers.Any(m => m.WarehouseId == w.Id));
        }
    }
}
