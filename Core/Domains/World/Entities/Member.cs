using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class Member : BaseEntity, ICached
    {
        public Member()
        {

        }

        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;
        public Tribe Tribe { get; set; }
        public int TribeId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        
        [NotMapped]
        public Conversation Conversation { get; set; }
        public string Configuration { get; set; }
        [MaxLength(100)]
        public string TribeIdentifier { get; set; }
        public ConnectionType Type { get; set; }
        [NotMapped]
        public CacheType CacheType => CacheType.Remote;
        [NotMapped]
        public TimeSpan TimeToLiveInCache => TimeSpan.FromDays(1);
    }
}