using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.Economy.Entities
{
    public class FinancialEntity : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Economy;
    }
}