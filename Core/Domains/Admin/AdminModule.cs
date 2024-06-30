using Autofac;

namespace Horde.Core.Domains.Admin
{
    public class AdminModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TenantManager>().AsSelf().AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            base.Load(builder);
        }
    }
}
