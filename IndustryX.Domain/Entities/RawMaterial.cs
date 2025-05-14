namespace IndustryX.Domain.Entities
{
    public class RawMaterial
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public ICollection<ProductReceipt> ProductReceipts { get; set; } = new List<ProductReceipt>();
        public ICollection<RawMaterialStock> RawMaterialStocks { get; set; } = new List<RawMaterialStock>();
    }
}
