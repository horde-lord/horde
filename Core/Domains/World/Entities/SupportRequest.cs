using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Domains.World.Entities
{
    public class SupportRequest : BaseEntity
    {
        public override ContextNames Context => ContextNames.Ecosystem;
        public int UserId { get; set; }
        public User User { get; set; }
        public int AssignedToUserId { get; set; }
        public User AssignedToUser { get; set; }
        public int EntityId { get; set; }
        public string EntityType { get; set; }
        public SupportRequestType Type { get; set; }
        public RequestState State { get; set; }
        public string SubType { get; set; }
        public int? ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        /// <summary>
        /// Not to be provided by user
        /// </summary>
        public string RouteUrl { get; set; }
        /// <summary>
        /// not to be provided by user
        /// </summary>
        public string AdditionalData { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public Dictionary<string, List<List<string>>> Information { get; set; }
        [NotMapped]
        public List<string> SupportFiles { get; set; }

        [NotMapped]
        private static JsonSerializerOptions _options = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve };

        [NotMapped]
        public string Message { get; set; }

        public static SupportRequest CreateSupportRequest<T>(int userId, int assignedUserId, RequestState state,
            SupportRequestType type, string subType, T additionalData,string description = "", string url = "", int entityId = 0,int? conversationId = null)
        {
            return new SupportRequest()
            {
                UserId = userId,
                AssignedToUserId = assignedUserId,
                EntityId = entityId,
                Type = type,
                State = state,
                SubType = subType,
                RouteUrl = url,
                AdditionalData = JsonSerializer.Serialize(additionalData, options: _options),
                Description = description,
                ConversationId = conversationId
            };
        }

        public T GetSupportRequestValue<T>()
        {
            return JsonSerializer.Deserialize<T>(AdditionalData, options: _options);
        }
    }

    public enum RequestState
    {
        Open, Resolved, Rejected, Pending
    }


    public enum SupportRequestType
    {
        PlayerRegistration = 1, 
        MatchClaim = 2,
        DuplicateGameUserId = 3, 
        Feedback = 4,
        /// <summary>
        /// All issues which can be raised by tournament participant to the organizer
        /// </summary>
        ParticipantIssue =5
    }


    public enum SupportRelationType
    {
        Viewer = 0,
        User = 1,
        Admin = 2
    }
}
