using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Commerce
{
    public class OrderItemTransaction : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; }

        [NotMapped]
        public Transaction Transaction { get; set; }
        public int TransactionId { get; set; }
    }
}