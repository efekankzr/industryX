namespace IndustryX.WebUI.ViewModels
{
    public class SalesProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<string> ImagePaths { get; set; } = new();
    }
}
