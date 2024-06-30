using Horde.Core.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using Horde.Core.Domains.Commerce;
using Microsoft.Extensions.Configuration;

namespace Examples.Data
{
    public class CommerceContext : EfCoreContext
    {
        public override ContextNames Name => ContextNames.Commerce;
        public CommerceContext(IConfiguration configuration) : base(configuration) { }
        //public CommerceContext(DbContextOptions options) : base(options)
        //{
        //}
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemTransaction> OrderItemTransactions { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> Variants { get; set; }
        public DbSet<ProductVariantImage> Images { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<DepositOrder> DepositOrders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("commerce");
            modelBuilder.Entity<DepositOrder>().HasBaseType<Order>();
            modelBuilder.Entity<CommerceOrder>().HasBaseType<Order>();
            SetQueryFilters(modelBuilder);
            //base.OnModelCreating(modelBuilder);
        }
        private void SetQueryFilters(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Order>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<OrderItemTransaction>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Price>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Product>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<ProductVariant>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<ProductVariantImage>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Shipment>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Vendor>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Address>().HasQueryFilter(x => x.PartnerId == id);

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}


