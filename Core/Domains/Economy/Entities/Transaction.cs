using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Economy.Entities
{
    public class Transaction : BaseEntity
    {
        public Transaction() { AllowEvents = true; }
        [NotMapped]
        public override ContextNames Context => ContextNames.Money;
        public virtual Account Source { get; set; }
        public virtual Account Destination { get; set; }
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public string Mode { get; set; }
        public string PaymentAccount { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        //public TransactionStatusType Status { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string PaymentKey { get; set; }
        [MaxLength(200)]
        public string UniqueTransactionKey { get; set; }
        public LockType Locked { get; set; } = LockType.Open;
        public virtual Activity Activity { get; set; }
        public int? ActivityId { get; set; }

        [NotMapped]
        public virtual GatewayPayout Payout { get; set; }

        [NotMapped]
        public bool isTransactionHistory { get; set; } = false;

        [NotMapped]
        public TransactionType Type { get; set; }

        [NotMapped]
        public int CurrencyId { get; set; }

        [NotMapped]
        public Dictionary<string, string> Properties { get; set; }
    }
}

namespace Core
{
    public enum TransactionType
    {
        Payout,Debit,Credit
    }

    public enum LockType
    {
        Open, Locked, Rejected
    }
}