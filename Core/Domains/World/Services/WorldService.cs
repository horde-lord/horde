using Autofac;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Domains.Economy.Services;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;

namespace Horde.Core.Domains.World.Services
{
    public class WorldService : BaseService
    {
        public WorldService(ILifetimeScope scope) : base(scope, ContextNames.World)
        {
        }

        public async Task  Init()
        {
            var myOrg = _<Tenant>(1);
            if(myOrg == null)
            {
                //create partner
                myOrg = new Tenant()
                {
                    Key = "me",
                    Name = "Me",
                    Description = "This is strictly for hello worlding"
                };
                await Save(myOrg);

            }

            using (var registrationService = GetTenanted<RegistrationService>(myOrg.Id))
            using (var currencyService = GetTenanted<CurrencyService>(myOrg.Id))
            {
                var currency = currencyService._<Currency>(1);
                if(currency == null)
                {
                    //Add Currency
                    currency = new Currency()
                    {
                        Name = "My Cool Currency",
                        ShortName = "MCC",
                        Type = CurrencyNatureType.DigitalCurrency,
                        Symbol = "MC🆒",
                        LogoUrl = "https://example.com/cool-logo.jpg",
                        Key = "mcc"
                    };
                    await currencyService.Save(currency);

                }
                //Create user

                var me = await registrationService.SearchOrCreateRegistration("MySystemUserId", "Me", ConnectionType.Tenant);

            }


            

        }
    }
}
