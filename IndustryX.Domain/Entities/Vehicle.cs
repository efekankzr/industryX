using System.ComponentModel.DataAnnotations;

namespace IndustryX.Domain.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        public string PlateNumber { get; set; }

        [Required]
        public string DeviceId { get; set; }

        public List<VehicleUser>? VehicleUsers { get; set; }

        public LocationLog? LocationLog { get; set; }
    }
}
