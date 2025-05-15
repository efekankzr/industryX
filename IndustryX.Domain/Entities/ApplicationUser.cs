using Microsoft.AspNetCore.Identity;

namespace IndustryX.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Surname { get; set; }

        public int? WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}
