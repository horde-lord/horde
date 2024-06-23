using Core.Interfaces;
using Core.Interfaces.Data;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Economy.Entities
{


    public class Activity : BaseEntity, INamed
    {
        public override ContextNames Context => ContextNames.Money;

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DefaultAmountPerTransaction { get; set; }
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public DateInterval? Interval { get; set; } = DateInterval.Day;
        /// <summary>
        /// How many intervals should be waiting for (example 10 minutes) before the next transaction can be made
        /// </summary>
        public int IntervalCount { get; set; } = 1; 
        public int MaxTransactionsPerInterval { get; set; } = 1;
        public virtual Currency Currency { get; set; }
        public int CurrencyId { get; set; }
        public UserRoleType UserRole { get; set; }
        public RoleLevelType RoleLevel { get; set; }
        public bool RequiresUniqueKey { get; set; } = false;
        [NotMapped]
        public string UniqueKey { get; set; }
        [NotMapped]
        public decimal? Amount { get; set; }
        [NotMapped]
        public string Narration { get; set; }
    }

    public class ActivityLog : BaseEntity
    {
        public override ContextNames Context => ContextNames.Money;
        public virtual Activity Activity { get; set; }
        public int ActivityId { get; set; }
        public virtual Transaction Transaction { get; set; }
        public int? TransactionId { get; set; }
        public int UserId { get; set; }
        public ActivityStatusType Status { get; set; }
        public string Narration { get; set; }
        public string DetailedError { get; set; }
    }

    public enum ActivityStatusType
    {
        Duplicate,
        Completed,
        Cancelled,
        Failed
    }
}
