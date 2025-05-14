namespace IndustryX.Domain.Entities
{
    public class RawMaterialStock
    {
        public int Id { get; set; }

        public int RawMaterialId { get; set; }
        public RawMaterial RawMaterial { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public decimal Stock { get; set; }
        public decimal QruicalStock { get; set; }
    }
}
