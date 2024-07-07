using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Economy.Entities
{
    public class TransactionExceptionLog : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Economy;
        public int? TransactionId { get; set; }
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public string? Mode { get; set; }
        public string? PaymentAccount { get; set; }
        //public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Narration { get; set; }
        //public TransactionStatusType Status { get; set; }
        public string? EntityType { get; set; }
        public int EntityId { get; set; }
        public string? PaymentKey { get; set; }

        public static TransactionExceptionLog From(Transaction transaction, Exception ex)
        {
            var log = TransactionExceptionLog.From(transaction);
            log.Exception = ex.Message;
            log.StackTrace = ex.StackTrace;
            return log;
        }

        public static TransactionExceptionLog From(Transaction transaction)
        {
            TransactionExceptionLog log = new TransactionExceptionLog();
            log.PaymentKey = transaction.PaymentKey;
            log.PaymentAccount = transaction.PaymentAccount;
            log.Amount = transaction.Amount;
            log.DestinationId = transaction.DestinationId;
            log.Mode = transaction.Mode;
            log.EntityId = transaction.EntityId;
            log.EntityType = transaction.EntityType;
            log.SourceId = transaction.SourceId;
            log.Key = transaction.Key;
            log.TransactionId = transaction.Id;
            return log;
        }
    }

    public class TransactionException : Exception
    {
        public TransactionException(string message, string key) : base(message)
        {
            TransactionKey = key;
        }
        public string TransactionKey { get; set; }
        public string Key1 { get; set; }
        public string Value1 { get; set; }
        public string Key2 { get; set; }
        public string Value2 { get; set; }
        public string Key3 { get; set; }
        public string Value3 { get; set; }
    }
}
