using Horde.Core.Ecosystem.Entities;
using Horde.Core.Interfaces.Data;
using Serilog;
using shortid;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

namespace Horde.Core.Domains
{
    public abstract class BaseEntity : IEquatable<BaseEntity>, IEntity
    {
        public BaseEntity()
        {
            Properties = new Dictionary<string, object>();
        }


        private static BufferBlock<List<BaseMessage>> _block =
            new BufferBlock<List<BaseMessage>>(new DataflowBlockOptions() { EnsureOrdered = true });
        private static BufferBlock<BaseEvent> _eventBlock =
            new BufferBlock<BaseEvent>(new DataflowBlockOptions() { EnsureOrdered = true });

        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string Key { get; set; }
        public bool Deleted { get; set; }
        public int PartnerId { get; set; } = 0;
        [NotMapped]
        public bool LoadedFromCache { get; set; } = false;
        [NotMapped]
        public string Data { get; set; }
        [NotMapped]
        public abstract ContextNames Context { get; }
        [NotMapped]
        public Dictionary<string, object> Properties { get; set; }

        [NotMapped]
        protected bool AllowEvents { get; set; } = false;
        


        public T GetProperty<T>(string name)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(Properties[name].ToString());
                }
                return default(T);

            }
            catch
            {
                Log.Error("Could not get property {p} from {T} with id {id}, key {key}", name, typeof(T).Name, Id, Key);
                return default(T);
            }
        }
        public string GetCacheKey(int id = 0)
        {
            if (Id > 0)
                id = Id;
            if (id <= 0)
                return string.Empty;
            return $"{GetType().Name}:{id}";
        }
        public void Publish(List<BaseMessage> messages)
        {
            _block.Post(messages);
        }

        public string GenerateCode(int length = 20)
        {

            return ShortId.Generate(new shortid.Configuration.GenerationOptions(useNumbers: true, length: length));
        }
        public void Publish(BaseMessage message)
        {
            var messages = new List<BaseMessage>();
            messages.Add(message);
            _block.Post(messages);
        }


        public void FireEvent(EntityEventType type, TimeSpan? ttl = null,
            string secondaryEventName = "", Dictionary<string, string> data = null, string textContent = "", string channelName = null,
            BaseMessage channelMessage = null)
        {
            if (!AllowEvents)
                return;
            var e = new BaseEvent(eventType: type, entityId: this.Id, entityName: this.GetType().Name,
                secondaryEvent: secondaryEventName, ttl: ttl, data: data, message: textContent, channelName: channelName, //tribeIdsCsv: tribeIdsCsv,
                baseMessage: channelMessage, partnerId: PartnerId);
            _eventBlock.Post(e);
        }

   

        public static IObservable<List<BaseMessage>> GetMessageObservable()
        {
            return _block.AsObservable();
        }
        public static IObservable<BaseEvent> GetEventObservable()
        {
            return _eventBlock.AsObservable();
        }
        
        public bool Equals(BaseEntity other)
        {
            return other.Id == this.Id;
        }
    }

    public interface IEntity
    {
        int Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
        string Key { get; set; }
        bool Deleted { get; set; }
    }

    public interface ICached
    {
        [NotMapped]
        CacheType CacheType => CacheType.Remote;
        [NotMapped]
        TimeSpan TimeToLiveInCache { get; }

    }

    public abstract class BaseConfigurableEntity : BaseEntity
    {
        public string Config { get; set; }
        public Dictionary<string, string> GetConfigData()
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(Config);
        }
        public void SetConfigData(Dictionary<string, string> data)
        {
            Config = JsonSerializer.Serialize(data);
        }
    }

    public enum CacheType
    {
        Local, Remote, Hybrid
    }

    public enum EntityEventType
    {
        Created, Activated, Updated, Deactivated, Deleted,
        UpdateCache
    }

    public class BaseMessage
    {
        public BaseMessage()
        {
            Embeds = new(); ComponentRows = new();
        }
        public BaseMessage(TribeTopics topic, string message = "", string tribeIdentifier = "",
            string channelName = "", string category = "",
            List<Dictionary<string, string>> embeds = null, List<List<Dictionary<string, string>>> componentRows = null, TimeSpan? ttl = null)
        {
            Topic = topic;
            Message = message;
            TribeIdentifier = tribeIdentifier;
            Embeds = embeds ?? new();
            ComponentRows = componentRows ?? new();
            ChannelName = channelName;
            Category = category;
            TimeToLive = ttl;
        }

        public BaseMessage(string message, Dictionary<string, string> embed = null, Dictionary<string, string> component = null)
        {
            Message = message;
            if (embed != null)
                Embeds = new() { embed };
            if (component != null)
            {
                if (ComponentRows == null)
                    ComponentRows = new();
                ComponentRows.Add(new() { component });
            }
        }

        public string ChannelName { get; set; }
        public string Category { get; set; }
        public TribeTopics Topic { get; set; }
        public string Message { get; set; }
        public string TribeIdentifier { get; set; }
        public List<Dictionary<string, string>> Embeds { get; set; }
        public List<List<Dictionary<string, string>>> ComponentRows { get; set; }
        public TimeSpan? TimeToLive { get; set; } = null;
        internal static void AddChannelAttributes(Dictionary<string, string> data,
            bool readOnly = true, bool isPublic = true)
        {
            data.TryAdd("ForceCreate", "true");
            if (!readOnly)
                if (!data.TryAdd("SendMessage", "true"))
                    data["SendMessage"] = "true";
                else
                if (!data.TryAdd("SendMessage", "false"))
                    data["SendMessage"] = "false";

            if (!isPublic)
                if (!data.TryAdd("Visibility", "private"))
                    data["Visibility"] = "private";
                else
                if (!data.TryAdd("Visibility", "public"))
                    data["Visibility"] = "public";

        }

        public static Dictionary<string, string> GetEmbed(string title, string description,
            Dictionary<string, string> fields, string url = "", string imageUrl = "")
        {

            return new Dictionary<string, string>()
            {
                { "Embed_Title", title }, { "Embed_Description", description},
                { "Embed_Url", url},
                { "Embed_Fields", JsonSerializer.Serialize(fields)},
                { "Embed_Image", imageUrl}
            };
        }



        /// <summary>
        /// Gets Action rows of components on Discord
        /// </summary>
        /// <param name="type">"button" or "select". Use data for select options</param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="action"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="style">"Primary" or "Success" or "Danger"</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetComponent(string type, string title, string description,
             string action, string url = "", Dictionary<string, string> data = null, string style = "Primary")
        {

            return new Dictionary<string, string>()
            {
                { "Action_Type", type },{ "Action_Action", action },
                { "Action_Title", title }, { "Action_Description", description},
                { "Action_Url", url },
                { "Action_Style", style },
                { "Action_Fields", JsonSerializer.Serialize(data)}
            };
        }
    }
}
