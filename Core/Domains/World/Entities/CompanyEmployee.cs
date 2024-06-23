namespace Core.Domains.World.Entities
{
    public class CompanyEmployee: TeamMember
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public CompanyEmployeeRoleType Role { get; set; }
    }

    public enum CompanyEmployeeRoleType
    {   
        Member, 
    }
}