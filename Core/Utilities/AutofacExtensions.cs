using Autofac;
using Core.Domains.Admin;

namespace Core.Utilities
{
    public static class AutofacExtensions
    {
        

        public static ILifetimeScope GetChild(this ILifetimeScope scope, int? tenantId,bool refreshCache = false)
        {
            var childScope = scope.BeginLifetimeScope();
            var tenantManager = childScope.Resolve<TenantManager>();
            tenantManager.SetTenant(tenantId??1,refreshCache);
            return childScope;
        }
    }
}
