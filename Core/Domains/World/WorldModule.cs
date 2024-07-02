using Autofac;
using Horde.Core.Domains.World.Services;

namespace Horde.Core.Domains.World
{
    public class WorldModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RegistrationService>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<WorldService>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
