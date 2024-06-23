using System.ComponentModel.DataAnnotations;
using Core.Interfaces;
using Core.Interfaces.Data;

namespace Core.Domains.Economy.Entities;

public class Currency: BaseEntity, INamed
{
    public override ContextNames Context => ContextNames.Money;
    public string Name { get; set; }
    public string ShortName { get; set; }
    public CurrencyNatureType Type { get; set; }
    public int? OwnerId { get; set; }



    //exchange rate in terms of USD
    public decimal ExchangeRate { get; set; }

    [MaxLength(10)]
    public string Symbol { get; set; }

    
    public string LogoUrl { get; set; }
    
    public int Decimal { get; set; }

}

public enum CurrencyNatureType
{
    Fiat, DigitalCurrency
}