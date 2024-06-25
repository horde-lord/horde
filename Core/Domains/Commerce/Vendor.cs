using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.Commerce
{
    public class Vendor : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public string Name { get; set; }
        public List<Product> Products { get; set; }
    }
}