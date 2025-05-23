namespace IndustryX.WebUI.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Setup Check Flags
        public bool HasWarehouse { get; set; }
        public bool HasMainProductWarehouse { get; set; }
        public bool HasMainRawMaterialWarehouse { get; set; }
        public bool HasMainSalesProductWarehouse { get; set; }
        public bool HasLaborCost { get; set; }
        public bool HasRawMaterial { get; set; }
        public bool HasProduct { get; set; }
        public bool HasCategory { get; set; }
        public bool HasSalesProduct { get; set; }

        // Role Assignments
        public bool HasSalesManager { get; set; }
        public bool HasWarehouseManager { get; set; }
        public bool HasProductionManager { get; set; }
        public bool HasDriver { get; set; }

        // Warehouse manager assignment check
        public bool AllWarehousesHaveManager { get; set; }

        // Computed property to determine if system setup is complete
        public bool IsSetupComplete =>
            HasWarehouse &&
            HasMainProductWarehouse &&
            HasMainRawMaterialWarehouse &&
            HasMainSalesProductWarehouse &&
            HasLaborCost &&
            HasRawMaterial &&
            HasProduct &&
            HasCategory &&
            HasSalesProduct &&
            HasSalesManager &&
            HasWarehouseManager &&
            HasProductionManager &&
            HasDriver &&
            AllWarehousesHaveManager;

        // Optional: Helper method to assign role presence dynamically
        public void SetRolePresence(string role, bool exists)
        {
            switch (role)
            {
                case "SalesManager":
                    HasSalesManager = exists;
                    break;
                case "WarehouseManager":
                    HasWarehouseManager = exists;
                    break;
                case "ProductionManager":
                    HasProductionManager = exists;
                    break;
                case "Driver":
                    HasDriver = exists;
                    break;
            }
        }
    }
}
