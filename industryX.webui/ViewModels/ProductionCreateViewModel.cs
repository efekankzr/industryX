using System.ComponentModel.DataAnnotations;
using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
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
}
