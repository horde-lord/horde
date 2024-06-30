using Horde.Core.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using Horde.Core.Domains.Admin.Entities;
using Microsoft.Extensions.Configuration;

namespace Examples.Data
{
    public class AdminContext : EfCoreContext
    {

        public AdminContext(IConfiguration configuration) : base(configuration) { }

        public override ContextNames Name => ContextNames.Admin;
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Asset> Assets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("admin");


        }

        
    }



}
