using IndustryX.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
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
