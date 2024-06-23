using Core.Domains.Economy.Entities;
using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Commerce
{
    public class Price : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public int VariantId { get; set; }
        public ProductVariant Variant { get; set; }
        public decimal SalePrice { get; set; }
        public int CurrencyId { get; set; }
        [NotMapped]
        public Currency Currency { get; set; }
    }
}