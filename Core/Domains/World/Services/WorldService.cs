using Autofac;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horde.Core.Domains.World.Services
{
    public class WorldService : BaseService
    {
        public WorldService(ILifetimeScope scope) : base(scope, ContextNames.World)
        {
        }

        public void Init()
        {
            
            //create partner
            //create create currency
            
        }
    }
}
