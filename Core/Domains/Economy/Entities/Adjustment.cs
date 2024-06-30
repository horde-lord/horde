using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Economy.Entities
{
    public class Adjustment : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Economy;
        public Account Account { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }

    }
}

