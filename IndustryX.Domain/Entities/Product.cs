using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal MaterialPrice { get; set; }
        public int PiecesInBox { get; set; }

        public ICollection<ProductReceipt> ProductReceipts { get; set; } = new List<ProductReceipt>();
        public ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
    }
}
