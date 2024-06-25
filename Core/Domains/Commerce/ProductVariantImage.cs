using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.Commerce
{
    public class ProductVariantImage : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public ProductVariant Variant { get; set; }
        public int VariantId { get; set; }
        public string Url { get; set; }
        public int Order { get; set; } = 1;
    }
}
