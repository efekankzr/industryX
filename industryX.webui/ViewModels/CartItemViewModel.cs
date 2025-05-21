namespace IndustryX.WebUI.ViewModels
{
    public class CartItemViewModel
    {
        public int SalesProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
        public string? ImageUrl { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
