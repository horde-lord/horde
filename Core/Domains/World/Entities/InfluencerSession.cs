using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class InfluencerSession : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;
        public int CasterId { get; set; }
        public Influencer Caster { get; set; }
        public int? MatchId { get; set; }
        public int? TournamentId { get; set; }

        public int? GameId { get; set; }
        public string MatchName { get; set; }
        public string SessionUrl { get; set; }
        public decimal Rating { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
    }
}