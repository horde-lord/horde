namespace Core.Domains.World.Entities
{
    public class OrganizerTeamMember : TeamMember
    {
        
        public OrganizerTeam OrganizerTeam => (OrganizerTeam)Team;
        public User User { get; set; }
        public int UserId { get; set; }
        public string Remarks { get; set; }
    }
}

