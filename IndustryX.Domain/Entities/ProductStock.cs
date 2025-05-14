namespace IndustryX.Domain.Entities
{
    public class ProductStock
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int Stock { get; set; }
        public int QruicalStock { get; set; }
        public decimal Price { get; set; }
    }
}
