using System.ComponentModel.DataAnnotations.Schema;

namespace IndustryX.Domain.Entities
{
    public class ProductReceipt
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int RawMaterialId { get; set; }
        public RawMaterial RawMaterial { get; set; }

        public decimal Quantity { get; set; }

        [NotMapped]
        public bool Include { get; set; }
    }
}
