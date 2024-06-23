using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.World.Entities
{
    public class AllianceMember : TeamMember
    {
        
        public Tribe Tribe { get; set; }
        public int TribeId { get; set; }
        
        [NotMapped]
        public Alliance Alliance => (Alliance)Team;

        
    }
}

