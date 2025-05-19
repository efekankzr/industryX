using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Url { get; set; }
    }
}
