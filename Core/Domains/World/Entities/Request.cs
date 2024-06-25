using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.World.Entities
{
    public abstract class Request : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public int RequesterId { get; set; }
        public User Requester { get; set; }
        public List<Conversation>? Conversations { get; set; }
        public RequestStateType State { get; set; }

    }

    public enum RequestStateType
    {
    }

    public class CastingRequest: Request
    {
        public int CasterId { get; set; }
        public Influencer Caster { get; set; }
        public int TournamentId { get; set; }
        public InfluencerSession CastingSession { get; set; }
        public int CastingSessionId { get; set; }
    }

    public class Bounty: Request
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<BountyReward> Rewards { get; set; }
        public List<BountyHunter> Hunters { get; set; }
    }
    
}