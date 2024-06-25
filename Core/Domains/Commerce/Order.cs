using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Commerce
{
    public class Order : BaseEntity
    {
        public OrderStatusType Status { get; set; }
        public int UserId { get; set; }

        public override ContextNames Context => ContextNames.Commerce;
    }

    public class CommerceOrder : Order
    {
        //public override ContextNames Context => ContextNames.Commerce;
        public string PlatformOrderId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Vendor Vendor { get; set; }
        public int VendorId { get; set; }
        public Shipment Shipment { get; set; }
        public int? ShipmentId { get; set; }

    }

    public class DepositOrder : Order
    {

        public int TransactionId { get; set; } // Transaction Bw GatewayAccount and AccountId

        public string Narration { get; set; }

        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        [NotMapped]
        public string ImageUrl => "https://tribalassets.blob.core.windows.net/general/depositMoney.png";

        //public override ContextNames Context => ContextNames.Commerce;
    }
}

namespace Horde.Core
{
    public enum OrderStatusType
    {
        Draft, Confirmed, Canceled
    }
}