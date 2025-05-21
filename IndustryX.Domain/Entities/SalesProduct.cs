namespace IndustryX.Domain.Entities
{
    public class SalesProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public decimal SalePrice { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsPopular { get; set; }
        public bool IsBestSeller { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public ICollection<SalesProductImage> Images { get; set; } = new List<SalesProductImage>();
        public ICollection<SalesProductCategory> SalesProductCategories { get; set; } = new List<SalesProductCategory>();
    }
}
