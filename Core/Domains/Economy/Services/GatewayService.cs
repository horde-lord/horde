using Autofac;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Interfaces.Payment;
using Horde.Core.Services;
using Horde.Core.Utilities;
using Serilog;
using Account = Horde.Core.Domains.Economy.Entities.Account;
using Horde.Core.Domains.Economy.Services;
using Horde.Core.Domains.Economy.Entities;
using Microsoft.Extensions.Hosting;
using Horde.Core.Domains.Commerce;
using Horde.Core.Domains.Commerce.Services;

namespace Horde.Core.Domains.Economy.Services
{
    public class GatewayService : BaseService, IHostedService
    {
        public GatewayService(ILifetimeScope scope, ContextNames name = ContextNames.Economy) : base(scope, name)
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ProcessPendingGatewayPayins();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task ProcessPendingGatewayPayins()
        {
            while (true)
            {
                try
                {
                    var gateways = __<GatewayPayin>().Where(x => ((x.Status == PayinStatusType.Pending ||
            x.Status == PayinStatusType.Undefined) && x.CreatedAt > DateTime.UtcNow.AddDays(-15)) ||
            x.Status == PayinStatusType.PaidButNotReflected).ToList() ?? new List<GatewayPayin>();
                    foreach (var gateway in gateways)
                    {
                        try
                        {
                            await CheckAndUpdateToLatestStatus(gateway);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Failed to update gateway payin status {gateway.Id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to update gateway payin status");
                }
                await Task.Delay(1000 * 5 * 60);
            }
        }


        public async Task<GatewayPayin> SuccessGatewayPayin(int gatewayPayinId)
        {
            var gatewayPayin = __<GatewayPayin>(gatewayPayinId);
            if (gatewayPayin == null)
                throw new Exception($"Gateway Payin not found {gatewayPayinId}");
            if (!ShouldUpdatePayinStatus(gatewayPayin))
                return gatewayPayin;
            gatewayPayin = await CheckAndUpdateToLatestStatus(gatewayPayin);
            return gatewayPayin;
        }

        public async Task<GatewayPayin> PaidButNotReflectedGatewayPayin(int gatewayPayinId)
        {
            var gatewayPayin = __<GatewayPayin>(gatewayPayinId);
            if (gatewayPayin == null)
                throw new Exception($"Gateway Payin not found {gatewayPayinId}");
            if (!ShouldUpdatePayinStatus(gatewayPayin))
                return gatewayPayin;
            var gatewayPain = await CheckAndUpdateToLatestStatus(gatewayPayin);
            return await ForceUpdateGatewayPayinStatus(gatewayPayinId, PayinStatusType.PaidButNotReflected);
        }

        public async Task<GatewayPayin> CancelGatewayPayin(int gatewayPayinId)
        {
            var gatewayPayin = __<GatewayPayin>(gatewayPayinId);
            if (gatewayPayin == null)
                throw new Exception($"Gateway Payin not found {gatewayPayinId}");
            if (!ShouldUpdatePayinStatus(gatewayPayin))
                return gatewayPayin;
            var gatewayPain = await CheckAndUpdateToLatestStatus(gatewayPayin);
            return await ForceUpdateGatewayPayinStatus(gatewayPayinId, PayinStatusType.Failed);
        }

        public async Task<GatewayPayin> CheckAndUpdateToLatestStatus(GatewayPayin gatewayPayin)
        {
            if (gatewayPayin == null)
                return null;
            if (ShouldUpdatePayinStatus(gatewayPayin))
            {
                var provider = Scope.ResolveNamed<IGatewayPayinProvider>(gatewayPayin.GatewayName);
                if (provider != null)
                {
                    var status = await provider.GetPayinStatusInfo(gatewayPayin);
                    if (status != gatewayPayin.Status)
                    {
                        return await ForceUpdateGatewayPayinStatus(gatewayPayin.Id, status);
                    }
                }
            }
            return gatewayPayin;
        }


        public async Task<GatewayPayin> ForceUpdateGatewayPayinStatus(int gatewayPayinId, PayinStatusType status)
        {
            var gatewayPayin = __<GatewayPayin>().FirstOrDefault(x => x.Id == gatewayPayinId);
            if (gatewayPayin == null)
                throw new Exception($"No gateway payin found with id {gatewayPayinId}");
            if (gatewayPayin.Status == status)
                return gatewayPayin;
            var partnerId = gatewayPayin?.PartnerId ?? 0;
            if (partnerId <= 0)
                throw new Exception($"No partner found with id {partnerId}");
            var service = Scope.GetChild(partnerId).Resolve<PaymentService>();
            var order = service.__<Order>().FirstOrDefault(x => x.Id == gatewayPayin.OrderId);
            if (order == null)
                throw new Exception($"No order found with id {gatewayPayin.OrderId}");
            var userId = order.UserId;
            if (userId <= 0)
                throw new Exception($"No user found with id {userId}");
            var currencyId = gatewayPayin?.CurrencyId ?? 0;
            if (currencyId <= 0)
                throw new Exception($"No currency found with id {currencyId}");
            if (gatewayPayin.Status == PayinStatusType.Failed)
                return gatewayPayin;
            if (gatewayPayin.Status == PayinStatusType.Succeeded)
                return gatewayPayin;
            if (status == PayinStatusType.Succeeded)
            {
                order.Status = OrderStatusType.Confirmed;
                await service.GetNew<PaymentService>().Save(order);
                gatewayPayin.Status = status;
                await GetNew<GatewayService>().Save(gatewayPayin);
                try
                {
                    await service.GetNew<CommerceService>().ProcessOrder(order.Id);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to process order {order.Id}");
                }
            }
            else
            {
                gatewayPayin.Status = status;
                await GetNew<GatewayService>().Save(gatewayPayin);
            }
            return gatewayPayin;
        }

        public string GetGatewayApiKeyForPartner(AssetType type, int partnerId)
        {
            var apiKey = __<Asset>().FirstOrDefault(x => x.Type == type && x.PartnerId == 1)?.Value ?? "";
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception($"No api key found for partner {partnerId} type {type}");
            return apiKey;
        }

        public async Task<GatewayPayin> CreateGatewayPayin(int orderId, string gatewayName, string gatewaySubAccount, string gatewayUrl, string orderUrl)
        {
            var order = __<Order>().FirstOrDefault(x => x.Id == orderId);
            if (order == null)
                throw new Exception($"No order found with id {orderId}");
            var partnerId = order?.PartnerId ?? 0;
            if (partnerId <= 0)
                throw new Exception($"No partner found with id {partnerId}");
            var mainService = Scope.GetChild(partnerId).Resolve<PaymentService>();
            if (string.IsNullOrEmpty(gatewayName))
                throw new Exception($"No gateway name not found");
            if (string.IsNullOrEmpty(gatewaySubAccount))
                throw new Exception($"No gateway sub account found");
            var existingSuccessPayin = __<GatewayPayin>().Where(x => x.OrderId == orderId && (x.Status == PayinStatusType.Succeeded || 
            x.Status == PayinStatusType.PaidButNotReflected)).FirstOrDefault();
            if (existingSuccessPayin != null)
                return existingSuccessPayin;
            var existingPayins = __<GatewayPayin>().Where(x => x.OrderId == orderId && x.Status != PayinStatusType.Failed).ToList();
            foreach (var existingPayin in existingPayins)
                await mainService.GetNew<GatewayService>().CancelGatewayPayin(existingPayin.Id);

            var gatewayPayin = CreateEmptyGatewayPayin(orderId);
            if (gatewayPayin == null)
                throw new Exception($"No gateway payin found with order id {orderId}");
            var account = await GetNew<PaymentService>().
                GetGatewayAccount(order.PartnerId, gatewayPayin.CurrencyId, gatewayName, AccountType.GatewayInput);
            gatewayPayin.GatewayInputAccountId = account.Id;
            await GetNew<GatewayService>().Save(gatewayPayin);
            var currencyId = gatewayPayin?.CurrencyId ?? 0;
            if (currencyId <= 0)
                throw new Exception($"No currency found with id {currencyId}");


            if (gatewayPayin.Status == PayinStatusType.Pending || gatewayPayin.Status == PayinStatusType.Undefined)
            {
                var gatewayOrderId = DateTime.UtcNow.Ticks.ToString();
                
                gatewayPayin.GatewayName = gatewayName;
                gatewayPayin.GatewaySubAccountName = gatewaySubAccount;
                gatewayPayin.GatewayOrderId = gatewayOrderId;
                await GetNew<GatewayService>().Save(gatewayPayin);
            }
            return gatewayPayin;
        }

        public bool ShouldUpdatePayinStatus(GatewayPayin gatewayPayin)
        {
            if (gatewayPayin == null)
                return false;
            if (gatewayPayin.Status == PayinStatusType.PaidButNotReflected ||
                gatewayPayin.Status == PayinStatusType.Pending || 
                gatewayPayin.Status == PayinStatusType.Undefined)
            {
                if (string.IsNullOrEmpty(gatewayPayin.GatewayName) || string.IsNullOrEmpty(gatewayPayin.GatewayOrderId))
                    return false;
                return true;
            }
            return false;
        }




        private async Task<TransactionHistory> CreateGatewayPayinTransactionHistory(GatewayPayin gatewayPayin)
        {
            if (gatewayPayin == null)
                throw new Exception($"No gateway payin found");
            var partnerId = gatewayPayin?.PartnerId ?? 0;
            var currencyId = gatewayPayin?.CurrencyId ?? 0;
            if (partnerId <= 0)
                throw new Exception($"No partner found with id {partnerId}");
            if (currencyId <= 0)
                throw new Exception($"No currency found with id {currencyId}");

            var service = Scope.GetChild(partnerId).Resolve<PaymentService>();
            var globalAccountSponsor = await service.GetNew<AccountService>().GetOrCreateCentralAccounts(currencyId, AccountSponsorType.Private);
            var payInAccount = _<Account>().FirstOrDefault(a => a.Id == globalAccountSponsor.PayInAccountId);
            if (payInAccount == null)
                throw new Exception($"No payin account found with id {globalAccountSponsor.PayInAccountId}");

            var transactionHistory = await GetNew<PaymentService>().CreateOrUpdateDraftTransactionHistoryForAddingGlobalAccountMoney(payInAccount.Id,
               gatewayPayin.Amount, gatewayPayin.GatewayName, gatewayPayin.TransactionHistoryId);
            return transactionHistory;
        }


        public GatewayPayin CreateEmptyGatewayPayin(int orderId)
        {
            var order = __<Order>().FirstOrDefault(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Order is null");

            var currencyId = 0;
            Currency currency = null;
            decimal amount = 0;
            var title = "";
            var imageUrl = "";
            if (order is DepositOrder)
            {
                var depositOrder = order as DepositOrder;
                if (depositOrder == null)
                    throw new Exception($"No deposit order found with id {order.Id}");
                var account = __<Account>("Currency").FirstOrDefault(a => a.Id == depositOrder.AccountId);
                currencyId = account?.CurrencyId ?? 0;
                currency = account?.Currency;
                amount = depositOrder.Amount;
                title = "Add Money";
                imageUrl = "https://tribalassets.blob.core.windows.net/general/depositMoney.png";
            }

            if (currencyId <= 0)
                throw new Exception($"Currency id cannot be empty");
            if (amount <= 0)
                throw new Exception($"Amount cannot be empty");

            var gatewayPayin = new GatewayPayin
            {
                Amount = amount,
                OrderId = order.Id,
                PartnerId = order.PartnerId,
                Status = PayinStatusType.Pending,
                CurrencyId = currencyId,
                Currency = currency,
                Url = "",
                Title = title,
                ImageUrl = imageUrl,
                Deleted = false
            };
            return gatewayPayin;
        }





    }
}
