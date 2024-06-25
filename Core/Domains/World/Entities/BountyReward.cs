using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.World.Entities
{
    public class BountyReward : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public int Currency { get; set; }
        public decimal Amount { get; set; }
        public int Level { get; set; } = 1;
        public User Winner { get; set; }
        public int WinnerId { get; set; }
        public Bounty Bounty { get; set; }
        public int BountyId { get; set; }

    }
}