namespace IndustryX.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ICollection<SalesProductCategory> SalesProductCategories { get; set; } = new List<SalesProductCategory>();
    }
}
