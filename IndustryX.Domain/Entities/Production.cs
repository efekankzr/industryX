using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class Production
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int BoxQuantity { get; set; }

        public int TotalProducedPieces => BoxQuantity * Product.PiecesInBox;

        public int WorkerCount { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public ProductionStatus Status { get; set; }

        public decimal TotalTime { get; set; }
        public decimal BreakOutTime { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Notes { get; set; } = string.Empty;

        public ICollection<ProductionPause> Pauses { get; set; }
    }


    public enum ProductionStatus
    {
        Planned = 0,
        InProgress = 1,
        Paused = 2,
        Completed = 3
    }
}
