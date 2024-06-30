using Autofac;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Domains.Economy.Services;
using Horde.Core.Domains.World.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horde.Core.Domains.Economy
{
    public class EconomyModule : Module
    {
        

        protected override void Load(ContainerBuilder builder)
        {
        
            builder.RegisterType<CurrencyService>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ActivityService>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<PaymentService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
