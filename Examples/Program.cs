using Autofac;
using Autofac.Extensions.DependencyInjection;
using Horde.Core.Domains.Economy;
using Horde.Core.Domains.World;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    builder.RegisterModule<WorldModule>();
                })
                .ConfigureServices(services =>
                {
                
                    services.AddHostedService<TestService>();
                })
                .Build()
                .RunAsync();
        }
    }
}
