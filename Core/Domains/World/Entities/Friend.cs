using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.World.Entities
{
    public class Friend : BaseEntity
    {
        public override ContextNames Context => ContextNames.World;
        public int FirstUserId { get; set; }
        public int SecondUserId { get; set; }
        public User FirstUser { get; set; }
        public User SecondUser { get; set; }
        public FriendshipStatusType Status { get; set; }

    }

}

namespace Horde.Core.Domains.World.Entities
{
    public enum FriendshipStatusType
    {
        Requested, Rejected, Active, UnfriendedByFirstUser, UnfriendedBySecondUser
    }
}