using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
    public class ProductTransferFilterViewModel
    {
        public int? SourceWarehouseId { get; set; }
        public int? DestinationWarehouseId { get; set; }
        public TransferStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<Warehouse> Warehouses { get; set; } = new();
        public List<ProductTransfer> Transfers { get; set; } = new();
    }
}
