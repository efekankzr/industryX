using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class SalesProductImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;

        public int SalesProductId { get; set; }
        public SalesProduct SalesProduct { get; set; }
    }
}
