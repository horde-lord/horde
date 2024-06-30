using Horde.Core.Interfaces;
using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Admin.Entities
{
    public class Tenant : BaseEntity, INamed
    {
        public override ContextNames Context => ContextNames.Admin;
        public List<Asset> Assets { get; set; }

        public Tenant(List<Asset> assets)
        {
            Assets = assets;
        }

        public Tenant()
        {

        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerUserId { get; set; }
        public bool IsPublic { get; set; }
        public int CompanyId { get; set; }
        public int CurrencyId { get; set; }


        public SubscriptionTierType Tier { get; set; }
       

        public int DigitalCurrencyId { get; set; }

        public string GetAsset(AssetType type)
        {
            var asset = Assets?.FirstOrDefault(a => a.Type == type);
            if (asset == null || string.IsNullOrEmpty(asset.Value))
            {
                return GetAssetDefaultValue(type);
            }
            return asset.Value;
        }

        public static string GetAssetDefaultValue(AssetType type)
        {
            switch (type)
            {
                case AssetType.Logo:
                    return "https://tribalassets.blob.core.windows.net/partners/shared/emptyAssets/emptyBrandLogo.png";
                case AssetType.Name:
                    return "";
                case AssetType.DiscordSlashCommandName:
                    return "tr";
                case AssetType.Icon:
                    return "https://tribalassets.blob.core.windows.net/partners/shared/emptyAssets/emptyBrandLogo.png";
                case AssetType.BrandNameLogo:
                    return "https://tribalassets.blob.core.windows.net/partners/shared/emptyAssets/emptyBrandNameLogo.png";
                case AssetType.DigitalCurrencyLogo:
                    return "https://tribalassets.blob.core.windows.net/partners/shared/emptyAssets/emptyCurrencyLogo.png";
                case AssetType.BackgroundColor:
                    return "Dark";
                case AssetType.DiscordServerInvite:
                    return "https://discord.gg/hRcDzGnwhC";
            }
            return "";
        }



        [NotMapped]
        public bool IsUserTenantEmployee = false;
    }
    public enum SubscriptionTierType
    {
        Free,
        Basic,
        Pro,
        Enterprise
    }
}
