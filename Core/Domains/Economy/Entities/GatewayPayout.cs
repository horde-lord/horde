using Horde.Core.Interfaces.Data;
using Horde.Core.Interfaces.Payment;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Economy.Entities
{
    public class GatewayPayout : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Money;
        public string GatewayName { get; set; }
        public string GatewaySubAccountName { get; set; }
        public PayoutType Type { get; set; }
        public int UserId { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public PayoutStatusType Status { get; set; }
        public decimal Fees { get; set; }
        public string BeneficiaryAccount { get; set; }
        public string GatewayOrderId { get; set; }
        [NotMapped]
        public GatewayPayoutStatus Result { get; set; }
        public string PayoutLink { get; set; }
        [NotMapped]
        public Transaction Transaction { get; set; }
    }
}

namespace Horde.Core
{
    public enum PayoutStatusType
    {
        Initiated, Pending, Succeeded, Failed, Canceled, Refunded, Undefined
    }

    public enum PayoutType
    {
        MatchWinning,Internal
    }
}