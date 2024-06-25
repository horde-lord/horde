using Autofac;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Transaction = Horde.Core.Domains.Economy.Entities.Transaction;
using Horde.Core.Domains.Economy.Services;
using Horde.Core.Domains.Economy.Entities;

namespace Horde.Core.Domains.Economy.Services
{
    public class VirtualCurrencyService : BaseService
    {
        public VirtualCurrencyService(ILifetimeScope scope, ContextNames name = ContextNames.Money) : base(scope, name)
        {
        }


        public async Task<Account> GetUserVirtualCurrencyWallet(int userId, int currencyId)
        {
            if (currencyId <= 0)
                currencyId = Partner.DigitalCurrencyId;
            var currency = _<Currency>(currencyId);
            if(currency.Type != CurrencyNatureType.DigitalCurrency)
            {
                throw new Exception("Only digital currencies can be fetched with this function");
            }
            var paymentService = Get<PaymentService>();
            var accountSponsor = _<AccountSponsor>().Where(a => a.CurrencyId == currencyId).FirstOrDefault();
            if (accountSponsor == null)
                throw new Exception($"No account sponsor found for digital currency and partner {Partner.Id}");
            var account = await paymentService.GetUserAccount(userId, accountSponsor.Id);
            account.Balance = paymentService.GetAccountBalance(account.Id);
            account.LockedBalance = paymentService.GetLockedBalance(account.Id);
            return account;
        }

        public async Task TransferVirtualCurrencyByUserId(int destinationUserId, int sourceUserId, decimal amount, string uniqueKey, BaseEntity entity,
      string paymentKey, string purpose, int currencyId)
        {
            var sourceAccount = await GetUserVirtualCurrencyWallet(sourceUserId, currencyId);
            var destinationAccount = await GetUserVirtualCurrencyWallet(destinationUserId, currencyId);
            await TransferVirtualCurrency(destinationAccount, sourceAccount, amount, uniqueKey, entity, paymentKey, purpose);
        }


        public async Task TransferVirtualCurrency(Account destination, Account source, decimal amount, string uniqueKey, BaseEntity entity,
         string paymentKey, string purpose)
        {
            var paymentService = Get<PaymentService>();
            await paymentService.Transfer(amount, key: uniqueKey,
               entityType: entity.GetType().Name, entityKey: entity.Id,
               paymentKey: paymentKey, purpose: purpose, gatewayAccount: "", mode: "Internal", destination, source);
        }


        public async Task<Transaction> AwardVirtualCurrency(Account destination, decimal amount, string uniqueKey, BaseEntity entity,
            string paymentKey, string purpose, string mode = "Award")
        {
            var paymentService = GetNew<PaymentService>();
            if(destination == null)
            {
                throw new Exception("Destination account not found");
            }
            if(destination?.Currency == null)
                destination.Currency = __<Currency>(destination.CurrencyId);
            if((destination?.Currency?.Type??CurrencyNatureType.Fiat) != CurrencyNatureType.DigitalCurrency)
            {
                throw new Exception("Only digital currencies can be awarded with this function");
            }
            var source = __<Account>().Where(a => a.AccountSponsorId == destination.AccountSponsorId && 
            a.Id == a.AccountSponsor.PayInAccountId && !a.Deleted).SingleOrDefault();
            if(source == null)
                throw new Exception($"No source global account found for digital currency and partner {Partner.Id}");
            await paymentService.Transfer(amount, key: uniqueKey,
                entityType: entity.GetType().Name, entityKey: entity.Id,
                paymentKey: paymentKey, purpose: purpose, gatewayAccount: "", mode: mode, destination, source);
            var transaction = __<Transaction>().FirstOrDefault(t => t.Key == uniqueKey);
            return transaction;
        }


        public async Task AwardJoiningVirtualCurrency(int userId, int refererUserId = 0)
        {
            var account = await GetUserVirtualCurrencyWallet(userId, Partner.DigitalCurrencyId);
            var user = _<User>(userId);
            await AwardVirtualCurrency(account, 10, $"Verified_{userId}", user, "NewVerification", $"User {user.Username} getting verified for first time");
            if (refererUserId > 0 && refererUserId != userId)
            {
                var referer = _<User>(refererUserId);
                var refererAccount = await GetUserVirtualCurrencyWallet(refererUserId, Partner.DigitalCurrencyId);
                await AwardVirtualCurrency(refererAccount, 5, $"Referred_{userId}", user, "NewVerification", $"User {user.Username} invite referral bonus");
            }

            var member = _<Member>("Tribe.Owner").OrderBy(m => m.Id).FirstOrDefault(m => m.UserId == userId);
            if (member == null)
                return;
            var owner = member.Tribe.Owner;
            if (owner.Id == user.Id)
                return;
            if (owner.Id == refererUserId)
                return;
            var ownerAccount = await GetUserVirtualCurrencyWallet(owner.Id, Partner.DigitalCurrencyId);
            await AwardVirtualCurrency(ownerAccount, 5, $"ReferredMember_{member.Id}", member, "MemberReferral", $"User {user.Id}:{user.Username} referral bonus to owner of {member.Tribe.Name}");

        }




        public async Task ExchangeVirtualCurrency(decimal amountRequired, int userId, int digitalCurrencyId)
        {
            var paymentService = GetNew<PaymentService>();
            var user = _<User>(userId);
            var wallets = await paymentService.GetAccountsForUser(userId);
            var virtualWallet = await GetUserVirtualCurrencyWallet(userId, digitalCurrencyId);
            var gameWallet = wallets.FirstOrDefault(w => w.CurrencyId == user.CurrencyId &&
            w.AccountSponsor.Type == AccountSponsorType.Gaming && w.Deleted == false);
            var incentiveWallet = wallets.FirstOrDefault(w => w.CurrencyId == user.CurrencyId &&
            w.AccountSponsor.Type == AccountSponsorType.Marketing && w.Deleted == false);
            decimal balance = 0.0M;
            balance = gameWallet?.Balance + incentiveWallet?.Balance ?? 0.0M;
            if (balance < amountRequired / 2)
            {
                throw new Exception("Insufficient balance");
            }
            var amountInFiat = CalculateConversion(amountRequired, user.CurrencyId, virtualWallet.CurrencyId);
            var amountInVirtualCurrency = amountRequired;
            if (gameWallet != null && gameWallet.Balance > 0)
            {
                if (amountInFiat > gameWallet.Balance)
                {
                    amountInVirtualCurrency = CalculateConversion(gameWallet.Balance, virtualWallet.CurrencyId, gameWallet.CurrencyId);
                    await Convert(amountInVirtualCurrency, gameWallet.Balance, gameWallet, virtualWallet);
                    amountInFiat = amountInFiat - gameWallet.Balance;
                    amountInVirtualCurrency = amountRequired - amountInVirtualCurrency;
                }
                else
                {
                    await Convert(amountInVirtualCurrency, amountInFiat, gameWallet, virtualWallet);
                    amountInFiat = 0; amountInVirtualCurrency = 0;
                }
            }
            if (incentiveWallet != null && incentiveWallet.Balance > 0 && amountInVirtualCurrency > 0)
            {
                if (amountInFiat <= incentiveWallet.Balance)
                {
                    await Convert(amountInVirtualCurrency, amountInFiat, incentiveWallet, virtualWallet);

                }
                else
                {
                    throw new Exception("While converting this error means that some major algo goof up has happened.");
                }
            }
        }

        internal async Task Convert(decimal amountInVirtualCurrency, decimal amountInFiat, Account from, Account to)
        {
            var paymentService = GetNew<PaymentService>();

            var user = _<User>(from.UserId.Value);
            //deduct from from account to global account meant for fee

            var sponsor = _<AccountSponsor>(from.AccountSponsorId.Value);
            var feeAccount = _<Account>(sponsor.PayInAccountId);
            await paymentService.Transfer(amountInFiat, Guid.NewGuid().ToString(), user.GetType().Name, user.Id,
                "Conversion", "Conversion to another currency",
                               "", "Internal", feeAccount, from);
            await AwardVirtualCurrency(to, amountInVirtualCurrency, Guid.NewGuid().ToString(), user, "VirtualCurrencyConversion", "Converted to Digital Currency");
        }

        /// <summary>
        /// Calculates how much of From currency is required for the Amount to get in "To" currency
        /// </summary>
        /// <param name="amountToGet">Amount in "To" currency</param>
        /// <param name="fromCurrencyId">Convert from</param>
        /// <param name="toCurrencyId">Convert to</param>
        /// <returns></returns>
        private decimal CalculateConversion(decimal amountToGet, int fromCurrencyId, int toCurrencyId)
        {
            var from = _<Currency>(fromCurrencyId);
            var to = _<Currency>(toCurrencyId);
            return amountToGet * from.ExchangeRate / to.ExchangeRate;
        }

    }
}
