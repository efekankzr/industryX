using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int SalesProductId { get; set; }
        public SalesProduct SalesProduct { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
