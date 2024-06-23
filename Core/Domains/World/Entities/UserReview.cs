using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;

namespace Core.Domains.World.Entities
{
    public class UserReview : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public int ReviewerId { get; set; }
        public User Reviewer { get; set; }
        public int EntityId { get; set; }
        [MaxLength(100)]
        public string EntityName { get; set; }
        public ReviewType Type { get;set; }
        public Conversation Conversation { get; set; }
        public int ConversationId { get; set; }
        [Range(1,5)]
        public int Rating { get; set; }
    }
    public enum ReviewType
    {
        Organizer, TournamentReview, MatchReview
    }
}