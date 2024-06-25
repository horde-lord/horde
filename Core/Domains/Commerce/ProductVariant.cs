using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;

namespace Horde.Core.Domains.Commerce
{
    public class ProductVariant : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string VariationType { get; set; } = "Default";
        public string Variation { get; set; } = "Default";
        public List<ProductVariantImage> Images { get; set; }
        public string BulletPoints { get; set; }
        [MaxLength(100)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public int CurrencyId { get; set; }
        public List<Price> Prices { get; set; }
        public bool IsQtyAvailable { get; set; }
        
        public int TotalQty { get; set; }
    }

    

}
