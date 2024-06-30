using Autofac;
using Horde.Core.Domains;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories
{
    public abstract class EfCoreContext : DbContext, IEntityContext
    {

        public EfCoreContext(ILifetimeScope scope) : base()
        {
            this._configuration = scope.Resolve<IConfiguration>();
            //this._tenantManager = scope.Resolve<TenantManager>();
        }

        protected EfCoreContext()
        {

        }

        public EfCoreContext(DbContextOptions options) : base(options)
        {
            
        }

        public void SetTenant(Tenant tenant)
        {
            _tenant = tenant;
        }

        protected int id => _tenant?.Id ?? 1;
        private readonly IConfiguration _configuration;
        private readonly ILifetimeScope _scope;
        protected Tenant _tenant;

        public Tenant Partner => _tenant;

        public virtual ContextNames Name { get; }
        public string _connectionString { get; private set; } = "Server=216.48.181.29;Database=btxsports;User ID=tribal;Password=d1sc0d@nc3;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            foreach (var entity in ChangeTracker.Entries<BaseEntity>())
            {
                if (entity.State == EntityState.Added)
                {
                    entity.Entity.CreatedAt = DateTime.UtcNow;
                    if (entity.Entity.PartnerId < 1)
                    {
                        if (Partner.Id < 1)
                            throw new Exception($"Tenant is not set correctly at Save. Current value {Partner.Id}");
                        entity.Entity.PartnerId = Partner.Id;
                    }
                    
                }
                if (entity.State == EntityState.Deleted)
                {
                    entity.State = EntityState.Modified;

                }
                if (entity.State == EntityState.Added || entity.State == EntityState.Modified || entity.State == EntityState.Deleted)
                {
                    entity.Entity.ModifiedAt = DateTime.UtcNow;
                }
                

                if (entity.Entity.GetType().IsAssignableTo(typeof(ICached)))
                {
                    //CacheEntity(entity.Entity);
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        private void CacheEntity(BaseEntity entity)
        {
            if (!entity.GetType().IsAssignableTo(typeof(ICached)))
                return;

            entity.FireEvent(EntityEventType.UpdateCache);
        }


        public override int SaveChanges()
        {
            foreach (var entity in ChangeTracker.Entries<BaseEntity>())
            {
                if (entity.State == EntityState.Added)
                {
                    entity.Entity.CreatedAt = DateTime.UtcNow;
                }

                if (entity.State == EntityState.Added || entity.State == EntityState.Modified || entity.State == EntityState.Deleted)
                {
                    entity.Entity.ModifiedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChanges();
        }

        public static void Configure(IServiceProvider services, DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = services.GetService(typeof(IConfiguration)) as IConfiguration;
            var connectionString = "";
            if (configuration != null && configuration.AsEnumerable().Any(c => c.Key == "BtxSportsDb"))
                connectionString = configuration["BtxSportsDb"];
            optionsBuilder

                //.LogTo(Console.WriteLine)
                //.UseLoggerFactory(loggerFactory)
                .UseSqlite(connectionString,
                r =>
                {
                    r.CommandTimeout(120);
                });


        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            


            base.OnModelCreating(modelBuilder);
        }

        public void CreateDatabase()
        {
            if (Database.GetDbConnection().Database.Contains("BtxSportsDb"))
            {
                Database.EnsureCreated();
            }


        }

        public void CreateSchemaObjects()
        {
            string sql = Database.GenerateCreateScript();
            var statements = sql.Split("GO").ToList();
            statements.ForEach(s =>
            {
                try
                {
                    Database.ExecuteSqlRaw(s);
                }
                catch
                {
                    //Tracer.Instance.TraceError(ex, "Error in creating " + this.Name);
                }

            });

        }

        public void DeleteDatabase()
        {
            if (Database.GetDbConnection().Database.Contains("LocalTest"))
            {
                Database.EnsureDeleted();
            }

        }

        async Task IEntityContext.SaveChanges()
        {
            await base.SaveChangesAsync();
        }
    }
}
