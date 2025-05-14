using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IndustryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Persistence.Contexts
{
    public class IndustryXDbContext : IdentityDbContext<ApplicationUser>
    {
        public IndustryXDbContext(DbContextOptions<IndustryXDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<ProductReceipt> ProductReceipts { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<RawMaterialStock> RawMaterialStocks { get; set; }
        public DbSet<LaborCost> LaborCosts { get; set; }
        public DbSet<Production> Productions { get; set; }
        public DbSet<ProductionPause> ProductionPauses { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
