using Autofac;
using Core.Domains.Games.Entities;
using Core.Interfaces.Data;
using Core.Services;

namespace Core.Domains.Games.Services
{
    public class GameService : BaseService
    {
        public GameService(ILifetimeScope scope) : base(scope, ContextNames.Game)
        {
        }

    
        public async Task AddGame(Game game)
        {
            var existing = _<Game>().Any(g => g.Name == game.Name);
            if (existing)
                throw new GameException($"The game {game.Name} already exists");
            await Save(game);
        }

        public Game GetGameById(int gameId)
        {
            return _<Game>().Where(g => g.Id == gameId && g.Deleted == false).SingleOrDefault();
        }

        public Game GetGameByKey(string gameKey)
        {
            return _<Game>().Where(g => g.Key == gameKey && g.Deleted == false).SingleOrDefault();
        }

        public List<Game> GetGamesByKeys(string gameKeys)
        {
            var gameKeyList = gameKeys.Split(',');
            if (gameKeyList.Contains("all"))
                return _<Game>().Where(g => !g.Deleted).ToList();
            else
                return _<Game>().Where(g => gameKeys.Contains(g.Key) && !g.Deleted).ToList();
        }

        public List<string> GameProfileDataRequired(string gameName)
        {
            return GetProvider(gameName).ProfileDataRequired;
        }

        public List<Game> GetGames()
        {
            return _<Game>().Where(g => g.Deleted == false).ToList();
        }

        public async Task<Player> VerifyPlayer(Player player)
        {
            var game = _<Game>(player.GameId);
            var provider = GetProvider(game.Name);
            return await provider.VerifyPlayerProfile(player);
        }

        public BaseGameProvider GetProvider(string name)
        {
            var providers = Get<IEnumerable<BaseGameProvider>>();
            var provider = providers.FirstOrDefault(p => p.Name == name);
            if (provider == null)
                provider = providers.FirstOrDefault(p => p.Name == "Default");
            return provider;
        }



        public string GetPlayerVerificationScreenShot(int gameId, int squadSize)
        {
            var game = _<Game>(gameId);
            var provider = GetProvider(game.Name);
            //if (squadSize > 1)
            //    return provider.ImageUrls[$"PositionClaim_{squadSize}"];
            return provider.ImageUrls["PositionClaim"];

        }

        public async Task<string> GetGameUrl(int id) {
            var game = _<Game>(id);
            return game.Url;
        }

       
    }
}
