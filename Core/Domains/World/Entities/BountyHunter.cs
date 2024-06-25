using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.World.Entities
{
    public class BountyHunter : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public User Hunter { get; set; }
        public int HunterId { get; set; }
        public Bounty Bounty { get; set; }
        public int BountyId { get; set; } 
        public BountyHunterStatusType Status { get; set; }
    }
    public enum BountyHunterStatusType
    {
    }
}