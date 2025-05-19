using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public class SalesProductCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal SalePrice { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsPopular { get; set; }
        public bool IsBestSeller { get; set; }

        [Required]
        public int ProductId { get; set; }

        public List<int> SelectedCategoryIds { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();
        public List<ProductItem> Products { get; set; } = new();

        [Display(Name = "Upload Images")]
        public List<IFormFile> Images { get; set; } = new();
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ProductItem
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
    }
}
