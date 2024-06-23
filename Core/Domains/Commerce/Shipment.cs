using Core.Interfaces.Data;

namespace Core.Domains.Commerce
{
    public class Shipment : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public List<CommerceOrder> Orders { get; set; }
        public Address Address { get; set; }
        public int? AddressId { get; set; }
        public ShipmentStatusType Status { get; set; }
        
    }
}

namespace Core
{
    public enum ShipmentStatusType {
        NotYetDispatched, Dispatched, Delivered, ReturnToSender
    }
}