using Autofac;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Horde.Core.Domains.World.Services
{

    public class CurrencyService : BaseService
    {
        IMemoryCache _cache;
        public CurrencyService(ILifetimeScope scope, ContextNames name = ContextNames.Ecosystem) : base(scope, name)
        {
            _cache = scope.Resolve<IMemoryCache>();
        }

        public string emptyDigitalCurrencyLogo = "https://tribalassets.blob.core.windows.net/partners/shared/emptyAssets/emptyCurrencyLogo.png";

        public Currency GetCurrency(int id)
        {
            //if(!_cache.TryGetValue($"Currency:{id}", out Currency currency))
            //{
            //    _cache.CreateEntry($"Currency:{id}").Value = __<Currency>(id);
            //}
            return _cache.GetOrCreate($"Currency:{id}", (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return __<Currency>(id);
            }
            );
        }

        public async Task GetExchangeRateFromApi()
        {
            var currencies = _<Currency>().Where(c => c.Type == CurrencyNatureType.Fiat).ToList();
            string currencyShortNames = string.Join(",", currencies.Select(c => c.ShortName));
            var baseCurrency = "USD";
            var url = $"https://api.apilayer.com/exchangerates_data/latest?symbols={currencyShortNames}&base={baseCurrency}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", "Qw6DhFrOmMopaW0IRglmPuvE1Ldj099C");
            var response = await Http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);

                var rates = jsonDocument.RootElement.GetProperty("rates");

                foreach (var rate in rates.EnumerateObject())
                {
                    string currencyIso = rate.Name;
                    double exchangeRate = rate.Value.GetDouble();

                    var currency = currencies.FirstOrDefault(c => c.ShortName == currencyIso);
                    if (currency != null)
                    {
                        currency.ExchangeRate = (decimal)exchangeRate;
                    }
                }
                await Save(currencies);
            }
        }

        public decimal? GetExchangedRateAmount(decimal fromAmount,int from, int to)
        {
            var fromCurrency = __<Currency>().FirstOrDefault(c => c.Id == from);
            var toCurrency = __<Currency>().FirstOrDefault(c => c.Id == to);
            return GetExchangedRateAmount(fromAmount, fromCurrency, toCurrency);
        }

        public decimal GetExchangedRateAmount(decimal fromAmount, Currency from, Currency to)
        {
            if (from == null || to == null)
                return 0;
            var toAmount = (fromAmount * (to?.ExchangeRate ?? 0) / (from?.ExchangeRate ?? 0));
            return toAmount;
        }



        public decimal GetExchangeRate(int to, int from, List<Currency> currencies)
        {
            var toCurrency = currencies.FirstOrDefault(c => c.Id == to);
            var fromCurrency = currencies.FirstOrDefault(c => c.Id == from);
            return toCurrency.ExchangeRate / fromCurrency.ExchangeRate;
        }
    }
}
