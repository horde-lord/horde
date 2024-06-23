using Core.Interfaces;
using Core.Interfaces.Data;

namespace Core.Domains.World.Entities
{
    public class DirectConversation : BaseEntity, IConversationContext
    {
        public override ContextNames Context => ContextNames.Ecosystem;

        public int? ConversationId { get; set; }

        public Conversation Conversation { get; set; }

        public int InitiatorUserId { get; set; }
        public User InitiatorUser { get; set; }

        public int ReceiverUserId { get; set; }
        public User ReceiverUser { get; set; }
    }
}
