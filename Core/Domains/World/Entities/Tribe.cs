using Horde.Core.Domains.Games.Entities;
using Horde.Core.Domains.Admin.Entities;
using Horde.Core.Ecosystem.Entities;
using Horde.Core.Interfaces;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Horde.Core.Domains.World.Entities
{
    public class Tribe : BaseEntity, ICached, IConversationContext
    {
        public Tribe()
        {
            AllowEvents = true;
            Members = new List<Member>();
        }
        [NotMapped]
        public CacheType CacheType => CacheType.Remote;
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public User Owner { get; set; }
        public ConnectionType Type { get; set; }
        [MaxLength(100)]
        public string TribeIdentifier { get; set; }
        public string InteractionId { get; set; }

        public string LogoUrl { get; set; }
        public string PublicUrl { get; set; }
        [JsonIgnore]
        public List<Member> Members { get; set; }

        public string Region { get; set; }
        public string Locale { get; set; }
        public string TimeZone { get; set; }

        public int RegisteredMemberCount { get; set; }
        public int ActiveMemberCount { get; set; }
        public TournamentVisibility TournamentVisibility { get; set; }
        public int WeeklyMinGuaranteedAmount { get; set; }
        public int TribeAdminPreferredCurrencyType { get; set; }
        public DateTime LastDisconnectedOn { get; set; }
        public string ConnectionConfig { get; set; }
        public int LastKnownMAU { get; set; }
        public DateTime MAULastUpdatedOn { get; set; }

        public ConnectionStatusType ConnectionStatus { get; set; }
        public string ConnectionRemarks { get; set; }

        
        public int? CountryId { get; set; }

        
        public Country BaseCountry { get; set; }

        [NotMapped]
        public List<Alliance> Alliances { get; set; }
        public List<AllianceMember> AllianceMemberships { get; set; }

        [NotMapped]
        public List<Game> Games { get; set; }
        
        [MaxLength(20)]
        public string InviteCode { get; set; }
        
        [JsonIgnore]
        [NotMapped]
        public Member CurrentMember { get; internal set; }
        [NotMapped]
        public override ContextNames Context => ContextNames.World;


        

        public string RefererId { get; set; }


        public string GamesAllowedCsv { get; set; } = "all";


        public TimeSpan TimeToLiveInCache => TimeSpan.FromDays(1);



        public int TribeId => Id;

        public int? ConversationId { get; set; }

        public Conversation Conversation { get; set; }
        [NotMapped]
        public string LogHeader => $"{Id}: {Name}: {RegisteredMemberCount}: {ConnectionStatus}: Partner {PartnerId}";

        public void AllowGames(string[] games)
        {
            GamesAllowedCsv = string.Join(",", games);
        }
        public void AllowAllGames(string[] games)
        {
            GamesAllowedCsv = "all";
        }
        public bool IsGameAllowed(string gameKey)
        {
            if (GamesAllowedCsv == "all")
                return true;
            var allowedGames = GamesAllowedCsv.Split(",").Select(g => g.ToLower()).ToList();
            return allowedGames.Contains(gameKey.ToLower());
        }

        public List<Game> GetGamesForTribe(BaseService service)
        {
            var games = new List<Game>();
            if (GamesAllowedCsv == "all")
                games = service._<Game>().Where(g => !g.Deleted).ToList();
            else
            {
                var gameKeys = GamesAllowedCsv.Split(",");
                games = service._<Game>().Where(g => gameKeys.Contains(g.Key) && !g.Deleted).ToList();
            }
            return games;
        }

        

        

        public void Created()
        {
            FireEvent(EntityEventType.Activated,
                secondaryEventName: TribeTopics.TribeCreated.ToString());
        }

        internal void TournamentCreated(int tournamentId)
        {
            FireEvent(EntityEventType.Updated, secondaryEventName: TribeTopics.TribeTournamentChannelsCreated.ToString(),
                textContent: $"{tournamentId}");
        }

        public string GetChannelName(TribeChannelType channelType)
        {
            switch (channelType)
            {
                case TribeChannelType.About:
                    return "about";
                case TribeChannelType.UpcomingEvents:
                    return "upcoming-events";
                case TribeChannelType.DailyWinners:
                    return "daily-winners";
                case TribeChannelType.Announcements:
                    return "announcements";
                case TribeChannelType.Helpdesk:
                    return "helpdesk";
                case TribeChannelType.AdminSupport:
                    return "admin-support";
            }
            return "";
        }

        public string GetChannelName(string channel)
        {
            if (Enum.TryParse<TribeChannelType>(channel, out var channelType))
                return GetChannelName(channelType);
            return "";
        }

        public int MinimumTribeSizeRequired()
        {
            return 0;
        }


        public string GetCategoryName(Tenant partner)
        {
            
            //string name = "TribalArena";
            return $"{partner.Name} Tournaments";
        }




        internal void TearDown()
        {
            FireEvent(EntityEventType.Updated, secondaryEventName: TribeTopics.RemoveTribe.ToString());
        }

        



    }
    public enum TribeChannelType
    {
        About,
        UpcomingEvents,
        DailyWinners,
        Announcements,
        Helpdesk,
        AdminSupport
    }
}

namespace Horde.Core.Ecosystem.Entities
{
    public enum TournamentVisibility
    {
        Private, Sponsored, All
    }

    public enum ConnectionStatusType
    {
        /// <summary>
        /// Tribe is active and available
        /// </summary>
        Active,
        /// <summary>
        /// We got kicked.
        /// </summary>
        Kicked,
        /// <summary>
        /// We have revoked access for these. We are not sure if we want them back
        /// </summary>
        WeRevoked,
        /// <summary>
        /// we wanted these but they havent given us the right permission
        /// </summary>
        InsufficientPermissions,
        ///<summary>we have the administration permission
        ///but server has to add us again becuase we are facing permission mismatch.
        ///</summary>
       PermissionMismatch,
        /// <summary>
        /// This is in between state, where some repair work is going on
        /// </summary>
        UnderMaintenance,
        /// <summary>
        /// We dont need these. Good riddance
        /// </summary>
        HardRemoved,
        ActiveClan
    }

    public enum TribeTopics
    {

        TribeCreated,
        TournamentCreated, TournamentArchived,
        TournamentRegistrationOpen, TournamentRegistrationClosed,
        MatchCreated, MatchRecreated, MatchCompleted, MatchArchived, MatchStateUpdated,
        MatchResultFinalized,
        MessageBroadcasting,
        //CreateCategory, DeleteCategory, CreateChannel, DeleteChannel, 
        //ChangeChannelVisibility, UpdateChannelDescription,

        ClearChannelMessages,

        AddMemberToChannel, AddMembersToChannel,
        RemoveMemberFromMatch, RemoveMembersFromChannel,

        SendMessageToMember, SendMessageToTribeChannel,
        SendMessageToTribeAdmin,
        MatchMessageAvailable,
        SendTournamentMessage,
        MatchGotParticipant,
        RemoveTribe,
        SendParticipationReminderToMember,
        TribeTournamentChannelsCreated,
        ScrimPublished,
        ScrimArchived,
        MatchLeaderboardUploaded,
        TournamentPublished,
        MatchJoiningStarted,
        MatchStartingIn2Minutes,
        MatchResultAnnounced,
        ParticipantMovedToNextStage,
        TransactionMade,
        UnpublishTournamentInTribe,
        TournamentPublishedInAlliance,
        TournamentRemovedFromAlliance,

        ParticipantRegistering,
        ParticipantRegistered,
        ParticipantPlayedMatch,
        ParticipantPaidFee,
        PersonalMessage,
        SupportRequestOpened,
        TournamentUpdated,
        MatchUpdated,

        PartnerAppAdded

        //SendMessageToTribeAdmin,

    }

}