using Horde.Core.Interfaces.Data;

namespace Horde.Core.Domains.Games.Entities
{
    public class GameMode: BaseEntity
    {
        public override ContextNames Context => ContextNames.Game;
        public string Name { get; set; }
        public List<GameModeConfiguration> Configurations { get; set; }
        public string TemplateJson { get; set; }
        public string MatchDeepLink { get; set; }

    }
}
