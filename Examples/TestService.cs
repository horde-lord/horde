using Autofac;
using Horde.Core.Domains.World.Services;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Examples
{
    internal class TestService : BaseService, IHostedService
    {
        public TestService(ILifetimeScope scope) : base(scope, ContextNames.World)
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                GetRepository(ContextNames.World).CreateDatabase();
                //run migrations as explained in readme
                try
                {
                    await Get<WorldService>().Start();
                }
                catch(Exception ex)
                {
                    Log.Error(ex, "The world starter failed");
                }
            });
            //Initialize and migrate sqlite
            

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}