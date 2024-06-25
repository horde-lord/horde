namespace Horde.Core.Domains
{
    public class BaseEvent
    {
        public BaseEvent() { }
        public BaseEvent(EntityEventType eventType,
            int entityId, string entityName,  int? partnerId,
            string secondaryEvent = "", TimeSpan? ttl = null,
            Dictionary<string, string> data = null, string message = null, string channelName = null,
            //string tribeIdsCsv = null, 
            string topic = "", BaseMessage baseMessage = null)
        {
            Event = eventType;
            SecondaryEventName = secondaryEvent; Data = data;
            TimeToLive = ttl.GetValueOrDefault();
            EntityId = entityId;
            EntityName = entityName;
            Message = message;
            ChannelName = channelName;
            PartnerId = partnerId;
            //TribeIdsCsv = tribeIdsCsv;
            topic = Topic;
            BaseMessage = baseMessage;
        }
        public string ChannelName { get; set; }
        //public string TribeIdsCsv { get; set; }
        public TimeSpan TimeToLive { get; set; }
        public EntityEventType Event { get; set; }
        public string Message { get; set; }
        public string SecondaryEventName { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string Topic { get; set; }
        public BaseMessage BaseMessage { get; set; }
        public int? PartnerId { get; set; }

        //public BaseEntity Entity { get; set; }
    }
}
