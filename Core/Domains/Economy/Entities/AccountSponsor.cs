using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Economy.Entities
{
    public class AccountSponsor : BaseConfigurableEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Economy;
        public int UserId { get; set; }
        public string ProviderName { get; set; }
        public string? ProviderCredentials { get; set; }
        public List<Account> SponsoredAccounts { get; set; }
        public int PayInAccountId { get; set; }
        public int PayOutAccountId { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }

        public int? CountryId { get; set; }

        [NotMapped]
        public Country BaseCountry { get; set; }

        public AccountSponsorType Type { get;set; }

    }

    public enum AccountSponsorType
    {
        Gaming, Marketing,Private
    }
}
