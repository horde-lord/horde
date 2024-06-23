using Core.Interfaces.Data;

namespace Core.Domains.World.Entities
{
    public class Incentive : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public decimal Amount { get; set; }
        public int? TransactionId { get; set; }
        public string Narration { get; set; }

        public override ContextNames Context => ContextNames.Ecosystem;
    }
    public enum IncentiveType
    {
        WeeklyAdminMinGuarantee
    }
}
