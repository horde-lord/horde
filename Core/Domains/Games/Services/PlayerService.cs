using Autofac;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Domains.Games.Entities;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Horde.Core.Utilities;

namespace Horde.Core.Domains.Games.Services
{
    public class PlayerService : BaseService
    {
        private RegistrationService _registrationService;
        public PlayerService(ILifetimeScope scope, RegistrationService registrationService) : base(scope, ContextNames.Game)
        {
            _registrationService = registrationService;
        }

        public List<Player> GetPlayers(int userId)
        {
            var players = _<Player>("Aliases", "Game").Where(p => p.UserId == userId && p.Deleted == false).ToList();
            players.RemoveAll(p => p.Game.Deleted);
            foreach (var player in players)
            {
                player.ProfilePicUrl = player.GetDownloadableProfilePicUrl();
            }
            return players;
        }

        public Player GetPlayerById(int playerId)
        {
            return _<Player>("Game").Where(p => p.Id == playerId && !p.Deleted).FirstOrDefault();
        }

        public Player GetPlayer(int userId, int gameId)
        {
            var player = _<Player>("Game").Where(p => p.UserId == userId && p.GameId == gameId && !p.Deleted).SingleOrDefault();
            return player;
        }


        private bool UpdateExistingPlayer(Player existingPlayer, Player newPlayer)
        {
            if (newPlayer == null || existingPlayer == null)
                return false;
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var dirty = false;

            if (existingPlayer.ExtractedGameUserId?.IsNotEqualTo(newPlayer.ExtractedGameUserId, allowEmpty: false) == true)
            {
                dirty = true;
                existingPlayer.ExtractedGameUserId = newPlayer.ExtractedGameUserId;
            }
            if (existingPlayer.ExtractedIgn?.IsNotEqualTo(newPlayer.ExtractedIgn, allowEmpty: false) == true)
            {
                dirty = true;
                existingPlayer.ExtractedIgn = newPlayer.ExtractedIgn;
            }
            if (existingPlayer.IGN?.IsNotEqualTo(newPlayer.IGN, allowEmpty: false) == true)
            {
                dirty = true;
                existingPlayer.IGN = newPlayer.IGN;
            }
            if (existingPlayer.ProfilePicUrl?.IsNotEqualTo(newPlayer.ProfilePicUrl, allowEmpty: false) == true)
            {
                dirty = true;
                existingPlayer.ProfilePicUrl = newPlayer.ProfilePicUrl;
            }
            if (existingPlayer.GameUserId?.IsNotEqualTo(newPlayer.GameUserId, allowEmpty: false) == true)
            {
                dirty = true;
                existingPlayer.GameUserId = newPlayer.GameUserId;
            }
            return dirty;
        }

        public async Task<Player> AddPlayer(Player player)
        {
            player.IGN = player?.IGN?.Trim() ?? "";
            

            var existingPlayer = _<Player>("Aliases")
                .SingleOrDefault(p => p.UserId == player.UserId && p.GameId == player.GameId);
            if (existingPlayer == null)
            {
                if (string.IsNullOrEmpty(player.IGN))
                    throw new Exception("Player IGN cannot be empty");
                player.Game = null;
                await Save(player);
                return player;
            }
            else
            {
                var dirty = UpdateExistingPlayer(existingPlayer, player);
                if (!(player.ExtractedLevel == null) && player.ExtractedLevel != existingPlayer.ExtractedLevel)
                {
                    dirty = true;
                    existingPlayer.ExtractedLevel = player.ExtractedLevel;
                }

                foreach (var alias in player.Aliases)
                {
                    if (existingPlayer.Aliases.Any(o => o.TextType == alias.TextType && o.OcrText == alias.OcrText))
                        continue;
                    dirty = true;
                    existingPlayer.Aliases.Add(alias);
                }
                try
                {
                    if (dirty)
                        await Save(existingPlayer);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to save player" + ex.Message);
                }

            }
            return existingPlayer;
        }

        public Player GetPlayerGraph(int playerId)
        {
            var player = _<Player>(playerId, "Game", "Squad");
            var user = _<User>(player.UserId, "Connections");
            player.User = user;
            return player;
        }

        public async Task<Player> GetOrCreatePlayer(User user, int gameId, string ign)
        {
            var player = _<Player>().FirstOrDefault(p => p.UserId == user.Id && p.GameId == gameId);
            if(player != null)
            {
                player.CurrentIgn = ign;
                return player;
            }
            if (string.IsNullOrEmpty(ign))
            {
                ign = user.Username;
            }
            player = await AddPlayer(new Player() 
            {
                UserId = user.Id,
                GameId = gameId,
                IGN = ign,
                CurrentIgn = ign,
                
            });
            return player;
        }

        public async Task<Player> UpdatePlayer(User user, int gameId, string ign, string uid)
        {
            var player = await GetOrCreatePlayer(user, gameId, ign);
            player.IGN = ign;
            if (!string.IsNullOrEmpty(uid))
            {
                player.GameUserId = uid;
            }
            await Save(player);
            return player;
        }
    }
}
