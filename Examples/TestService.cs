using Autofac;
using Horde.Core.Domains.World.Services;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Microsoft.Extensions.Hosting;

namespace Examples
{
    internal class TestService : BaseService, IHostedService
    {
        public TestService(ILifetimeScope scope) : base(scope, ContextNames.World)
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //Initialize and migrate sqlite
            GetRepository(ContextNames.World).CreateDatabase();
            //run migrations as explained in readme

            await Get<WorldService>().Init();

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}