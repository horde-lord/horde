﻿using Autofac;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Horde.Core.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Horde.Core.Domains.Admin
{
    public class TenantManager
    {
        public TenantManager(ILifetimeScope scope, IMemoryCache cache)
        {
            _db = scope.Resolve<IRepoReader>();
            _repos = scope.Resolve<IEnumerable<IEntityContextRepository<IEntityContext>>>().ToList();
            _cache = cache;
            
        }
        private int _tenantId = 1;
        private List<Tenant> _tenants = new();
        private readonly IRepoReader _db;
        private readonly List<IEntityContextRepository<IEntityContext>> _repos;
        private readonly IMemoryCache _cache;

        public Tenant GetTenant()
        {
            if(_tenants == null || _tenants.Empty())
            {
                LoadTenants();
            }
            var tenant = _tenants.Where(t => t.Id == _tenantId).FirstOrDefault();
            return tenant;

        }

        public void Reload()
        {
            _tenants = LoadTenantsGraphFromDb();
            _cache.Set("tenants", _tenants, TimeSpan.FromMinutes(1));
        }

        private void LoadTenants()
        {
            _tenants = _cache.Get<List<Tenant>>("tenants");
            if (_tenants == null || _tenants.Empty())
            {
                Reload();
            }
        }

        private List<Tenant> LoadTenantsGraphFromDb()
        {
            var repo = _repos.LastOrDefault(r => r.Name == ContextNames.Admin);
            var tenants = repo.GetNoTrackingQueryable<Tenant>("Assets").ToList();
            
            //var assets = _<Asset>().ToList();
            //foreach(var tenant in tenants)
            //{
                
            //    tenant.Assets = assets.Where(assets => assets.TenantId == tenant.Id).ToList();
            //    tenant.Assets.ForEach(a => a.Tenant = tenant);
            //}
            return tenants;
        }

        

        

       
        public void SetTenant(int tenantId, bool refreshCache = false)
        {
            var tenant = _tenants?.FirstOrDefault(a => a.Id == tenantId);
            if (refreshCache)
                Reload();
            if (tenant == null)
            {
                LoadTenants();
                tenant = _tenants?.FirstOrDefault(a => a.Id == tenantId);
            }
            if (tenant == null)
            {
                throw new Exception($"Cant find a suitable tenant for tenant id {tenantId}");
            }
            _tenantId = tenant.Id;
        }

        public List<Tenant> GetTenants()
        {
            LoadTenants();
            return _tenants;
        }

        

        public Tenant GetTenant(int tenantId)
        {
            LoadTenants();
            var tenant = _tenants?.Find(t => t.Id == tenantId);
            return tenant;
        }

        

        
    }
}
