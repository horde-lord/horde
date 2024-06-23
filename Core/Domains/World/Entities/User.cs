using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Domains.Games.Entities;
using Core.Domains.Admin.Entities;

namespace Core.Domains.World.Entities
{
    public class User : BaseEntity
    {
        public User()
        {
            //OcrLookups = new List<OcrLookup>();
            //GameSpecificInfos = new List<GameSpecificInfo>();
            Connections = new List<Connection>();
            Roles = new List<UserRole>();
        }
        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;
        [MaxLength(100)]
        public string Username { get; set; }
        [MaxLength(100)]
        public string EmailId { get; set; }
        public string Phone { get; set; }
        public virtual List<Connection> Connections { get; set; }
        //public List<GameSpecificInfo> GameSpecificInfos { get; set; }
        //public List<OcrLookup> OcrLookups { get; set; }
        public virtual Registration Registration { get; set; }
        public bool IsSuspended { get; set; }
        public virtual List<Member> Members { get; set; }
        public virtual List<UserRole> Roles { get; set; }
        public string ProfilePicUrl { get; set; }
        public string VerifiedNumber { get; set; }

        public int? CountryId { get; set; }
        
        
        public virtual Country BaseCountry { get; set; }

        public int? MergedWithUserId { get; set; }
        public int CurrencyId { get; set; }
        public string TimeZone { get; set; }
        public virtual List<UserContent> Contents { get; set; } = new();
        [NotMapped]
        public virtual Tenant Partner { get; set; }
        [NotMapped]
        public virtual Member CurrentMember { get; set; }


        [MaxLength(200)]
        public string RealName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public DateTime DateOfBirth { get; set; }
        public GenderType Gender { get; set; }
        [NotMapped]
        public virtual Player CurrentPlayer { get; set; }

        public string GetGameProfileUrl(string game = "ff")
        {

            return $"{Id}/{game}/profile.jpg";
        }

        public string GetAvatar()
        {
            if (ProfilePicUrl?.Length > 0)
                return ProfilePicUrl;
            foreach (var connection in Connections)
            {
                if (connection.ProfilePicUrl?.Length > 0)
                    return connection.ProfilePicUrl;
            }

            return "";
        }

		public Company CompanyId(Company company)
		{
			throw new NotImplementedException();
		}
	}
}

namespace Core
{
    public enum GenderType
    {
        Male, Female, Other
    }
}