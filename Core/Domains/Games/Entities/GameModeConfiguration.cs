using Core.Interfaces.Data;

namespace Core.Domains.Games.Entities
{
    public class GameModeConfiguration: BaseEntity
    {
        public override ContextNames Context => ContextNames.Game;
        public int GameModeId { get; set; }
        public GameMode GameMode { get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }
        public string Name { get; set; }
        public string TemplateJson { get; set; }
        public List<GamePlayStep> PlaySteps { get; set; }
        public bool IsMatchupAllowed { get; set; }
        
    }
}
