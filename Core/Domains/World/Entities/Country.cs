using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.World.Entities
{
    public class Country : BaseEntity
    {
        public override ContextNames Context => ContextNames.World;
        
        public string Name { get; set; }
        
        public string AlphaTwoCode { get; set; }
        
        public string AlphaThreeCode { get; set; }

        public string Region { get; set; }

        public int FiatCurrencyId { get; set; }
    }
}