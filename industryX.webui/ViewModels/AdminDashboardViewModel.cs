namespace IndustryX.WebUI.ViewModels
{
    public class AdminDashboardViewModel
    {
        public bool HasWarehouse { get; set; }
        public bool HasMainProductWarehouse { get; set; }
        public bool HasMainRawMaterialWarehouse { get; set; }
        public bool HasMainSalesProductWarehouse { get; set; }
        public bool HasLaborCost { get; set; }
        public bool HasRawMaterial { get; set; }
        public bool HasProduct { get; set; }
        public bool HasCategory { get; set; }
        public bool HasSalesProduct { get; set; }

        public bool HasSalesManager { get; set; }
        public bool HasWarehouseManager { get; set; }
        public bool HasProductionManager { get; set; }
        public bool HasDriver { get; set; }
        public bool AllWarehousesHaveManager { get; set; }

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
    }
}
