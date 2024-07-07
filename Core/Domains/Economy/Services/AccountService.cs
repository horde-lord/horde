using Autofac;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horde.Core.Domains.Economy.Services
{
    class AccountService : BaseService
    {
        public AccountService(ILifetimeScope scope) : base(scope, ContextNames.Economy)
        {

        }

        public async Task<AccountSponsor> GetOrCreateCentralAccounts(int currencyId, AccountSponsorType sponsorType)
        {
            var sponsor = _<AccountSponsor>().FirstOrDefault(a => a.CurrencyId == currencyId && a.Type == sponsorType);
            if (sponsor != null)
                return sponsor;

            

            var currency = __<Currency>().FirstOrDefault(c => c.Id == currencyId);

            if (currency == null)
                throw new Exception($"Currency not found - currencyId {currencyId}");
            

            var accounts = new List<Account>();
            var payinAccount = new Account()
            {
                Balance = 0.0M,
                CurrencyId = currencyId,
                Name = "Pay-in " + currency.ShortName + " Account",
                Type = AccountType.Global,
                Purpose = sponsorType.ToString() + " payin",
                UserId = null,
            };
            accounts.Add(payinAccount);

            var payoutAccount = new Account()
            {
                Balance = 0.0M,
                CurrencyId = currencyId,
                Name = "Pay-out " + currency.ShortName + " Account",
                Type = AccountType.Global,
                Purpose = sponsorType.ToString() + " payout",
                UserId = null,
            };
            accounts.Add(payoutAccount);

            await Save(accounts);

            sponsor = new AccountSponsor()
            {
                CurrencyId = currencyId,
                PayInAccountId = accounts[0].Id,
                PayOutAccountId = accounts[1].Id,
                UserId = 0,
                Key = sponsorType.ToString(),
                Type = sponsorType,
                ProviderName = "",
                ProviderCredentials = "",
            };
            await Save(sponsor);

            foreach (var account in accounts)
                account.AccountSponsorId = sponsor.Id;

            await Save(accounts);

            return sponsor;
        }


        public Account GetCentralPayinAccountBySponsorType(int currencyId, AccountSponsorType type)
        {
            if (currencyId <= 0) currencyId = 1;
            var sponsor = GetSponsorByType(type, currencyId);

            return _<Account>(sponsor.PayInAccountId, "AccountSponsor");
        }

        public Account GetCentralPayoutAccountBySponsorType(int currencyId, AccountSponsorType type)
        {
            if (currencyId <= 0) currencyId = 1;
            var sponsor = GetSponsorByType(type, currencyId);


            return _<Account>(sponsor.PayOutAccountId);
        }

        public AccountSponsor GetSponsorByType(AccountSponsorType type, int currencyId)
        {
            if (currencyId <= 0) currencyId = 1;
            return _<AccountSponsor>().FirstOrDefault(a => a.Type == type && a.CurrencyId == currencyId);
        }

    }
}
