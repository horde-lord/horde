using Core.Interfaces.Data;

namespace Core.Domains.World.Entities
{
    public class Friend : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public int FirstUserId { get; set; }
        public int SecondUserId { get; set; }
        public User FirstUser { get; set; }
        public User SecondUser { get; set; }
        public FriendshipStatusType Status { get; set; }

    }

}

namespace Core.Domains.World.Entities
{
    public enum FriendshipStatusType
    {
        Requested, Rejected, Active, UnfriendedByFirstUser, UnfriendedBySecondUser
    }
}