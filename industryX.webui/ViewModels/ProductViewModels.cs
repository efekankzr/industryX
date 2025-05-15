using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public abstract class ProductBaseViewModel
    {
        [Required]
        public string Barcode { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Pieces in box must be at least 1.")]
        public int PiecesInBox { get; set; }

        public List<RawMaterialInputViewModel> RawMaterials { get; set; } = new();
    }

    public class ProductCreateViewModel : ProductBaseViewModel { }

    public class ProductEditViewModel : ProductBaseViewModel
    {
        public int Id { get; set; }
        public decimal MaterialPrice { get; set; }
    }

    public class RawMaterialInputViewModel
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; } = string.Empty;

        public bool Include { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal? Quantity { get; set; }
    }
}
