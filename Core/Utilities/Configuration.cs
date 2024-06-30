using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Domains.Economy.Entities;
using Microsoft.Extensions.Configuration;

namespace Horde.Core.Utilities
{
    public class HordeConfiguration
    {
        public Currency DigitalCurrency { get; set; } = new Currency();
        public Tenant Owner { get; set; } = new Tenant();



    }

}
