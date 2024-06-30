using Horde.Core.Interfaces.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Horde.Core.Domains.Admin.Entities;

namespace Infrastructure.DataContexts
{
    public class AdminContext : EfCoreContext
    {

        public AdminContext(DbContextOptions options) : base(options) { }

        public override ContextNames Name => ContextNames.Admin;
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Asset> Assets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("admin");


        }

        
    }



}
