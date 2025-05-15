namespace IndustryX.Domain.Entities
{
    public class ProductTransfer
    {
        public int Id { get; set; }

        public string TransferBarcode { get; set; }

        public int SourceWarehouseId { get; set; }
        public Warehouse SourceWarehouse { get; set; }

        public int DestinationWarehouseId { get; set; }
        public Warehouse DestinationWarehouse { get; set; }

        public string InitiatedByUserId { get; set; }
        public ApplicationUser InitiatedByUser { get; set; }

        public string? DeliveredByUserId { get; set; }
        public ApplicationUser DeliveredByUser { get; set; }

        public string? ReceivedByUserId { get; set; }
        public ApplicationUser ReceivedByUser { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int TransferQuantityBox { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public TransferStatus Status { get; set; } = TransferStatus.Created;
    }


    public enum TransferStatus
    {
        Created = 0,         // Transfer oluşturuldu ama işlem başlamadı
        InTransit = 1,       // Ürün yolda (taşıyıcı teslim aldı)
        Delivered = 2        // Teslim tamamlandı
    }
}
