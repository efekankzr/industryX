namespace IndustryX.Domain.Entities
{
    public class LaborCost
    {
        public int Id { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal HourlyWage { get; set; }
    }
}
