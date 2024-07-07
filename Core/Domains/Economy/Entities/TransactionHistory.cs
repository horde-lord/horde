using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Economy.Entities
{
    public class TransactionHistory : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Economy;
        public Account Source { get; set; }
        public Account Destination { get; set; }
        public int? SourceId { get; set; }
        public int DestinationId { get; set; }
        public string? Mode { get; set; }
        public string? PaymentAccount { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Narration { get; set; }
        public TransactionStatusType Status { get; set; }
        public string? EntityType { get; set; }
        public int EntityId { get; set; }
        public string? PaymentKey { get; set; }

        [NotMapped]
        public GatewayPayout Payout { get; set; }

        public Transaction AdaptTransaction()
        {
            LockType? lockType = null;

            if (Status == TransactionStatusType.Open)
                lockType = LockType.Open;
            else if (Status == TransactionStatusType.Locked)
                lockType = LockType.Locked;
            else if (Status == TransactionStatusType.Rejected)
                lockType = LockType.Rejected;
            
            var transaction = new Transaction()
            {
                Id = Id,
                Key = Key,
                Source = Source,
                Destination = Destination,
                SourceId = SourceId??0,
                DestinationId = DestinationId,
                Mode = Mode,
                CreatedAt = CreatedAt,
                ModifiedAt = ModifiedAt,
                Deleted = Deleted,
                PaymentAccount = PaymentAccount,
                Type = Type,
                isTransactionHistory = true,
                Amount = Amount,
                Narration = Narration,
                Locked = lockType??LockType.Locked,
                EntityType = EntityType,
                EntityId = EntityId,
                PaymentKey = PaymentKey,
                Payout = Payout
            };

            return transaction;
        }
    }
}

namespace Horde.Core
{
    public enum TransactionStatusType
    {
        Open, Locked, Rejected
    }
}
