using Autofac;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Horde.Core.Domains.World.Services
{
    public class WorldService : BaseService
    {
        public WorldService(ILifetimeScope scope) : base(scope, ContextNames.World)
        {
        }

        public async Task  Init()
        {
            //Create owner
            var me = new User()
            {
                Username = "Me",
                
            };
            me = await Get<RegistrationService>().SearchOrCreateRegistration(me);
            //create partner
            var myOrg = new Tenant()
            {
                Key = "me",
                Name = "Me",
                
            }
            //create create currency
            
        }
    }
}
