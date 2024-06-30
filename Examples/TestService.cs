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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Get<WorldService>().Init();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}