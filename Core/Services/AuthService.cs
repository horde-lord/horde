using Autofac;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;

namespace Horde.Core.Services
{
    public class AuthService : BaseService
    {
        public AuthService(ILifetimeScope scope) : base(scope, ContextNames.Ecosystem)
        {
        }

        public User RegisterUser(User user)
        {
            return user;

        }

        public async Task<List<User>> ImportUsers(string path)
        {
            var users = ImportFromCsv<User>(path);
            users.ForEach(u => u.Key = u.EmailId);
            var distinctUsers = users.GroupBy(u => u.EmailId).Select(u => u.Last()).ToList();
            var repo = GetRepository(ContextNames.Ecosystem);
            repo.UpsertRange(distinctUsers);
            await repo.SaveChanges();
            return distinctUsers;
        }

        public List<User> GetAllUnregisteredUsers()
        {
            return GetRepository(ContextNames.Ecosystem).GetNoTrackingQueryable<User>("Registration").Where(u => u.EmailId != null).ToList();
        }

        
        public async Task UpdateUserSuspension(int userId, bool suspend)
        {
            var repo = GetRepository(ContextNames.Ecosystem);
            var money = GetRepository(ContextNames.Money);
            var user = repo.GetNoTrackingQueryable<User>().SingleOrDefault(u => u.Id == userId);
            user.IsSuspended = suspend;
            var accounts = money.GetNoTrackingQueryable<Account>().Where(a => a.UserId == userId).ToList();
            accounts.ForEach(a => a.IsFrozen = suspend);
            repo.Upsert(user);
            money.UpsertRange(accounts);
            await repo.SaveChanges();
            await money.SaveChanges();
        }

        internal async Task<User> VerifyDiscordConnection(string code, string username, ulong id)
        {
            var repo = GetRepository(ContextNames.Ecosystem);
            var connection = repo.GetQueryable<Connection>().FirstOrDefault(c => c.InviteCode == code.ToLower() && !c.Deleted);
            if (connection == null)
                return null;
            connection.Established = true;
            var user = repo.GetQueryable<User>().SingleOrDefault(u => u.Id == connection.UserId);
            connection.UserName = username;
            user.Username = username;
            connection.ConnectionKey = id.ToString();
            await repo.SaveChanges();
            return user;
        }

        public List<User> GetAllUnregisteredConnections()
        {
            var eco = GetRepository(ContextNames.Ecosystem);
            var users = eco.GetNoTrackingQueryable<User>("Connections").Where(u => u.Connections.Count > 0 && u.Registration == null).ToList();
            return users;
        }

    }
}
