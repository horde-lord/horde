using Horde.Core.Interfaces.Data;
using Horde.Core.Domains.Economy.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Examples.Data
{
    public class EconomyContext : EfCoreContext
    {

        //public EconomyContext(DbContextOptions options) : base(options) { }
        public EconomyContext(IConfiguration configuration) : base(configuration) { }
        public override ContextNames Name => ContextNames.Economy;
        public DbSet<Account> Accounts { get; set; }
        public DbSet<FinancialEntity> FinancialEntities { get; set; }
        public DbSet<GatewayPayout> GatewayPayouts { get; set; }
        public DbSet<GatewayPayin> GatewayPayins { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionHistory> TransactionHistory { get; set; }
        public DbSet<TransactionExceptionLog> TransactionExceptionLogs { get; set; }
        public DbSet<Adjustment> Adjustments { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Activity> Activities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("economy");
            modelBuilder.Entity<Account>()
                .HasIndex(c => new { c.AccountSponsorId, c.UserId, c.Type })
                .IsUnique();
            modelBuilder.Entity<Transaction>().
                HasIndex(c => c.UniqueTransactionKey)
                .IsUnique();
            SetQueryFilters(modelBuilder);

        }

        private void SetQueryFilters(ModelBuilder modelBuilder)
        {
            //var id = _tenant?.Id ?? 1;


            modelBuilder.Entity<Account>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<AccountSponsor>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<FinancialEntity>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<GatewayPayout>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<GatewayPayin>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Transaction>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<TransactionHistory>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<TransactionExceptionLog>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Adjustment>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Activity>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<ActivityLog>().HasQueryFilter(x => x.PartnerId == id);


        }
    }



}
