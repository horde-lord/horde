using Autofac;
using Autofac.Extensions.DependencyInjection;
using Examples.Data;
using Horde.Core.Domains.Admin;
using Horde.Core.Domains.Economy;
using Horde.Core.Domains.World;
using Horde.Core.Interfaces.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Examples
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule<EconomyModule>();
                    builder.RegisterType<EconomyContext>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
                    builder.RegisterType<EfCoreRepo<EconomyContext>>().As<IEntityContextRepository<IEntityContext>>();
                    
                    builder.RegisterModule<WorldModule>();
                    builder.RegisterType<WorldContext>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
                    builder.RegisterType<EfCoreRepo<WorldContext>>().As<IEntityContextRepository<IEntityContext>>();
                    builder.RegisterType<EfCoreContextFactory<WorldContext>>().InstancePerLifetimeScope();

                    builder.RegisterModule<AdminModule>();
                    builder.RegisterType<AdminContext>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
                    builder.RegisterType<EfCoreRepo<AdminContext>>().As<IEntityContextRepository<IEntityContext>>();

                    builder.RegisterType<Dap>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();

                })
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddMemoryCache();
                    services.AddHostedService<TestService>();
                    
                })
                .Build()
                .RunAsync();
        }
    }
}
