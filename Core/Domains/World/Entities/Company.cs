using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class Company: Team
    {
        
        public Country Country { get; set; }
        public int CountryId { get; set; }
        public string RegistrationId { get; set; }
        public string TaxId { get; set; }
        public List<CompanyEmployee> Employees { get; set; }
        [NotMapped]
        public double ConversionRatio { get; set; }
    }
}
