using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Horde.Core.Domains.Games.Entities
{
    public class Game : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Game;
        public string Name { get; set; }
        public int OwnerUserId { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public int? CompanyId { get; set; }
        public IntegrationType IntegrationType { get; set; }
        
        public List<GameModeConfiguration> ModeConfigurations { get; set; }
        public int KillMultiplier { get; set; }
        public string PositionPointJson { get; set; }
        
        public string Url { get; set; }
        public List<Tag> Tags { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public GameType Type { get; set; }
        [NotMapped]
        [JsonIgnore]
        public Dictionary<int, int> PositionPoints => JsonSerializer.Deserialize<Dictionary<int, int>>(PositionPointJson);
        //[NotMapped]
        //public List<Tournament> Tournaments { get; set; }
        [NotMapped]
        public bool HasPlaySteps  => ModeConfigurations?.FirstOrDefault()?.PlaySteps?.Any() ?? false;
        [NotMapped]
        public int CurrentParticipation { get; set; }


        //public List<string> GameProfileDataRequired(BaseService service)
        //{
        //    return service.Get<BaseGameProvider>(Name).ProfileDataRequired;
        //}

    }
}

namespace Horde.Core.Domains.Games.Entities
{
    public enum IntegrationType
    {
        AIAssisted, ApiAssisted, Manual
    }
}

namespace Horde.Core.Domains.Games.Entities
{
    public enum GameType
    {
        Esports = 0, 
        Casual = 1,
        PvP = 2,
    }

    public enum GameCategory
    { 
       Action = 4,
       BattleRoyale = 5,
       Strategy= 6,
       Puzzle=7,
       Sports=8,
       Casual=9,
       Board=10,
       Arcade=11,
    
    
    }
}