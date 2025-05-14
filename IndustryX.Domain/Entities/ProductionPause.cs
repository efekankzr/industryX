namespace IndustryX.Domain.Entities
{
    public class ProductionPause
    {
        public int Id { get; set; }

        public int ProductionId { get; set; }
        public Production Production { get; set; } = null!;

        public DateTime PausedAt { get; set; }
        public DateTime ResumedAt { get; set; }

        public decimal Duration { get; set; }
    }

}
