using Autofac;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Domains.Economy.Services;
using Horde.Core.Domains.World.Services;
using Horde.Core.Interfaces.Data;
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
        
            builder.RegisterType<CurrencyService>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ActivityService>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<PaymentService>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            
        }
    }
}
