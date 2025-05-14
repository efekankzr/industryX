using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.Models
{
    public class ProductFormViewModel
    {
        public Product Product { get; set; } = new();
        public List<RawMaterial> RawMaterials { get; set; } = new();
    }
}
