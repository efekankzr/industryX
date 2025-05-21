using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class UserAddress
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } // Ev, İş, vb.

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string District { get; set; }

        [Required]
        [MaxLength(500)]
        public string FullAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDefault { get; set; } = false;

    }
}
