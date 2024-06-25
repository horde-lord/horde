using Horde.Core.Interfaces;
using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.World.Entities
{
    public class UserContent : BaseEntity, IFeedContent
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public int UserId { get; set; }
        public User User { get; set; }
        public string Title { get ; set ; }
        public string Description { get ; set ; }
        public string ImageUrl { get ; set ; }
        public string DeepLink { get ; set ; }
    }
}
