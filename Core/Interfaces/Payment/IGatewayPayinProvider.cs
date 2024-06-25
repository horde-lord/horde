using Horde.Core.Domains.Economy.Entities;

namespace Horde.Core.Interfaces.Payment
{
    public interface IGatewayPayinProvider
    {
        static string GatewayName { get; }
        Task<GatewayPayin>CreateGatewayPayin(Transaction transaction);
        Task<PayinStatusType> GetPayinStatusInfo(GatewayPayin gatewayPayin);
    }
}
