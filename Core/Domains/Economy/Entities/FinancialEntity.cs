using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Economy.Entities
{
    public class FinancialEntity : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Money;
    }
}