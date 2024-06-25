using Horde.Core.Interfaces;
using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.Games.Entities
{
    public class Category : BaseEntity, INamed
    {
        public override ContextNames Context => ContextNames.Game;

        public string Name { get; set; }
        public List<Game> Games { get; set; } = new();
    }
}
