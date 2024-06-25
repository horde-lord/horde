using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class OrganizerTeam : Team
    {
        [NotMapped]
        public List<OrganizerTeamMember> OrganizerMembers { get; set; }  /*=> TeamMembers.Cast<OrganizerTeamMember>().ToList();*/

        [NotMapped]
        public bool IsSelected { get; set; }

        public void AddToTeam(ITeamMember teamMember)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromTeam(ITeamMember teamMember)
        {
            throw new NotImplementedException();
        }


    }
}

