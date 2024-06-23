using Core.Domains.World.Entities;
using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Games.Entities
{
    public class Player : BaseEntity
    {
        public Player()
        {
            Aliases = new List<Alias>();
        }
        [NotMapped]
        public override ContextNames Context => ContextNames.Game;
        public int UserId { get; set; }
        public int GameId { get; set; }
        public string GameKey { get; set; }
        public Game Game { get; set; }
        /// <summary>
        /// In game name
        /// </summary>
        /// 
        [MaxLength(100)]
        public string IGN { get; set; }
        /// <summary>
        /// Sometime a game can have a technical looking id as well
        /// </summary>
        /// 
        [MaxLength(100)]
        public string GameUserId { get; set; }
        [MaxLength(100)]
        public string ExtractedGameUserId { get; set; }
        [MaxLength(100)]
        public string ExtractedIgn { get; set; }
        public int? ExtractedLevel { get; set; }

        public List<Alias> Aliases { get; set; }

        public string ProfilePicUrl { get; set; }
        [NotMapped]
        public Stream ImageStream { get; set; }
        //public int? SquadId { get; set; }
        //[ForeignKey("SquadId")]
        [NotMapped]
        public Squad CurrentSquad { get; set; }
        [NotMapped]
        public User User { get; internal set; }
        [NotMapped]
        public string CurrentIgn { get; set; }

        public string GetDownloadableProfilePicUrl()
        {
            //return ProfilePicUrl + sas;
            string profileUrl = $"{UserId}/{Game.Key}/profile.jpg";
            //return AzureBlobService.GetBlobUrl("users", profileUrl);
            return profileUrl;
        }



    }
}
