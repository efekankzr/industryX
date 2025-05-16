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

        public bool IsSetupComplete =>
            HasWarehouse &&
            HasMainProductWarehouse &&
            HasMainRawMaterialWarehouse &&
            HasMainSalesProductWarehouse &&
            HasLaborCost &&
            HasRawMaterial &&
            HasProduct;
    }
}
