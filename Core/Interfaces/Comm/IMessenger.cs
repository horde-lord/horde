using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Horde.Core.Interfaces.Comm
{
    public interface IMessage<T> where T : new()
    {
        string Key { get; set; }
        string StringValue { get; set; }
        T TypedValue { get; set; }

        string Topic { get; set; }
    }

    public interface IMessenger : IHostedService
    {
        Task Publish<T>(IMessage<T> message) where T:new();
        Task RegisterConsumer<T>(Func<T, Task> consumer, string group, params string[] topics) where T:new();
        Task CheckHealth();

        bool Started { get; }
    }

    public class Message<T> : IMessage<T> where T : new()
    {
        public Message(T t)
        {
            TypedValue = t;
        }
        public Message()
        {

        }
        public string Key { get; set; }
        public string StringValue { get; set; }
        public string Topic { get; set; }
        public T TypedValue
        {
            get => JsonSerializer.Deserialize<T>(StringValue,
                    new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve });

            set
            {
                StringValue = JsonSerializer.Serialize(value,
                    new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve });
            }
        }

    }
}
