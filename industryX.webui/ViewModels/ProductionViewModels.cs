using System.ComponentModel.DataAnnotations;
using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
    public class ProductionListViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int BoxQuantity { get; set; }
        public int TotalProducedPieces { get; set; }
        public ProductionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductionCreateViewModel
    {
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Box Quantity")]
        public int BoxQuantity { get; set; }

        [Required]
        [Display(Name = "Worker Count")]
        public int WorkerCount { get; set; }

        public string? Notes { get; set; }

        public List<Product>? AvailableProducts { get; set; }
    }

    public class ProductionFormViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Box quantity must be at least 1.")]
        public int BoxQuantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Worker count must be at least 1.")]
        public int WorkerCount { get; set; }

        public string? Notes { get; set; }

        public List<Product> Products { get; set; } = new();
    }
}
