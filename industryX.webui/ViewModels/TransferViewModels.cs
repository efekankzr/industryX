using System.ComponentModel.DataAnnotations;
using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
    public class ProductTransferBaseViewModel
    {
        [Required]
        public int SourceWarehouseId { get; set; }

        [Required]
        public int DestinationWarehouseId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Transfer quantity must be at least 1 box.")]
        public int TransferQuantityBox { get; set; }
    }

    public class ProductTransferFormViewModel : ProductTransferBaseViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Warehouse> Warehouses { get; set; } = new();
    }

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

    public class AcceptTransferViewModel
    {
        [Required]
        public string TransferBarcode { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Delivered box count must be at least 1.")]
        public int DeliveredBoxCount { get; set; }

        // Read-only info
        public string ProductName { get; set; } = string.Empty;
        public int QuantityBox { get; set; }
        public string SourceWarehouse { get; set; } = string.Empty;
        public string DestinationWarehouse { get; set; } = string.Empty;
    }

    public class CompleteTransferViewModel
    {
        [Required]
        public string Barcode { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Received box count must be at least 1.")]
        public int ReceivedBoxCount { get; set; }

        // Read-only info
        public string ProductName { get; set; } = string.Empty;
        public int TotalBoxCount { get; set; }
        public string SourceWarehouse { get; set; } = string.Empty;
        public string DestinationWarehouse { get; set; } = string.Empty;
    }
}
