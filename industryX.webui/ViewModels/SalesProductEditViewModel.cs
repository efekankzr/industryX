using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public class SalesProductEditViewModel
    {
        public int Id { get; set; }

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

        public List<IFormFile> NewImages { get; set; } = new();
        public List<ExistingImageItem> ExistingImages { get; set; } = new();
    }

    public class ExistingImageItem
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
    }
}
