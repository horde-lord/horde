using Core.Interfaces.Data;

namespace Core.Domains.World.Entities
{
    public class Conversation : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public List<Talk> Talks { get; set; }
        public User Initiator { get; set; }
        public int InitiatorId { get; set; }
        
    }
}