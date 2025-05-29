using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class LocationLog
    {
        [Key]
        public int VehicleId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public VehicleStatus Status { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Vehicle Vehicle { get; set; }
    }

    public enum VehicleStatus
    {
        Stopped = 0,
        Running = 1
    }
}
