﻿using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Economy.Entities
{
    public class Adjustment : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Money;
        public Account Account { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }

    }
}

