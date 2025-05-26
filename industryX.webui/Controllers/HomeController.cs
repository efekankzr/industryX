using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
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
        private readonly IProductionService _productionService;
        private readonly IOrderService _orderService;

        public HomeController(
            IWarehouseService warehouseService,
            IProductService productService,
            IRawMaterialService rawMaterialService,
            ILaborCostService laborCostService,
            IProductTransferService productTransferService,
            ISalesProductService salesProductService,
            IUserService userService,
            ICategoryService categoryService,
            IProductionService productionService,
            IOrderService orderService)
        {
            _warehouseService = warehouseService;
            _productService = productService;
            _rawMaterialService = rawMaterialService;
            _laborCostService = laborCostService;
            _productTransferService = productTransferService;
            _salesProductService = salesProductService;
            _userService = userService;
            _categoryService = categoryService;
            _productionService = productionService;
            _orderService = orderService;
        }

        // -------------------------------
        // Homepage for Visitors & Customers
        // -------------------------------
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Customer"))
                return await RedirectDasboards();

            var bestSellers = await _salesProductService.GetBestSellersAsync();
            var populars = await _salesProductService.GetPopularAsync();

            var model = new HomePageViewModel
            {
                BestSellers = bestSellers.Select(p => new ProductCardViewModel
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
                }).ToList(),

                PopularProducts = populars.Select(p => new ProductCardViewModel
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
                }).ToList()
            };

            return View(model);
        }

        // -------------------------------
        // Role-Based Dashboard Redirection
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

            if (User.IsInRole("Driver")) return RedirectToAction(nameof(DriverDashboard));
            if (User.IsInRole("WarehouseManager")) return RedirectToAction(nameof(WarehouseManagerDashboard));
            if (User.IsInRole("ProductionManager")) return RedirectToAction(nameof(ProductionManagerDashboard));
            if (User.IsInRole("SalesManager")) return RedirectToAction(nameof(SalesManagerDashboard));

            return Forbid();
        }

        // -------------------------------
        // Admin Dashboard
        // -------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetupDashboard()
        {
            var model = await GetSetupStatusAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var model = await GetSetupStatusAsync();

            model.TotalProducts = (await _productService.GetAllAsync()).Count();
            model.TotalSalesProducts = (await _salesProductService.GetAllAsync()).Count();
            var users = await _userService.GetAllAsync();

            model.TotalCustomers = await _userService.GetUserCountByRoleAsync("Customer");
            model.TotalStaff = await _userService.GetStaffCountAsync();

            model.PendingOrders = await _orderService.GetOrderCountByStatusAsync(OrderStatus.Pending);
            model.DeliveredOrders = await _orderService.GetOrderCountByStatusAsync(OrderStatus.Delivered);
            model.TotalRevenue = await _orderService.GetTotalRevenueAsync();

            var recentOrders = await _orderService.GetRecentOrdersAsync(5);
            model.RecentOrders = recentOrders.Select(o => new RecentOrderViewModel
            {
                Id = o.Id,
                CustomerName = o.User.Firstname,
                TotalPrice = o.TotalPrice,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            }).ToList();

            model.SalesChartData = await _orderService.GetWeeklySalesChartDataAsync();

            model.CriticalProductStocks = (await _productService.GetCriticalStocksAsync()).Select(ps => new StockAlertItem
            {
                Name = ps.Product.Name,
                Stock = ps.Stock,
                CriticalStock = ps.QruicalStock,
                Type = "Product"
            }).ToList();

            model.CriticalRawMaterialStocks = (await _rawMaterialService.GetCriticalStocksAsync()).Select(rs => new StockAlertItem
            {
                Name = rs.RawMaterial.Name,
                Stock = (int)rs.Stock,
                CriticalStock = (int)rs.QruicalStock,
                Type = "RawMaterial"
            }).ToList();

            model.CriticalSalesProductStocks = (await _salesProductService.GetCriticalStocksAsync()).Select(sp => new StockAlertItem
            {
                Name = sp.SalesProduct.Name,
                Stock = (int)sp.Stock,
                CriticalStock = (int)sp.CriticalStock,
                Type = "SalesProduct"
            }).ToList();

            var productions = await _productionService.GetAllAsync();

            model.PlannedProductions = productions
                .Where(p => p.Status == ProductionStatus.Planned)
                .Select(p => new ProductionPlanItem
                {
                    ProductName = p.Product.Name,
                    BoxQuantity = p.BoxQuantity,
                    StartTime = p.StartTime ?? p.CreatedAt
                })
                .ToList();

            model.ActiveProductions = productions
                .Where(p => p.Status == ProductionStatus.InProgress || p.Status == ProductionStatus.Paused)
                .Select(p => new ProductionPlanItem
                {
                    ProductName = p.Product.Name,
                    BoxQuantity = p.BoxQuantity,
                    StartTime = p.StartTime ?? p.CreatedAt
                })
                .ToList();


            return View(model);
        }

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
        // Setup Status Check
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
