﻿using Core.Domains.World.Entities;

using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Economy.Entities
{
    public class Account : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Money;
        public decimal Balance { get; set; }
        public virtual Currency Currency { get; set; }
        public int CurrencyId { get; set; }
        public AccountType Type { get; set; }
        public int? UserId { get; set; }

        [NotMapped]
        public virtual User User { get; set; }
        public string Name { get; set; }
        public string Purpose { get; set; }
        public bool IsFrozen { get; set; }
        //A locked account is like an escrow that can be used only for a single purpose once locked
        public bool IsLocked { get; set; }
        public int? AccountSponsorId { get; set; }
        public virtual AccountSponsor AccountSponsor { get; set; }
        
        [NotMapped]
        public List<(string date, decimal amount, string reason,string mode,string narration,int sourceId,int destinationId)> TransactionSummary { get; set; }
        [NotMapped]
        public virtual List<GatewayPayout> Payouts { get; set; }
        [NotMapped]
        public Dictionary<string, string> PayoutInfo { get; set; }

        [NotMapped]
        public decimal LockedBalance { get; set; }
    }
}

namespace Core
{
    public enum AccountType
    {
        Global, User,Partner, RewardStrategy, 
        GatewayInput = 4, GatewayOutput = 5
    }
}