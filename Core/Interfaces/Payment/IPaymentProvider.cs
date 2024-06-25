using Horde.Core.Domains.Economy.Entities;

namespace Horde.Core.Interfaces.Payment
{
    public interface IPaymentProvider
    {
        string AccountName { get; }
        Task<GatewayPayout> SendPayment(Transaction transaction);
        BasePayoutInfo GetPayoutInfo();
    }

    public class GatewayPayoutStatus
    {
        public string Status { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public string GatewayOrderId { get; set; }
        public string Fees { get; set; }
        public string Beneficiary { get; set; }
        public string Utr { get; set; }
        public string PayoutLink { get; set; }
        public PayoutStatusType PayoutStatus { get; set; }

        public string GetNarration()
        {
            return $"{Code}:{Status}-{Description}";
        }
    }
    public abstract class BasePayoutInfo
    {
        abstract public string UserName { get; set; }
        abstract public string Email { get; set; }
        abstract public int Amount { get; set; }
        abstract public string VpaAddress { get; set; }
        abstract public string BankName { get; set; }

        abstract public string PhoneNumber { get; set; }

        abstract public string WithdrawlMessage { get; set; }


    }
}
