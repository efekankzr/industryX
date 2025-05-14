using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
    public class LaborCostFormViewModel
    {
        public decimal HourlyWage { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<LaborCost> ExistingCosts { get; set; } = new();
    }
}
