using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class Influencer : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;
        public int UserId { get; set; }
        public User User { get; set; }
        public string ChannelName { get; set; }
        public string ChannelUrl { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string PhoneNumber { get; set; }
        public List<InfluencerSession> Sessions { get; set; }

    }
}
