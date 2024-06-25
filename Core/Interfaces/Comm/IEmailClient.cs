namespace Horde.Core.Interfaces.Comm
{
    public interface IEmailClient
    {
        void SendMail(string to, string recepient, string subject, string content);
    }
}
