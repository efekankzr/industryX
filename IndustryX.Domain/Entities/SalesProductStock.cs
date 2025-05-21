namespace IndustryX.Domain.Entities
{
    public class SalesProductStock
    {
        public int Id { get; set; }

        public int SalesProductId { get; set; }
        public SalesProduct SalesProduct { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int Stock { get; set; }
        public int CriticalStock { get; set; }
        public decimal Price { get; set; }
    }
}
