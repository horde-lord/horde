using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class Connection : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.World;
        public string UserName { get; set; }
        public ConnectionType Type { get; set; }
        [MaxLength(100)]
        public string ConnectionKey { get; set; }
        public string? InteractionId { get; set; }
        [MaxLength(20)]
        public string? InviteCode { get; set; }
        public DateTime InviteExpiry { get; set; }
        public bool Established { get; set; }
        public int UserId { get; set; }
        [Column("InviteUrl")]
        public string? ProfilePicUrl { get; set; }
        public string? EmailId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        
        public int? MergedWithConnectionId { get; internal set; }
    }
}

namespace Horde.Core
{
    public enum ConnectionType
    {
        Discord = 0, 
        Teams = 1, 
        Facebook = 2, 
        Slack = 3, 
        Blackboard =4, 
        Telegram =5,
        GoogleCaster =6, 
        GoogleBasic = 7,
        Clan = 8, 
        Whatsapp = 9, 
        Group = 10,
        Zulip = 11, 
        Web = 12, 
        Identity = 13,
        Tenant = 14
    }
}