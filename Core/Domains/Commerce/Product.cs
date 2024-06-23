using Core.Interfaces.Data;

namespace Core.Domains.Commerce
{
    public class Product : BaseEntity
    {
        public override ContextNames Context => ContextNames.Commerce;
        public Vendor Vendor { get; set; }
        public int VendorId { get; set; }
        public string Category { get; set; }

        
        public string Title { get; set; }

    }
}
