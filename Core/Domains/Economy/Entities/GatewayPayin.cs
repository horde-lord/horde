using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Economy.Entities
{
    public class GatewayPayin : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Money;
        public string GatewayName { get; set; } //Stripe , OnMeta
        public string GatewaySubAccountName { get; set; } // Upi , CreditCard
        public string DataJson { get; set; }
        public int? PayinTransactionId { get; set; }
        public Transaction PayinTransaction { get; set; }
        public int OrderId { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public decimal Amount { get; set; }
        public PayinStatusType Status { get; set; }
        public decimal Fees { get; set; } //Platform fees
        public string BeneficiaryAccount { get; set; }
        public string GatewayOrderId { get; set; } // Unique Id bw ta and platform
        public int GatewayInputAccountId { get; set; }
        public Account GatewayInputAccount { get; set; } // Unique for Platorm(stripe,OnMeta),Partner,currency Balance 
        public string Title { get; set; } // Gateway Title
        public string ImageUrl { get; set; } // Gateway Image
        public string Url { get; set; } // Gateway Url
        [NotMapped]
        public int TransactionHistoryId { get; set; }
        [NotMapped]
        public TransactionHistory TransactionHistory { get; set; }

    }
}

namespace Horde.Core
{
    public enum PayinStatusType
    {
        PaidButNotReflected = 0, Pending = 1, Succeeded = 2, Failed = 3,
        Undefined = 4,
        RefundInitiated = 5, RefundFailed = 6, Refunded = 8
    }
}