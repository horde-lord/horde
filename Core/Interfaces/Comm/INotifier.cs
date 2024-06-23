using Core.Domains;

namespace Core.Interfaces.Comm
{
    public interface INotifier
    {
        public bool ShouldNotify(BaseEvent @event);

        public Task<BaseNotification> GetNotification(BaseEvent @event);
    }

    public abstract class BaseNotification
    {
        public NotificationType Type { get; set; }
        public int? UserId { get; set; }
        public List<int> UserIds { get; set; } = new List<int>();
        public string? Topic { get; set; }
        public string? Token { get; set; }
    }
}

namespace Core
{
    public enum NotificationType
    {
        SingleDevice, User, Topic, Group
    }
}