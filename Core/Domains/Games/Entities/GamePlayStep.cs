using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.Games.Entities
{
    public class GamePlayStep : BaseEntity
    {
        public override ContextNames Context => ContextNames.Game;
        public int ConfigurationId { get; set; }
        public GameModeConfiguration Configuration { get; set; }
        public int Step { get; set; } = 1;
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        
    }
}
