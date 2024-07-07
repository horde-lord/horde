using Autofac;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Domains.Economy.Services;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Serilog;
using shortid;

namespace Horde.Core.Domains.World.Services
{
    public class WorldService : BaseService
    {
        public WorldService(ILifetimeScope scope) : base(scope, ContextNames.World)
        {
        }

        public async Task  Init()
        {
            var myOrg = _<Tenant>(1);
            if(myOrg == null)
            {
                //create partner
                myOrg = new Tenant()
                {
                    Key = "me",
                    Name = "Me",
                    Description = "This is strictly for hello worlding"
                };
                await Save(myOrg);

            }

            using (var registrationService = GetTenanted<RegistrationService>(myOrg.Id))
            using (var currencyService = GetTenanted<CurrencyService>(myOrg.Id))
            using (var accountService = GetTenanted<AccountService>(myOrg.Id))
            using (var paymentService = GetTenanted<PaymentService>(myOrg.Id))
            {
                var currency = currencyService._<Currency>(1);
                if (currency == null)
                {
                    //Add Currency
                    currency = new Currency()
                    {
                        Name = "My Cool Currency",
                        ShortName = "MCC",
                        Type = CurrencyNatureType.DigitalCurrency,
                        Symbol = "MC🆒",
                        LogoUrl = "https://example.com/cool-logo.jpg",
                        Key = "mcc"
                    };
                    await currencyService.Save(currency);

                }

                //create global payin account for marketing.
                //This account is used as the central account where money can be added directly
                await accountService.GetOrCreateCentralAccounts(currency.Id, AccountSponsorType.Marketing);
                var centralPayinAccount = accountService.GetCentralPayinAccountBySponsorType(currency.Id, AccountSponsorType.Marketing);
                await paymentService.AddMoneyInGlobalAccountWithoutGateway(centralPayinAccount.Id, 1000);

                //Create user

                var me = await registrationService.SearchOrCreateRegistration("MySystemUserId", "Me", ConnectionType.Tenant);
                await ShowUserAccountBalances(paymentService, me);

                //Send me some cash
                await paymentService.TransferAmountFromGlobalAccount(me, 1, $"UniqueKeyForYourTransaction_{ShortId.Generate()}",
                    "nameof(YourEntityThatTrigerredTheTransfer)", 1, "GenericTransfer", centralPayinAccount.AccountSponsor,
                    purpose: "My eternal purpose to make transfers");
                await ShowUserAccountBalances(paymentService, me);

            }




        }

        private static async Task ShowUserAccountBalances(PaymentService paymentService, User me)
        {
            var accounts = await paymentService.GetAccountsForUser(me.Id);
            foreach (var account in accounts)
            {
                Log.Information($"Account {account.Name} has balance {paymentService.GetAccountBalance(account.Id)}");
            }
        }
    }
}
