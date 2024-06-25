using Autofac;
using Horde.Core;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Interfaces.Payment;
using Serilog;
using Horde.Core.Utilities;
using Horde.Core.Domains.World.Services;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Services;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Ecosystem.Entities;

namespace Horde.Core.Domains.Economy.Services
{
    public class PaymentService : BaseService
    {

        public PaymentService(ILifetimeScope scope) : base(scope, ContextNames.Money)
        {
        }

        public async Task AddMoneyInGlobalAccountWithoutGateway(int accountId, decimal amount)
        {
            if (amount <= 0)
                throw new Exception($"Amount cannot be {amount}");
            if (amount > 5000)
                throw new Exception($"Amount cannot be greater than 5000");
            var account = __<Account>("Currency", "AccountSponsor").
                FirstOrDefault(a => a.Id == accountId && a.Type == AccountType.Global && !a.Deleted);

            if (account == null)
                throw new Exception($"Invalid account Id {accountId}");
            if ((account?.Id ?? 0) != (account?.AccountSponsor?.PayInAccountId ?? -1))
                throw new Exception($"Invalid account Id {accountId}");

            var transactionHistory = await GetNew<PaymentService>().
                CreateOrUpdateDraftTransactionHistoryForAddingGlobalAccountMoney(account.Id, amount, "Internal");

            await GetNew<PaymentService>().AddGlobalAccountBalance(transactionHistory.Id);
        }


        public List<AccountSponsor> GetAccountSponsors(List<int> sponsorIds)
        {
            if (sponsorIds.IsNullOrEmpty())
                return new List<AccountSponsor>();

            var accountSponsors = __<AccountSponsor>("Currency").Where(a => sponsorIds.Contains(a.Id) && !a.Deleted).ToList() ?? new List<AccountSponsor>();
            var accountIds = accountSponsors.SelectMany(a => new List<int>() { a.PayInAccountId, a.PayOutAccountId }).ToList() ?? new List<int>();
            var accounts = Get<PaymentService>().GetAccounts(accountIds);

            foreach (var accountSponsor in accountSponsors)
            {
                var payinAccount = accounts.Where(a => accountSponsor.PayInAccountId == a.Id).FirstOrDefault();
                var payoutAccount = accounts.Where(a => accountSponsor.PayOutAccountId == a.Id).FirstOrDefault();
                accountSponsor.SponsoredAccounts = new List<Account>() { payinAccount, payoutAccount };
            }
            return accountSponsors;
        }


        public async Task RejectGlobalAccountAddBalance(int transactionHistoryId)
        {
            var transactionHistory = GetNew<PaymentService>().__<TransactionHistory>().FirstOrDefault(t => t.Id == transactionHistoryId);
            if (transactionHistory == null)
                throw new Exception($"Invalid transaction history id {transactionHistoryId}");
            if (transactionHistory.Status == TransactionStatusType.Rejected)
                return;
            transactionHistory.Status = TransactionStatusType.Rejected;
            await GetNew<PaymentService>().Save(transactionHistory);
        }



        public async Task<TransactionHistory> CreateOrUpdateDraftTransactionHistoryForAddingGlobalAccountMoney(int accountId, decimal amount, string mode, int transactionHistoryId = 0)
        {
            if (transactionHistoryId > 0)
            {
                var existingTransactionHistory = __<TransactionHistory>().FirstOrDefault(t => t.Id == transactionHistoryId);
                if (existingTransactionHistory == null)
                    throw new Exception($"Invalid transaction history id {transactionHistoryId}");
                if (existingTransactionHistory.Status != TransactionStatusType.Locked)
                    throw new Exception($"Transaction History Id {transactionHistoryId} already processed");
            }
            if (amount <= 0)
                throw new Exception($"Invalid amount {amount}");
            var account = __<Account>().FirstOrDefault(a => a.Id == accountId && a.Type == AccountType.Global && !a.Deleted);
            if (account == null)
                throw new Exception($"Invalid account id {accountId}");

            var transactionHistory = new TransactionHistory()
            {
                Id = transactionHistoryId,
                Key = $"addMoneyGlobalAccount_{DateTime.UtcNow.Ticks}",
                DestinationId = account.Id,
                Amount = amount,
                Type = TransactionType.Credit,
                Mode = mode,
                Narration = "Add Money In Global Account",
                Status = TransactionStatusType.Locked,
                EntityId = account.Id,
                EntityType = nameof(account),
                PaymentKey = "AddMoneyGlobalAccount",
                PartnerId = account.PartnerId
            };
            await GetNew<PaymentService>().Save(transactionHistory);
            return transactionHistory;
        }

        public async Task<TransactionHistory> AddGlobalAccountBalance(int transactionHistoryId)
        {
            var transactionHistory = __<TransactionHistory>().FirstOrDefault(t => t.Id == transactionHistoryId);
            if (transactionHistory == null)
                throw new Exception($"Invalid transaction history id {transactionHistoryId}");
            if (transactionHistory.Status != TransactionStatusType.Locked)
                throw new Exception($"Transaction History {transactionHistoryId} already been processed");
            var destinationAccountId = transactionHistory.DestinationId;
            var destinationAccount = __<Account>().FirstOrDefault(a => a.Id == destinationAccountId && a.Type == AccountType.Global);
            if (destinationAccount == null)
                throw new Exception($"Invalid destination account id {destinationAccountId} and transactionHistoryId {transactionHistoryId}");

            var oldBalance = GetAccountBalance(destinationAccountId);
            var finalAmount = GetAccountBalance(destinationAccountId) + (transactionHistory?.Amount ?? 0);
            destinationAccount.Balance = finalAmount;
            await GetNew<PaymentService>().Save(destinationAccount);

            if (GetAccountBalance(destinationAccountId) > oldBalance)
            {
                transactionHistory.Status = TransactionStatusType.Open;
                await GetNew<PaymentService>().Save(transactionHistory);
            }
            return transactionHistory;
        }

        public User GetUser(int userId)
        {
            var user = _<User>(userId);
            return user;
        }

        public User GetUserByDiscordId(string connectionKey)
        {
            var user = GetRepository(ContextNames.Ecosystem).GetNoTrackingQueryable<User>()
                .SingleOrDefault(u => u.Connections.Any(c => c.ConnectionKey == connectionKey));
            return user;
        }

        public List<(string date, decimal amount, string reason, string mode, string narration, int sourceId, int
            destinationId)> GetTransactionSummaryForAccountId(int id)
        {
            try
            {
                var results = Scope.Resolve<IRepoReader>()
                    .Get<(string date, decimal amount, string reason, string mode, string narration, int sourceId, int
                        destinationId)>
                    (@"select cast(datepart(day, CreatedAt) as nvarchar) + '-' 
                    + cast(datepart(MONTH, CreatedAt) as nvarchar) + '-' + 
                    cast(datepart(YEAR, CreatedAt) as nvarchar) AS Date, 
                    sum(amount) as Amount, PaymentKey as Reason,Mode,Narration,SourceId,DestinationId from money.Transactions 
                    where  SourceId = @id or DestinationId = @id
                    group by datepart(day, CreatedAt), PaymentKey, 
					datepart(YEAR, CreatedAt), datepart(MONTH, CreatedAt),Mode,Narration,SourceId,DestinationId,CreatedAt
					order by CreatedAt
                ", new { id })
                    .ToList();

                return results.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return null;
        }


        public override IEntityContextRepository<IEntityContext> GetRepository(ContextNames name = ContextNames.Money)
        {
            return base.GetRepository(name);
        }

        private IPaymentProvider GetProvider(Account account)
        {
            var sponsor = _<AccountSponsor>().SingleOrDefault(s => s.Id == account.AccountSponsorId);
            var provider = Scope.ResolveNamed<IPaymentProvider>(sponsor.ProviderName);
            return provider;
        }

        public async Task<AccountSponsor> CheckOrCreateAccountSponsor(int partnerId, int currencyId, int? countryId, AccountSponsorType sponsorType)
        {
            var sponsor = __<AccountSponsor>().FirstOrDefault(a => a.PartnerId == partnerId && a.CurrencyId == currencyId && a.Type == sponsorType);
            if (sponsor != null)
                return sponsor;

            var partner = __<Tenant>(partnerId);

            if (partner == null)
                throw new Exception($"Partner not found id {partnerId}");

            var currency = __<Currency>().FirstOrDefault(c => c.Id == currencyId);

            if (currency == null)
                throw new Exception($"Currency not found in partner {partnerId} and currencyId {currencyId}");
            if (currency.Type == CurrencyNatureType.DigitalCurrency && currency.PartnerId != partnerId)
                throw new Exception($"Currency not found in partner {partnerId} and currencyId {currencyId}");

            var accounts = new List<Account>();
            var payinAccount = new Account()
            {
                Balance = 0.0M,
                CurrencyId = currencyId,
                Name = "Pay-in " + currency.ShortName + " Account",
                Type = AccountType.Global,
                Purpose = sponsorType.ToString() + " payin",
                UserId = null,
                PartnerId = partnerId
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
                PartnerId = partnerId
            };
            accounts.Add(payoutAccount);

            await GetNew<PaymentService>().Save(accounts);

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
                CountryId = countryId,
                PartnerId = partnerId
            };
            await GetNew<PaymentService>().Save(sponsor);

            foreach (var account in accounts)
                account.AccountSponsorId = sponsor.Id;

            await GetNew<PaymentService>().Save(accounts);

            return sponsor;
        }

        public BasePayoutInfo GetAccountPayoutInfo(Account account)
        {
            var provider = GetProvider(account);
            return provider.GetPayoutInfo();
        }

        public List<GatewayPayout> GetPayoutsForUser(int userId)
        {
            var payouts = _<GatewayPayout>().Where(gp => gp.UserId == userId)
                .OrderByDescending(gp => gp.Id).ToList();
            var keys = payouts.Select(p => p.Key).ToList();
            var transactions = _<Transaction>().Where(t => keys.Contains(t.Key));
            payouts.ForEach(p => p.Transaction = transactions.FirstOrDefault(t => t.Key == p.Key));
            return payouts;
        }

        public async Task<List<Account>> GetAccountsForUser(int userId)
        {
            var partnerSponsors = _<AccountSponsor>().Where(p => p.PartnerId == Partner.Id &&
            p.Deleted == false && p.Type != AccountSponsorType.Private).ToList() ?? new List<AccountSponsor>();

            var accounts = _<Account>().Where(a => a.UserId == userId && !a.Deleted).ToList() ?? new List<Account>();

            var leftSponsors = partnerSponsors.Where(p => accounts.Select(a => a.AccountSponsorId).
            Contains(p.Id) == false).ToList() ?? new List<AccountSponsor>();

            foreach (var sponsor in leftSponsors)
            {
                await GetUserAccount(userId, sponsor.Id);
            }
            var accountIds = _<Account>().Where(a => a.UserId == userId && !a.Deleted).Select(a => a.Id).ToList() ?? new List<int>();
            return GetAccounts(accountIds);
        }


        public async Task<Account> GetGatewayAccount(int partnerId, int currencyId, string gatewayName, AccountType type = AccountType.GatewayInput)
        {
            if (type != AccountType.GatewayInput && type != AccountType.GatewayOutput)
                throw new Exception("Account type should be Gateway");
            gatewayName = gatewayName?.Trim()?.ToLower() ?? "";
            if (string.IsNullOrEmpty(gatewayName))
                throw new Exception($"Gateway name cannot be empty");
            var account = __<Account>().FirstOrDefault(a => a.CurrencyId == currencyId && a.PartnerId == partnerId
            && a.Key == gatewayName && a.Type == type);
            if (account != null)
                return account;

            var partner = __<Tenant>(partnerId);
            if (partner == null)
                throw new Exception($"Partner not found {partnerId}");
            var currency = __<Currency>().FirstOrDefault(c => c.Id == currencyId && c.Type == CurrencyNatureType.Fiat);
            if (currency == null)
                throw new Exception($"Currency not found for Id {currencyId}");
            var accountSponsor = await CheckOrCreateAccountSponsor(partnerId, currencyId, null, AccountSponsorType.Private);
            if (accountSponsor == null)
                throw new Exception($"Account sponsor not found");

            account = new Account()
            {
                UserId = null,
                PartnerId = partnerId,
                Type = type,
                Key = gatewayName,
                Name = $"{gatewayName} {type} Account",
                AccountSponsorId = accountSponsor.Id,
                CurrencyId = currencyId
            };
            await GetNew<PaymentService>().Save(account);
            return account;
        }

        public async Task<Account> GetUserAccount(int userId, int sponsorId, AccountType type = AccountType.User)
        {
            var account = _<Account>()
                .FirstOrDefault(a => a.UserId == userId && a.AccountSponsorId == sponsorId && a.Type == type);

            if (account != null)
                return account;

            var sponsor = _<AccountSponsor>("Currency").FirstOrDefault(s => s.Id == sponsorId);
            if (sponsor == null)
                throw new Exception($"Account sponsor not found with id {sponsorId}");

            var user = _<User>().FirstOrDefault(u => u.Id == userId);
            var currencyId = sponsor.CurrencyId;
            var currency = sponsor.Currency;
            
            if (user == null || (user?.PartnerId ?? 0) <= 0)
                throw new Exception("User not found");
            if (sponsor == null || (sponsor?.PartnerId ?? 0) <= 0)
                throw new Exception("Sponsor not found");

            account = new Account()
            {
                UserId = user.Id,
                Type = type,
                Name = $"{user.Username} {currency.ShortName} {type} account, sponsored by {sponsor.Key}",
                AccountSponsorId = sponsorId,
                CurrencyId = currencyId
            };
            await Save(account);


            return account;
        }


        public async Task<Adjustment> CreateAdjustment(Account account, decimal amount, string reason)
        {
            var adjustment = new Adjustment()
            { AccountId = account.Id, Amount = amount, Narration = reason };
            using (var child = Scope.BeginLifetimeScope())
            {
                var payment = child.Resolve<PaymentService>();
                var sourceAccount = GetGlobalAccount("GlobalAdjustmentSource", 1);

                payment.EnsureSourceAccountBalance(sourceAccount, amount);
                var repo = payment.GetRepository();

                repo.Upsert(adjustment);
                await repo.SaveChanges();
                var user = payment.GetRepository(ContextNames.Ecosystem).GetNoTrackingQueryable<User>()
                    .SingleOrDefault(u => u.Id == account.UserId);
                var sponsor = _<AccountSponsor>().SingleOrDefault(s => s.Id == account.AccountSponsorId);
                await payment.TransferAmountFromGlobalAccount(user, amount, $"Adjustment_{adjustment.Id}",
                    nameof(Adjustment), adjustment.Id, "OneTime", sponsor, reason);
            }

            return adjustment;
        }

        private void EnsureSourceAccountBalance(Account sourceAccount, decimal amount)
        {
            var balance = GetAccountBalance(sourceAccount.Id);
            if (balance < amount)
                throw new Exception("Not enough balance in the account " + sourceAccount.Name);
        }

        public decimal GetAccountBalance(int accountId)
        {

            string sql = $@"
            DECLARE @type INT;
            DECLARE @currencyType INT;
            DECLARE @balance DECIMAL(18, 2);

            SELECT @type = [type], @balance = Balance, @currencyType = (SELECT [Type] FROM [money].Currency 
WHERE id = CurrencyId) FROM [money].Accounts WHERE id = {accountId};

            IF @type = 4
            BEGIN
                SELECT @balance = a.outer_bal - b.inner_bal FROM 
                (SELECT ISNULL(SUM(amount), 0) AS outer_bal FROM money.gatewaypayins WHERE gatewayinputaccountid = {accountId} AND [status] = 2) a
                INNER JOIN
                (SELECT ISNULL(SUM(amount), 0) AS inner_bal FROM money.transactions WHERE sourceId = {accountId} AND Locked <> 2) b ON 1 = 1;
            END
            ELSE IF @type <> 0 OR (@type = 0 AND @currencyType = 0)
            BEGIN
                SELECT @balance = a.outer_bal - b.inner_bal FROM 
                (SELECT ISNULL(SUM(amount), 0) AS outer_bal FROM money.transactions WHERE destinationId = {accountId} AND Locked = 0) a
                INNER JOIN
                (SELECT ISNULL(SUM(amount), 0) AS inner_bal FROM money.transactions WHERE sourceId = {accountId} AND Locked <> 2) b ON 1 = 1;
            END

            SELECT @balance AS Balance;";

            /* var balance = Reader.Get<decimal>
                 ("select [money].GetAccountBalance(@id)", new { id = accountId }).FirstOrDefault();*/

            var balance = Reader.Get<decimal>
                (sql, new { id = accountId }).FirstOrDefault();

            return balance;
        }

        public Transaction GetUserTransaction(int userId, int? transactionId = null, string transactionKey = null)
        {
            if (userId <= 0)
                return null;
            Transaction transaction = null;
            if (transactionId != null && transactionId > 0)
                transaction = _<Transaction>()
                    .FirstOrDefault(t => !t.Deleted && t.Id == transactionId);
            if (transaction == null && !string.IsNullOrEmpty(transactionKey))
                transaction = _<Transaction>()
                    .FirstOrDefault(t => !t.Deleted && t.Key == transactionKey);
            if (transaction != null)
            {
                GetTransactionsByIds(new List<int>() { transaction.Id }, userId);
            }

            return transaction;
        }

        public Transaction GetOrCreateTransaction(Account source = null, Account destination = null,
            decimal? amount = -1, string key = "", string mode = "", string gatewayAccount = "", string entityType = "",
            int entityId = 0, string paymentKey = "", string narration = "", LockType lockType = LockType.Open)
        {

            if (string.IsNullOrEmpty(key))
                throw new TransactionException("Transaction key is non nullable", key);
            Transaction transaction = null;
            try
            {
                transaction = _<Transaction>("Source", "Destination").FirstOrDefault(t => t.Key == key);
                if (transaction == null)
                {
                    if (amount <= 0)
                        throw new TransactionException(
                            "Transaction amount cannot be less than = 0", key);
                    if (source == null || destination == null || amount == null)
                        throw new TransactionException(
                            "Cannot create a new transaction, invalid values, transactionkey: ", key);
                    if (source.AccountSponsorId != destination.AccountSponsorId && source.Type != AccountType.GatewayInput)
                        throw new TransactionException(
                      $"Account sponsor of source {source.Id} and destination {destination.Id} doesn't match", key);
                    if (source.PartnerId != destination.PartnerId)
                        throw new TransactionException(
                      $"Partner of source {source.Id} and destination {destination.Id} doesn't match", key);
                    if (source.PartnerId <= 0)
                        throw new TransactionException($"Partner of source {source.Id} is invalid", key);


                    transaction = new Transaction()
                    {
                        Amount = amount.GetValueOrDefault(),
                        Key = key,
                        SourceId = source.Id,
                        DestinationId = destination.Id,
                        Narration = narration,
                        PaymentAccount = gatewayAccount,
                        EntityType = entityType,
                        EntityId = entityId,
                        PaymentKey = paymentKey,
                        Mode = mode,
                        PartnerId = source.PartnerId,
                        UniqueTransactionKey = key,
                        Locked = lockType
                    };
                }
            }
            catch (Exception ex)
            {
                throw new TransactionException(ex.Message, key);
            }

            return transaction;
        }


        public async Task<Transaction> GratifyTransaction(int transactionId)
        {
            var transaction = __<Transaction>().Where(t => t.Id == transactionId).FirstOrDefault();
            if (transaction == null)
                throw new Exception($"Transaction {transactionId} not found");
            if (transaction.Locked == LockType.Locked)
                transaction = await UpdateTransactionLockStatus(transaction.Key, LockType.Open);
            if (transaction == null || transaction.Locked != LockType.Open)
                throw new Exception($"Unable to gratify transaction {transactionId}");
            var sourceAccount = __<Account>().FirstOrDefault(a => a.Id == transaction.DestinationId);
            if (sourceAccount == null)
                throw new Exception($"No destination account found for {transactionId}");
            var transactionKey = $"TransactionGratification_{transaction.Key}";
            await GratifyAccount(sourceAccount, transaction.Amount, transaction, transactionKey,
                "TransactionGratification", $"Gratification {transaction.Narration}");
            var gratifiedTransaction = __<Transaction>().FirstOrDefault(t => t.Key == transactionKey);
            return gratifiedTransaction;
        }


        public async Task GratifyAccount(Account source, decimal amount, BaseEntity entity, string key,
        string paymentKey, string purpose)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source.Type == AccountType.Global)
                throw new Exception($"Gratification not allowed on global account {source.Id}");
            var paymentService = GetNew<PaymentService>();
            var destination = __<Account>().Where(a => a.AccountSponsorId == source.AccountSponsorId &&
            a.Id == a.AccountSponsor.PayOutAccountId && !a.Deleted).FirstOrDefault();
            if (destination == null)
                throw new Exception($"No destination global account found for account {source.Id}");

            await paymentService.Transfer(amount, key: key,
                entityType: entity.GetType().Name, entityKey: entity.Id,
                paymentKey: paymentKey, purpose: purpose, gatewayAccount: "",
                mode: "Gratification", destination, source);
        }


        /// <summary>
        /// Sends cash to user's account from sponsor's account
        /// </summary>
        /// <param name="account">Which account to withdraw from</param>
        /// <param name="mode">In case gateway supports multiple modes, which mode to choose (wallet/bank/upi)</param>
        /// <param name="amount">Money to send</param>
        /// <param name="gratificationAccount">What is the user account if it exists</param>
        /// <param name="narration">Any narration</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Transaction> GratifyUser(Account account, string? mode,
            decimal? amount, string gratificationAccount, string narration = "",
            string emailId = "", string name = "", string vpaAddress = "", string bankName = "")
        {
            if (account.UserId == null)
                throw new Exception("This is not a valid user account");
            if (account.IsFrozen)
                throw new Exception("Your account is frozed");
            if (GetAccountBalance(account.Id) < amount)
                throw new Exception("Not enough balance in the account");
            var money = GetRepository();
            var sponsor = _<AccountSponsor>().SingleOrDefault(s => s.Id == account.AccountSponsorId);
            var provider = Scope.ResolveNamed<IPaymentProvider>(sponsor.ProviderName);
            var providerAccount = _<Account>().Single(a => a.Id == sponsor.PayOutAccountId);
            var transaction = GetOrCreateTransaction(source: account, destination: providerAccount,
                amount: amount, key: DateTime.UtcNow.Ticks.ToString()
                , mode: mode, gatewayAccount: gratificationAccount);
            transaction.Source = account;
            transaction.Properties = new Dictionary<string, string>()
            {
                { "EmailId", emailId },
                { "Name", name },
                { "VpaAddress", vpaAddress },
                { "BankName", bankName }
            };
            GatewayPayout payout = null;
            try
            {
                payout = await provider.SendPayment(transaction);
                if (payout == null)
                {
                    throw new Exception("Gateway payout catastrophic failure. Please try later");
                }
                else if (payout.Status == PayoutStatusType.Failed)
                {
                    transaction.Narration = payout.Result.GetNarration();
                    throw new Exception("Payout failed. " + transaction.Narration);
                }

                transaction.Narration = payout.Result.GetNarration();

                await DoTransaction(transaction.Amount, transaction.Key, nameof(GatewayPayout),
                    payout.Id, "PaymentCreated",
                    transaction.Narration, gratificationAccount, mode, providerAccount, account);
                return transaction;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Payout failed");
                var log = TransactionExceptionLog.From(transaction);
                money.Upsert(log);
                await money.SaveChanges();
                throw;
            }
        }


        public Account GetGlobalAccount(string purpose, int currencyId)
        {
            var account = _<Account>().FirstOrDefault(a => a.CurrencyId == currencyId
                && a.Type == AccountType.Global && a.Purpose == purpose);
            return account;
        }

        public async Task<Transaction> TransferAmountFromGlobalAccount(User user, decimal balance, string key,
            string entityType, int entityKey, string paymentKey,
            AccountSponsor sponsor,
            string purpose = "", int currencyId = 1, Account source = null)
        {
            var destination = await GetUserAccount(user.Id, sponsorId: sponsor.Id);
            if (source == null)
                source = _<Account>(sponsor.PayInAccountId);
            var accountBalance = GetAccountBalance(source.Id);
            if (accountBalance <= balance)
                throw new Exception($"Insufficient fund in the global payout account");
            var transaction = await DoTransaction(balance, key, entityType, entityKey, paymentKey, purpose, "", "Internal",
                destination, source);
            return transaction;
        }

        public async Task<Transaction> Transfer(decimal amount, string key, string entityType,
            int entityKey, string paymentKey, string purpose, string gatewayAccount,
            string mode,
            Account destination, Account source, LockType transactionLock = LockType.Open)
        {
            if (GetAccountBalance(source.Id) < amount)
                throw new Exception($"Account balance is less than {amount}");


            return await DoTransaction(amount, key, entityType, entityKey, paymentKey, purpose, gatewayAccount,
                mode, destination, source, lockType: transactionLock);
        }

        public async Task ReverseTransaction(string transactionKey, string reason)
        {
            try
            {
                var transaction = Get<PaymentService>().GetOrCreateTransaction(key: transactionKey);
                await DoTransaction(transaction.Amount, "reverse" + transaction.Key,
                    transaction.EntityType, 0, reason + "_" + transaction.Narration, reason, "", "Internal",
                    new Account { Id = transaction.SourceId }, new Account { Id = transaction.DestinationId });
            }
            catch (Exception e)
            {
                throw new Exception("Unable to process reverse transaction :" + transactionKey + ": " + e.ToString());
            }
        }

        //    private async Task<Transaction> DoTransaction(decimal amount, string key, string entityType,
        //int entityKey, string paymentKey, string purpose, string gatewayAccount,
        //string mode,
        //Account destination, Account source, LockType lockType = LockType.Open)
        //    {
        //        Transaction transaction = null;
        //        try
        //        {
        //            transaction = GetOrCreateTransaction(source, destination, amount, key, mode: mode,
        //                gatewayAccount: gatewayAccount, entityType, entityKey, paymentKey, purpose, lockType);
        //            if (transaction?.Id > 0)
        //            {
        //                Log.Warning("Could not create transaction to transfer {0} from {1} to {2}. Already exists", amount,
        //                    source.Name, destination.Name);
        //                throw new TransactionException("Transaction already exists", key);
        //                //return transaction;
        //            }
        //            if (source.Id == destination.Id)
        //                throw new Exception("Source and destination account cannot be same");

        //            // Convert the stored procedure to an inline SQL string
        //            // SQL string for the transaction
        //            /*string sql = @"
        //    DECLARE @type INT;
        //    DECLARE @currencyType INT;
        //    DECLARE @balance DECIMAL(18, 2);
        //    DECLARE @output INT;

        //    -- Get account type, balance, and currency type
        //    SELECT @type = [type], @balance = Balance, 
        //           @currencyType = (SELECT [Type] FROM [money].Currency WHERE id = CurrencyId)
        //    FROM [money].Accounts WHERE id = @SourceId;

        //    -- Calculate the balance based on account type and currency type
        //    IF @type = 4
        //    BEGIN
        //        SELECT @balance = a.outer_bal - b.inner_bal 
        //        FROM 
        //            (SELECT ISNULL(SUM(amount), 0) AS outer_bal FROM money.gatewaypayins WHERE gatewayinputaccountid = @SourceId AND [status] = 2) a,
        //            (SELECT ISNULL(SUM(amount), 0) AS inner_bal FROM money.transactions WHERE sourceId = @SourceId AND Locked <> 2) b;
        //    END
        //    ELSE IF @type <> 0 OR (@type = 0 AND @currencyType = 0)
        //    BEGIN
        //        SELECT @balance = a.outer_bal - b.inner_bal 
        //        FROM 
        //            (SELECT ISNULL(SUM(amount), 0) AS outer_bal FROM money.transactions WHERE destinationId = @SourceId AND Locked = 0) a,
        //            (SELECT ISNULL(SUM(amount), 0) AS inner_bal FROM money.transactions WHERE sourceId = @SourceId AND Locked <> 2) b;
        //    END

        //    -- Perform the transaction if the balance is sufficient
        //    IF @balance >= @Amount
        //    BEGIN
        //        BEGIN TRANSACTION;
        //        BEGIN TRY
        //            INSERT INTO money.transactions (
        //                [sourceId], [destinationId], [amount], [narration], 
        //                [CreatedAt], [ModifiedAt], [key], [Deleted], 
        //                [paymentAccount], [entityId], [entityType], [paymentKey], 
        //                [Mode], Locked, [PartnerId]
        //            ) 
        //            VALUES (
        //                @SourceId, @DestinationId, @Amount, @Narration, 
        //                GETDATE(), GETDATE(), @Key, 0, 
        //                @PaymentAccount, @EntityId, @EntityType, @PaymentKey, 
        //                @Mode, @Locked, @PartnerId
        //            );

        //            UPDATE [money].Accounts 
        //            SET balance = balance - @Amount 
        //            WHERE id = @SourceId AND [type] = 0;

        //            UPDATE [money].Accounts 
        //            SET balance = balance + @Amount 
        //            WHERE id = @DestinationId AND [type] = 0;

        //            SET @output = 1;
        //            COMMIT TRANSACTION;
        //        END TRY
        //        BEGIN CATCH
        //            ROLLBACK TRANSACTION;
        //            SET @output = -2;
        //            THROW;
        //        END CATCH
        //    END
        //    ELSE
        //    BEGIN
        //        SET @output = -1;
        //    END

        //    SELECT @output;";*/

        //            var parameters = new
        //            {
        //                sourceId = transaction.SourceId,
        //                destinationId = transaction.DestinationId,
        //                amount = transaction.Amount,
        //                narration = transaction.Narration,
        //                key = transaction.Key,
        //                paymentAccount = transaction.PaymentAccount,
        //                entityId = transaction.EntityId,
        //                entityType = transaction.EntityType,
        //                paymentKey = transaction.PaymentKey,
        //                mode = transaction.Mode,
        //                partnerId = transaction.PartnerId,
        //                locked = transaction.Locked
        //            };

        //            var output = Writer.Execute("upsert_transaction_test", parameters);
        //            if (output == 1)
        //            {
        //                Log.Information("Successfully transferred {0} from {1} to {2}", amount, source.Name, destination.Name);
        //                transaction.FireEvent(EntityEventType.Created, secondaryEventName: TribeTopics.TransactionMade.ToString(), data:
        //                    new Dictionary<string, string> { { "Amount", amount.ToString() } });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex, "Could not transfer {0} from {1} to {2}", amount, source.Name, destination.Name);
        //            await AddTransactionExceptionLog(transaction, ex);
        //            throw;
        //        }

        //        return transaction;
        //    }

        private async Task<Transaction> DoTransaction(decimal amount, string key, string entityType,
            int entityKey, string paymentKey, string purpose, string gatewayAccount,
            string mode,
            Account destination, Account source, LockType lockType = LockType.Open)
        {
            Transaction transaction = null;
            try
            {
                transaction = GetOrCreateTransaction(source, destination, amount, key, mode: mode,
                    gatewayAccount: gatewayAccount, entityType, entityKey, paymentKey, purpose, lockType);
                if (transaction?.Id > 0)
                {
                    Log.Error("Could not create transaction to transfer {0} from {1} to {2}. Already exists", amount,
                        source.Name, destination.Name);
                    throw new TransactionException("Transaction with this unique key already exists", key);
                    //return transaction;
                }
                if (source.Id == destination.Id)
                    throw new Exception("Source and destination account cannot be same");


                var output = Writer.ExecuteProcedureWithResult<int>("upsert_transaction_test", new
                {
                    sourceId = transaction.SourceId,
                    destinationId = transaction.DestinationId,
                    amount = transaction.Amount,
                    narration = transaction.Narration,
                    key = transaction.Key,
                    paymentAccount = transaction.PaymentAccount,
                    entityId = transaction.EntityId,
                    entityType = transaction.EntityType,
                    paymentKey = transaction.PaymentKey,
                    mode = transaction.Mode,
                    partnerId = transaction.PartnerId,
                    locked = transaction.Locked,
                }).FirstOrDefault();
                if (output == 1)
                    Log.Information("Successfully transfered {0} from {1} to {2}", amount, source.Name,
                        destination.Name);
                transaction.FireEvent(EntityEventType.Created, secondaryEventName: TribeTopics.TransactionMade.ToString(), data:
               new() { { "Amount", amount.ToString() } });

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not transfer {0} from {1} to {2}", amount, source.Name, destination.Name);
                //await AddTransactionExceptionLog(transaction, ex);
                throw;
            }
            transaction = _<Transaction>().SingleOrDefault(t => t.SourceId == transaction.SourceId
            && t.DestinationId == transaction.DestinationId && t.UniqueTransactionKey == key);
            return transaction;
        }

        public List<Account> GetAccounts(List<int> accountIds)
        {
            if (accountIds.IsNullOrEmpty())
                return new List<Account>();
            var accounts = __<Account>("AccountSponsor", "Currency").Where(a => accountIds.Contains(a.Id))
          .ToList() ?? new List<Account>();

            foreach (var account in accounts)
            {
                account.Balance = GetAccountBalance(account.Id);
                account.LockedBalance = GetLockedBalance(account.Id);
            }
            return accounts;
        }

        private async Task AddTransactionExceptionLog(Transaction transaction, Exception ex)
        {
            var repo = GetRepository();
            var log = TransactionExceptionLog.From(transaction, ex);
            log.Exception = ex.Message;
            log.StackTrace = ex.StackTrace;
            repo.Upsert(log);
            await repo.SaveChanges();
        }

        public async Task<Account> GetPartnerAccount(int userId, int currencyId)
        {
            var account = _<Account>().FirstOrDefault(a => a.CurrencyId == currencyId && a.UserId == userId
                && a.Type == AccountType.Partner);
            if (account != null)
                return account;
            var currency = _<Currency>(currencyId);
            account = new Account()
            {
                CurrencyId = currencyId,
                Name = $"Partner account",
                Purpose = "To fund campaigns",
                UserId = userId,
                Type = AccountType.Partner,
            };
            await Save(account);
            return account;
        }


        public async Task FundPartnerAccount(Account fundingAccount, AccountSponsor sponsor, decimal amount,
            string paymentAccount)
        {
            if (fundingAccount.Type != AccountType.Partner)
                throw new Warning("Only partner accounts can be funded in this method");
            var source = _<Account>(sponsor.PayInAccountId);
            if (source.Balance <= amount)
                throw new Warning($"Not sufficient fund in the payin account");
            await DoTransaction(amount, Guid.NewGuid().ToString(),
                "", 0, "PartnerFundAddition", "Added by partner",
                paymentAccount, "Internal",
                fundingAccount, source);

        }

        public async Task<Account> GetUserAccount(User owner, int currencyId, int countryId, AccountSponsorType sponsorType)
        {
            var sponsor = GetAccountSponsor(currencyId, countryId, sponsorType);
            var account = _<Account>("Currency")
                .FirstOrDefault(a =>
                    a.CurrencyId == currencyId && a.UserId == owner.Id && a.AccountSponsorId == sponsor.Id);

            if (account == null)
            {
                var currency = Get<CurrencyService>().GetCurrency(currencyId);
                account = new Account()
                {

                    UserId = owner.Id,
                    Type = AccountType.User,
                    Name = $"{owner.Username} {currency?.Name} account, sponsored by {sponsor.Key}",
                    AccountSponsorId = sponsor.Id,
                    CurrencyId = currencyId,
                    Purpose = "Receive payments for various activities",

                };
                await Save(account);
            }

            return account;
        }

        public AccountSponsor GetSponsor(int accountSponsorId)
        {
            return _<AccountSponsor>(accountSponsorId);
        }

        private AccountSponsor GetAccountSponsor(int currencyId, int countryId, AccountSponsorType sponsorType)
        {
            return _<AccountSponsor>("Currency").FirstOrDefault(a => a.CurrencyId == currencyId
            && a.CountryId == countryId && a.Type == sponsorType);
        }


        public async Task<decimal> GetUserTrinketBalanceAsync(int userId)
        {
            var accounts = await GetAccountsForUser(userId);
            var trinketAccounts = accounts.Where(t => t.CurrencyId == 2).ToList();
            var balance = 0.0M;
            foreach (var account in trinketAccounts)
            {
                balance += GetAccountBalance(account.Id);
            }
            return balance;
        }

        public AccountSponsor GetSponsorByType(AccountSponsorType type, int currencyId)
        {
            if (currencyId <= 0) currencyId = 1;
            return _<AccountSponsor>().FirstOrDefault(a => a.Type == type && a.CurrencyId == currencyId);
        }

        public Account GetGlobalAccountBySponsorType(int currencyId, AccountSponsorType type)
        {
            if (currencyId <= 0) currencyId = 1;
            var sponsor = GetSponsorByType(type, currencyId);

            return _<Account>().FirstOrDefault(a => a.Type == AccountType.Global && a.AccountSponsorId == sponsor.Id && a.CurrencyId == currencyId);
        }

        public Account GetPayinAccountBySponsorType(int currencyId, AccountSponsorType type)
        {
            if (currencyId <= 0) currencyId = 1;
            var sponsor = GetSponsorByType(type, currencyId);

            return _<Account>(sponsor.PayInAccountId);
        }

        public Account GetPayoutAccountBySponsorType(int currencyId, AccountSponsorType type)
        {
            if (currencyId <= 0) currencyId = 1;
            var sponsor = GetSponsorByType(type, currencyId);


            return _<Account>(sponsor.PayOutAccountId);
        }



        public async Task<Transaction> UpdateTransactionLockStatus(string transactionKey, LockType lockType, string narration = null)
        {
            if (string.IsNullOrEmpty(transactionKey))
                throw new Exception("Transaction key is null");
            var tran = __<Transaction>().FirstOrDefault(t => t.Key == transactionKey);
            if (tran == null)
                throw new Exception($"Transaction with key {transactionKey} not found");
            if (narration != null)
                tran.Narration = narration;
            tran.Locked = lockType;
            await Save(tran);
            return tran;
        }

        public decimal GetLockedBalance(int accountId)
        {
            return Reader.Get<decimal>("select isnull(sum(amount),0) as amount from economy.Transactions where DestinationId = @accountId and locked = 1",
                new { accountId }).FirstOrDefault();
        }



        public List<TransactionHistory> GetTransactionHistoryByIds(List<int> transactionHistoryIds, int? userId = null)
        {
            if (transactionHistoryIds.IsNullOrEmpty())
                return new List<TransactionHistory>();

            var transactionHistorys = __<TransactionHistory>("Source", "Destination").Where(t => !t.Deleted &&
            transactionHistoryIds.Contains(t.Id)).OrderByDescending(t => t.ModifiedAt).ToList() ?? new List<TransactionHistory>();

            var sourceAccounts = transactionHistorys.Select(t => t.Source).ToList() ?? new List<Account>();
            var destinationAccounts = transactionHistorys.Select(t => t.Destination).ToList() ?? new List<Account>();
            var accounts = sourceAccounts.Union(destinationAccounts).ToList() ?? new List<Account>();
            var userIds = accounts.Where(a => (a?.UserId ?? 0) > 0).Select(a => a.UserId).Distinct().ToList() ?? new List<int?>();
            var users = __<User>().Where(u => userIds.Contains(u.Id)).ToList() ?? new List<User>();
            var currencies = __<Currency>().ToList() ?? new List<Currency>();

            if (!transactionHistorys.IsNullOrEmpty())
            {

                foreach (var transactionHistory in transactionHistorys)
                {
                    transactionHistory.Type = TransactionType.Credit;

                    var payoutStatus = PayoutStatusType.Succeeded;

                    if (transactionHistory.Status == TransactionStatusType.Locked)
                        payoutStatus = PayoutStatusType.Pending;

                    if (transactionHistory.Status == TransactionStatusType.Rejected)
                        payoutStatus = PayoutStatusType.Canceled;
                    if (transactionHistory.Source != null)
                    {
                        transactionHistory.Source.User = users.FirstOrDefault(u => u.Id == transactionHistory.Source.UserId);
                        transactionHistory.Source.Currency = currencies.FirstOrDefault(c => c.Id == transactionHistory.Source.CurrencyId);
                    }
                    if (transactionHistory.Destination != null)
                    {
                        transactionHistory.Destination.Currency = currencies.FirstOrDefault(c => c.Id == transactionHistory.Destination.CurrencyId);
                        transactionHistory.Destination.User = users.FirstOrDefault(u => u.Id == transactionHistory.Destination.UserId);
                    }

                    var gatewayPayout = new GatewayPayout()
                    {
                        Type = PayoutType.Internal,
                        Status = payoutStatus,
                        BeneficiaryAccount = transactionHistory?.Destination?.User?.Username ?? "",
                        GatewayName = "Internal",
                        Narration = transactionHistory?.Narration ?? $"Transaction {payoutStatus}"
                    };

                    if (transactionHistory != null)
                    {
                        transactionHistory.Payout = gatewayPayout;
                        transactionHistory.Narration = gatewayPayout.Narration;
                    }
                }
            }
            return transactionHistorys;
        }



        public List<Transaction> GetTransactionsByIds(List<int> transactionIds, int? userId = null)
        {
            if (transactionIds.IsNullOrEmpty())
                return new List<Transaction>();

            var transactions = __<Transaction>("Source", "Destination").Where(t => !t.Deleted && transactionIds.Contains(t.Id)).OrderByDescending(t => t.ModifiedAt).ToList() ?? new List<Transaction>();
            var sourceAccounts = transactions.Select(t => t.Source).ToList() ?? new List<Account>();
            var destinationAccounts = transactions.Select(t => t.Destination).ToList() ?? new List<Account>();
            var accounts = sourceAccounts.Union(destinationAccounts).ToList() ?? new List<Account>();
            var userIds = accounts.Select(a => a.UserId).Distinct().ToList();
            var users = __<User>().Where(u => userIds.Contains(u.Id)).ToList() ?? new List<User>();
            var currencies = __<Currency>().ToList() ?? new List<Currency>();

            if (!transactions.IsNullOrEmpty())
            {
                var transactionKeys = transactions.Select(t => t.Key).ToList();
                var gatewayPayouts = __<GatewayPayout>().Where(g => !g.Deleted && transactionKeys.Contains(g.Key)).ToList();

                foreach (var transaction in transactions)
                {
                    transaction.Type = TransactionType.Credit;

                    if (transaction.Source?.UserId == userId)
                        transaction.Type = TransactionType.Debit;

                    var payoutStatus = PayoutStatusType.Succeeded;

                    if (transaction.Locked == LockType.Locked)
                        payoutStatus = PayoutStatusType.Pending;

                    if (transaction.Locked == LockType.Rejected)
                        payoutStatus = PayoutStatusType.Canceled;

                    transaction.Source.User = users.FirstOrDefault(u => u.Id == transaction.Source.UserId);
                    transaction.Source.Currency = currencies.FirstOrDefault(c => c.Id == transaction.Source.CurrencyId);
                    transaction.Destination.Currency = currencies.FirstOrDefault(c => c.Id == transaction.Destination.CurrencyId);
                    transaction.Destination.User = users.FirstOrDefault(u => u.Id == transaction.Destination.UserId);
                    var gatewayPayout = gatewayPayouts.FirstOrDefault(g => g.Key == transaction.Key);

                    if (gatewayPayout == null)
                    {
                        gatewayPayout ??= new GatewayPayout()
                        {
                            Type = PayoutType.Internal,
                            Status = payoutStatus,
                            BeneficiaryAccount = transaction?.Destination?.User?.Username ?? "",
                            GatewayName = "Internal",
                            Narration = transaction?.Narration ?? $"Transaction {payoutStatus}"
                        };
                    }

                    if (transaction != null)
                    {
                        transaction.Payout = gatewayPayout;
                        transaction.Narration = gatewayPayout.Narration;
                    }
                }
            }
            return transactions;
        }



        
    }
}

public class PayoutTransactionStatusModel
{
    public PayoutStatusType PayoutStatus { get; set; }
    public TransactionStatusType TransactionStatus { get; set; }
}