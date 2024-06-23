using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Games.Entities
{

    public class Squad : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Game;
        public int GameId { get; set; }
        public Game Game { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Password { get; set; }
        public int CaptainUserId { get; set; }
        [NotMapped]
        public List<Player> Players { get; set; }
        //[NotMapped]
        //public SquadInvite Invite { get; set; }
        public int? SquadInviteId { get; set; }
        public int? TournamentId { get; set; }
        //[NotMapped]
        //public List<Participant> Participants { get; set; }
        //[NotMapped]
        //public Tournament Tournament { get; set; }
        [NotMapped]
        public string CaptainUserName { get; set; }
        [NotMapped]
        public int Position { get; set; }
        //[NotMapped]
        //public Participant CaptainParticipant { get; set; }
        //[NotMapped]
        //public List<Match> MatchesParticipatingIn { get; set; }
        //[NotMapped]
        //public List<Participant> Participants { get; set; }

    }
}
