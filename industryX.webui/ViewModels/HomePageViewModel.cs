namespace IndustryX.WebUI.ViewModels
{
    public class HomePageViewModel
    {
        public List<ProductCardViewModel> BestSellers { get; set; } = new();
        public List<ProductCardViewModel> PopularProducts { get; set; } = new();
    }
}
