using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class TribeStats : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;
        public int TribeId { get; set; }
        public Tribe Tribe { get; set; }
        public string JsonData { get; set; }
        public int MonthlyActives { get; set; }
        public int WeeklyActives { get; set; }
        public int MaxActives { get; set; }
    }
}
