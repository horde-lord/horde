using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;


namespace Horde.Core.Domains.Commerce
{
    public class OrderItem : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public int VariantId { get; set; }
        public ProductVariant Variant { get; set; }
        public List<OrderItemTransaction> Transactions { get; set; }

        [NotMapped]
        public List<string> ReferenceImages { get; set; }
        public string OrderNote { get; set; }
        public CommerceOrder Order { get; set; }
        public int OrderId { get; set; }
        public int OrderedQty { get; set; } = 1;

        [NotMapped]
        public List<Transaction>TotalTransactions { get; set; }
    }
}
