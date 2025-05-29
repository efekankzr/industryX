using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Persistence.Contexts
{
    public class IndustryXDbContext : IdentityDbContext<ApplicationUser>
    {
        public IndustryXDbContext(DbContextOptions<IndustryXDbContext> options) : base(options) { }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<LaborCost> LaborCosts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Production> Productions { get; set; }
        public DbSet<ProductionPause> ProductionPauses { get; set; }
        public DbSet<ProductReceipt> ProductReceipts { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<ProductTransfer> ProductTransfers { get; set; }
        public DbSet<ProductTransferDeficit> ProductTransferDeficits { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<RawMaterialStock> RawMaterialStocks { get; set; }
        public DbSet<SalesProduct> SalesProducts { get; set; }
        public DbSet<SalesProductCategory> SalesProductCategories { get; set; }
        public DbSet<SalesProductImage> SalesProductImages { get; set; }
        public DbSet<SalesProductStock> SalesProductStocks { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleUser> VehicleUsers { get; set; }
        public DbSet<LocationLog> LocationLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SalesProductCategory>()
                .HasKey(spc => new { spc.SalesProductId, spc.CategoryId });

            modelBuilder.Entity<SalesProductCategory>()
                .HasOne(spc => spc.SalesProduct)
                .WithMany(sp => sp.SalesProductCategories)
                .HasForeignKey(spc => spc.SalesProductId);

            modelBuilder.Entity<SalesProductCategory>()
                .HasOne(spc => spc.Category)
                .WithMany(c => c.SalesProductCategories)
                .HasForeignKey(spc => spc.CategoryId);

            modelBuilder.Entity<OrderItem>()
                .Ignore(x => x.TotalPrice);

            modelBuilder.Entity<Order>(order =>
            {
                order.OwnsOne(o => o.BillingAddress);
                order.OwnsOne(o => o.ShippingAddress);
            });
        }
    }
}
