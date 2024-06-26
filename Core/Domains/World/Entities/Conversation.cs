﻿using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.World.Entities
{
    public class Conversation : BaseEntity
    {
        public override ContextNames Context => ContextNames.World;
        public List<Talk> Talks { get; set; }
        public User Initiator { get; set; }
        public int InitiatorId { get; set; }
        
    }
}