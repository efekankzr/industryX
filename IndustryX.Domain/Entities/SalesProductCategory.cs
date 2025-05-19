namespace IndustryX.Domain.Entities
{
    public class SalesProductCategory
    {
        public int SalesProductId { get; set; }
        public SalesProduct SalesProduct { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
