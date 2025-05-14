using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
    public class ProductionListViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int BoxQuantity { get; set; }
        public int TotalProducedPieces { get; set; }
        public ProductionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
