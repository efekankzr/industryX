namespace IndustryX.WebUI.ViewModels
{
    public class ProductCardViewModel
    {
        public SalesProductListItemViewModel Product { get; set; } = null!;
        public bool IsWishlist { get; set; } = false;
    }
}
