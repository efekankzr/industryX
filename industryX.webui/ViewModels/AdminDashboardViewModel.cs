namespace IndustryX.WebUI.ViewModels
{
    public class AdminDashboardViewModel
    {
        public bool HasWarehouse { get; set; }
        public bool HasMainProductWarehouse { get; set; }
        public bool HasMainRawMaterialWarehouse { get; set; }
        public bool HasMainSalesProductWarehouse { get; set; }
        public bool HasLaborCost { get; set; }
        public bool HasProduct { get; set; }
        public bool HasRawMaterial { get; set; }
        public bool HasSalesProduct { get; set; }
        public bool HasCategory { get; set; }
        public bool AllWarehousesHaveManager { get; set; }

        public bool IsSetupComplete =>
            HasWarehouse &&
            HasMainProductWarehouse &&
            HasMainRawMaterialWarehouse &&
            HasMainSalesProductWarehouse &&
            HasLaborCost &&
            HasProduct &&
            HasRawMaterial &&
            HasSalesProduct &&
            HasCategory &&
            AllWarehousesHaveManager &&
            RoleStatuses.TryGetValue("SalesManager", out var hasSales) && hasSales &&
            RoleStatuses.TryGetValue("WarehouseManager", out var hasWarehouseManager) && hasWarehouseManager &&
            RoleStatuses.TryGetValue("ProductionManager", out var hasProduction) && hasProduction &&
            RoleStatuses.TryGetValue("Driver", out var hasDriver) && hasDriver;


        public Dictionary<string, bool> RoleStatuses { get; set; } = new();

        public void SetRolePresence(string role, bool present)
        {
            RoleStatuses[role] = present;
        }

        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<StockAlertItem> CriticalProductStocks { get; set; } = new();
        public List<StockAlertItem> CriticalRawMaterialStocks { get; set; } = new();
        public List<ProductionPlanItem> TodaysProductions { get; set; } = new();
        public List<ProductionPlanItem> TomorrowsProductions { get; set; } = new();
        public List<int> SalesChartData { get; set; } = new();
    }

    public class RecentOrderViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class StockAlertItem
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Product"; // or "RawMaterial"
        public int Stock { get; set; }
        public int CriticalStock { get; set; }
    }

    public class ProductionPlanItem
    {
        public string ProductName { get; set; } = string.Empty;
        public int BoxQuantity { get; set; }
        public DateTime StartTime { get; set; }
    }
}
