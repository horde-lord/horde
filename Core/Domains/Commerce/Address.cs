using Core.Interfaces.Data;

namespace Core.Domains.Commerce
{
    public class Address : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string PinCode { get; set; }
        public string State { get;set; }
        public string City { get; set; }
        public string Country { get;set; }
        public string AddressOne { get; set; }
        public string AddressTwo { get; set; }
    }
}