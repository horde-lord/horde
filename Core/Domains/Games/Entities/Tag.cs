using Core.Interfaces;
using Core.Interfaces.Data;

namespace Core.Domains.Games.Entities
{
    public class Tag : BaseEntity, INamed
    {
        public override ContextNames Context => ContextNames.Game;

        public string Name { get; set; }
        public List<Game> Games { get; set; }
    }
}
