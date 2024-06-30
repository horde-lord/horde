using Autofac;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Microsoft.VisualBasic;
using System.Globalization;
using Horde.Core.Domains.Economy.Entities;

namespace Horde.Core.Domains.Economy.Services
{
    public class ActivityService : BaseService
    {
        public ActivityService(ILifetimeScope scope) : base(scope, ContextNames.Economy)
        {
        }

        public List<ActivityLog> GetUserActivities(int userId)
        {
            return _<ActivityLog>("Transaction", "Activity").Where(a => a.UserId == userId).ToList();
        }

        public async Task<ActivityLog> TriggerActivity(int id, User user, string? narration, string? uniqueKey, decimal? amount = null)
        {
            var activity = _<Activity>(id);
            try
            {
                if (activity == null)
                    throw new ArgumentException($"Activity {id} not matched with configured activities");
                string activityKey = GetActivityKey(activity, user.Id, uniqueKey);
                if (narration == null)
                {
                    narration = $"Activity triggered for user {user.RealName}";
                }
                if(activity.RequiresUniqueKey && uniqueKey == null)
                {
                    throw new ArgumentException("This activity requires a unique key to be passed as a parameter");
                }
                if (amount == null)
                    amount = activity.DefaultAmountPerTransaction;
                if (amount < activity.MinAmountPerTransaction)
                    throw new ArgumentException($"You cannot transfer less than " +
                        $"{activity.MinAmountPerTransaction} for activity {activity.Name} ");
                if (amount > activity.MaxAmountPerTransaction)
                    throw new ArgumentException($"You cannot transfer more than " +
                        $"{activity.MaxAmountPerTransaction} for activity {activity.Name} ");

                var sponsor = _<AccountSponsor>().FirstOrDefault(a => a.CurrencyId == activity.CurrencyId
                && a.Type == AccountSponsorType.Marketing);
                var transaction = await Get<PaymentService>().TransferAmountFromGlobalAccount(user, amount.Value, activityKey, nameof(Activity),
                    activity.Id, $"ActivityTriggered", sponsor, purpose: narration, currencyId: activity.CurrencyId);
                return await LogActivity(activity, transaction, user, ActivityStatusType.Completed);

            }
            catch (Exception ex)
            {
                return await LogActivity(activity, null, user, ActivityStatusType.Failed, ex);
            }



        }

        private async Task<ActivityLog> LogActivity(Activity activity, Transaction transaction,
            User user, ActivityStatusType status, Exception ex = null)
        {
            // Your code logic here
            var log = new ActivityLog()
            {
                TransactionId = transaction?.Id ?? null,
                UserId = user.Id,
                ActivityId = activity.Id,
                Status = status,
            };
            if (ex != null)
            {
                log.Narration = ex.Message;
                log.DetailedError = ex.StackTrace;
            }
            await Save(log);
            log.Transaction = transaction;
            log.Activity = activity;
            // Make sure to return a value at the end of the method
            return log;
        }



        public async Task<Activity> UpsertActivity(Activity activity)
        {
            

            if (activity.Id <= 0)
            {
                var existing = _<Activity>().FirstOrDefault(a => a.Name.ToLower() == activity.Name.ToLower() && a.RoleLevel == activity.RoleLevel && a.UserRole == activity.UserRole);
                if (existing != null)
                    activity.Id = existing.Id;
            }
            await Save(activity);
            return activity;
        }

        private string GetActivityKey(Activity activity, int userId, string? uniqueKey)
        {
            if (uniqueKey == null)
                return GetActivityKeyByTimeInterval(activity, userId);
            return $"{activity.Id}_{userId}_{uniqueKey}";
        }

        private string GetActivityKeyByTimeInterval(Activity activity, int userId)
        {
            string currentInterval = "";
            switch (activity.Interval)
            {
                case DateInterval.Day:
                    currentInterval = DateTime.UtcNow.DayOfYear.ToString();
                    break;
                case DateInterval.Second:
                    currentInterval = $"{DateTime.UtcNow.DayOfYear}_{DateTime.UtcNow.Hour}_{DateTime.UtcNow.Minute}_{DateTime.UtcNow.Second}";
                    break;
                case DateInterval.Minute:
                    currentInterval = $"{DateTime.UtcNow.DayOfYear}_{DateTime.UtcNow.Hour}_{DateTime.UtcNow.Minute}";
                    break;

                case DateInterval.Month:
                    currentInterval = $"{DateTime.UtcNow.Month}";
                    break;
                case DateInterval.Year:
                    currentInterval = DateTime.UtcNow.Year.ToString();
                    break;
                case DateInterval.WeekOfYear:
                    currentInterval = CultureInfo.InvariantCulture.Calendar
                        .GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString();
                    break;
                default:
                    currentInterval = DateTime.UtcNow.DayOfYear.ToString();
                    break;
            }
            return $"{activity.Id}_{userId}_{currentInterval}_{DateTime.UtcNow.Year}";
        }

    }
}
