﻿using Core.Interfaces.Data;

namespace Core.Domains.World.Entities
{
    public class InviteLanding : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public int UserId { get; set; }
        public User User { get; set; }
        public int ReferrerId { get; set; }
        public User Referrer { get; set; }
        public int InviteId { get; set; }
    }
}
