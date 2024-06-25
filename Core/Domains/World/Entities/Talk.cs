using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class Talk : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public string Content { get; set; }
        public User Speaker { get; set; }
        public int SpeakerId { get; set; }
        public Conversation Conversation { get; set; }
        public int ConversationId { get; set; }
        public string MediaUrl { get; set; }


        [NotMapped]
        public bool IsGroupTalk { get; set; }

        [NotMapped]
        public List<int> GroupParticipantsIds { get; set; }
        [NotMapped]
        public string GroupName { get; set; }
    }
}