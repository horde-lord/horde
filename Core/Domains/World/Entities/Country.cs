using Core.Interfaces.Data;

namespace Core.Domains.World.Entities
{
    public class Country : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        
        public string Name { get; set; }
        
        public string AlphaTwoCode { get; set; }
        
        public string AlphaThreeCode { get; set; }

        public string Region { get; set; }

        public int FiatCurrencyId { get; set; }
    }
}