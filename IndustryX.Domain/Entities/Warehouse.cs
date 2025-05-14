namespace IndustryX.Domain.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public bool IsMainForProduct { get; set; }
        public bool IsMainForRawMaterial { get; set; }
        public bool IsMainForSalesProduct { get; set; }

        public ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
        public ICollection<RawMaterialStock> RawMaterialStocks { get; set; } = new List<RawMaterialStock>();
    }
}
