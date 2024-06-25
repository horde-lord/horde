using Autofac;
using Horde.Core.Domains.Games.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;

namespace Horde.Core.Domains.Games.Services
{
    public class SquadService : BaseService
    {
        private readonly IEntityContextRepository<IEntityContext> _game;
        public SquadService(ILifetimeScope scope) : base(scope, ContextNames.Game)
        {
            _game = GetRepository(ContextNames.Game);
        }








        public async Task<Squad> JoinSquad(int squadId, int playerId, int passcode)
        {
            var player = _<Player>(playerId);
            if (player == null)
                throw new Exception("No player found. You need to create a player profile before you can join a squad");
            //var existingSquad = GetSquadByPlayerId(playerId);
            //if (existingSquad != null)
            //    throw new Exception($"Player cannot join a new squad as it is a part of {existingSquad.Name} squad. Player needs to leave this squad in order to join another squad");
            var squad = _<Squad>().FirstOrDefault(s => s.Id == squadId && s.Deleted == false);
            if (squad == null)
                throw new ArgumentException("Could not find squad with SquadId: " + squadId);
            if (squad.Password != passcode.ToString())
                throw new ArgumentException("Password entered is incorrect");
            await Save(player);
            return _<Squad>(squadId, "Players", "Game");
        }

    }
}
