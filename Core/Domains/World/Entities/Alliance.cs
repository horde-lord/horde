using Horde.Core.Domains.Games.Entities;
using Horde.Core.Domains.Games.Services;
using Horde.Core.Ecosystem.Entities;
using Horde.Core.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class Alliance : Team
    {
        public Alliance()
        {
            AllowEvents = true;
        }
        [NotMapped]
        public bool isPublishedInTournament { get; set; }
        public string GameKeys { get; set; }
        [NotMapped]
        public List<Game> Games { get; set; }
        [NotMapped]
        public List<AllianceMember> AllianceMembers {  get; set; }

        public bool IsOpen { get; set; }
        public int? MinMemberSizeRequirement { get; set; }
        public int? RewardCurrencyId { get; set; }
        public decimal? FirstPlayReward { get; set; }
        public decimal? RepeatPlayReward { get; set; }
        
        public decimal? RewardBudget { get; set; }


        public void SetGamesForAlliance(BaseService service)
        {
            Games = service.Get<GameService>().GetGamesByKeys(GameKeys);
        }

        internal bool HasTribe(Tribe tribe)
        {
            if (AllianceMembers == null)
                return false;
            return AllianceMembers.Any(m => m.TribeId == tribe.Id);
        }

        public void PublishTournament(int tid)
        {
            FireEvent(EntityEventType.Created, secondaryEventName: TribeTopics.TournamentPublishedInAlliance.ToString(),
                textContent: $"{tid}");
        }

        internal void UnpublishTournament(int tid)
        {
            FireEvent(EntityEventType.Deactivated, secondaryEventName: TribeTopics.TournamentRemovedFromAlliance.ToString(),
                textContent: $"{tid}");
        }
    }
}

