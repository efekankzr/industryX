using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
    public class ProductTransferFormViewModel
    {
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int ProductId { get; set; }
        public int TransferQuantityBox { get; set; }

        public List<Product> Products { get; set; } = new();
        public List<Warehouse> Warehouses { get; set; } = new();
    }
}
