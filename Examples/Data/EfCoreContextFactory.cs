using Horde.Core.Domains.Admin;
using Microsoft.EntityFrameworkCore;

namespace Examples.Data
{
    public class EfCoreContextFactory<TContext> : IDbContextFactory<TContext> where TContext: EfCoreContext
    {
        private readonly IDbContextFactory<TContext> _pooledFactory;
        private readonly TenantManager _tenantManager;

        public EfCoreContextFactory(IDbContextFactory<TContext> contextFactory, TenantManager tenantManager)
        {
            _pooledFactory = contextFactory;
            _tenantManager = tenantManager;
        }
        public TContext CreateDbContext()
        {
            var tenant = _tenantManager.GetTenant();
            var context = _pooledFactory.CreateDbContext();
            
            context.SetTenant(tenant);
            return context;
        }
    }

    //public class EcosystemContextFactory : IDbContextFactory<EcosystemContext> 
    //{
    //    private readonly IDbContextFactory<EcosystemContext> _pooledFactory;
    //    private readonly TenantManager _tenantManager;

    //    public EcosystemContextFactory(IDbContextFactory<EcosystemContext> contextFactory, TenantManager tenantManager)
    //    {
    //        _pooledFactory = contextFactory;
    //        _tenantManager = tenantManager;
    //    }
    //    public EcosystemContext CreateDbContext()
    //    {

    //        var context = _pooledFactory.CreateDbContext();
    //        context.SetTenant(_tenantManager.GetTenant());
    //        return context;
    //    }
    //}

    //public class LeagueContextFactory : IDbContextFactory<LeagueContext>
    //{
    //    private readonly IDbContextFactory<LeagueContext> _pooledFactory;
    //    private readonly TenantManager _tenantManager;

    //    public LeagueContextFactory(IDbContextFactory<LeagueContext> contextFactory, TenantManager tenantManager)
    //    {
    //        _pooledFactory = contextFactory;
    //        _tenantManager = tenantManager;
    //    }
    //    public LeagueContext CreateDbContext()
    //    {

    //        var context = _pooledFactory.CreateDbContext();
    //        context.SetTenant(_tenantManager.GetTenant());
    //        return context;
    //    }
    //}

    //public class GameContextFactory : IDbContextFactory<GameContext>
    //{
    //    private readonly IDbContextFactory<GameContext> _pooledFactory;
    //    private readonly TenantManager _tenantManager;

    //    public GameContextFactory(IDbContextFactory<GameContext> contextFactory, TenantManager tenantManager)
    //    {
    //        _pooledFactory = contextFactory;
    //        _tenantManager = tenantManager;
    //    }
    //    public GameContext CreateDbContext()
    //    {

    //        var context = _pooledFactory.CreateDbContext();
    //        context.SetTenant(_tenantManager.GetTenant());
    //        return context;
    //    }
    //}



}
