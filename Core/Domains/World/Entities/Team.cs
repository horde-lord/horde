using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Interfaces;

namespace Core.Domains.World.Entities
{
    public class Team : BaseEntity, IProfile, IConversationContext

    {
        public Team() { }

        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public string Name { get; set; }
        public TeamStatusType TeamStatus { get; set; }
        public IEnumerable<TeamMember> TeamMembers { get; set; }
        //void AddToTeam(ITeamMember teamMember);
        //void RemoveFromTeam(ITeamMember teamMember);
        public string Remarks { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int? ConversationId { get; set; }
        public int? CountryId { get; set; }
        public Country BaseCountry { get; set; }
        public Conversation Conversation { get; set; }
    }

    public class TeamMember : BaseEntity, ITeamMember
    {
        public int TeamId { get; set; }
        public virtual Team Team { get; set; }
        public override ContextNames Context => ContextNames.Ecosystem;

        public TeamMemberStatusType MemberStatus { get; set; }

    }
    public interface ITeamMember : IEntity
    {
        TeamMemberStatusType MemberStatus { get; set; }
    }

    public enum TeamType
    {
        Alliance, OrganizerTeam
    }

    public enum TeamStatusType
    {
        Recruiting, Frozen, Inactive, Draft
    }

    public enum TeamMemberStatusType
    {
        Active, Paused, Revoked
    }

    public enum UserTeamMembership
    {
        Owner, Member, Viewer
    }
}
