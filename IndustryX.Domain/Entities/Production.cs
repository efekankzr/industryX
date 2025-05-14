using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class Production
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        public int BoxQuantity { get; set; }

        public int TotalProducedPieces => BoxQuantity * Product.PiecesInBox;

        [Required]
        public int WorkerCount { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public ProductionStatus Status { get; set; } = ProductionStatus.Planned;

        public decimal TotalTime { get; set; }
        public decimal BreakOutTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Notes { get; set; } = string.Empty;

        public ICollection<ProductionPause> Pauses { get; set; } = new List<ProductionPause>();
    }


    public enum ProductionStatus
    {
        Planned = 0,
        InProgress = 1,
        Paused = 2,
        Completed = 3
    }
}
