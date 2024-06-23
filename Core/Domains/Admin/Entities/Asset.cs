﻿using Core.Interfaces.Data;

namespace Core.Domains.Admin.Entities
{
    public class Asset : BaseEntity
    {
        public override ContextNames Context => ContextNames.Partners;
        public AssetType Type { get; set; }
        public string Value { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

    }

    //0 1 4 5 pada hai
    public enum AssetType
    {
        Logo, 
        Name, 
        Icon,
        BackgroundColor,
        BrandNameLogo,
        [Obsolete("Use Currency.Logourl instead")]
        DigitalCurrencyLogo,
        DiscordSlashCommandName,
        StripeApiKey,
        DiscordServerInvite,
        OnMetaApiKey
    }
}