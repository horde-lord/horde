using Core.Domains.Economy.Entities;

namespace Core.Interfaces.Payment
{
    public interface IGatewayPayinProvider
    {
        static string GatewayName { get; }
        Task<GatewayPayin>CreateGatewayPayin(Transaction transaction);
        Task<PayinStatusType> GetPayinStatusInfo(GatewayPayin gatewayPayin);
    }
}
