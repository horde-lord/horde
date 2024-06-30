using Autofac;
using Horde.Core.Domains;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Domains.Games.Entities;
using Horde.Core.Domains.Games.Services;
using Horde.Core.Interfaces.Data;
using Horde.Core.Utilities;
using Serilog;
using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Horde.Core.Domains.Economy.Services;
using EnsureThat;
using Horde.Core.Services;

namespace Horde.Core.Domains.World.Services
{
    public class RegistrationService : BaseService
    {
        private readonly IEntityContextRepository<IEntityContext> _eco;

        public async Task AddRole(int id, string role)
        {
            if (!Enum.TryParse(typeof(UserRoleType), role, out var o))
                throw new ArgumentException("Invalid role. Please check and try again");
            var roleEnum = (UserRoleType)o;
            var userRole = _<UserRole>().FirstOrDefault(u => u.UserId == id && u.Role == roleEnum);
            if (userRole == null)
                userRole = new UserRole() { Role = roleEnum, UserId = id };
            if (userRole.Deleted == true)
                userRole.Deleted = false;
            await Save(userRole);
        }

        public async Task RemoveRole(int id, string role)
        {
            if (!Enum.TryParse(typeof(UserRoleType), role, out var o))
                throw new ArgumentException("Invalid role. Please check and try again");
            var roleEnum = (UserRoleType)o;
            var userRole = _<UserRole>().FirstOrDefault(u => u.UserId == id && u.Role == roleEnum);
            if (userRole == null)
                return;
            if (userRole.Deleted == false)
                userRole.Deleted = true;
            await Save(userRole);
        }

        private readonly IEntityContextRepository<IEntityContext> _league;

        public RegistrationService(ILifetimeScope scope) : base(scope, ContextNames.World)
        {
            _eco = base.GetRepository(ContextNames.World);
            _league = base.GetRepository(ContextNames.League);

        }

        public async Task<Registration> GetOrCreateRegistration(string userPlatformId, string username, ConnectionType connectionType,
            string firstName = "", string lastName = "", string email = "", string avatar = "",
            string token = "", string refresh = "")
        {
            Registration registration = null;
            var connection = _<Connection>().FirstOrDefault(c => c.Type == connectionType && c.ConnectionKey == userPlatformId);
            if (connection == null || connection.Deleted)
            {

                registration = new Registration() { Key = userPlatformId, Name = username, Step = RegistrationStepType.Started };

                User user = new User() { Username = username, Connections = new List<Connection>(), EmailId = email, ProfilePicUrl = avatar };

                user.Connections.Add(new Connection()
                {
                    Id = connection == null ? 0 : connection.Id,
                    ConnectionKey = userPlatformId,
                    Type = connectionType,
                    UserName = username,
                    Token = token,
                    RefreshToken = refresh
                });
                user.Registration = registration;
                user.Roles.Add(new UserRole() { Role = UserRoleType.Player });
                await Save(user);

            }
            else
            {
                registration = _<Registration>("User.Connections").FirstOrDefault(r => r.UserId == connection.UserId);
            }
            return registration;
        }



        public async Task<User> HandleLoginRequest(User user)
        {
            //create or search user by the connection
            if (user.Id == 0)
            {

                return await SearchOrCreateRegistration(user);
            }
            else
            {

                user = await HandleConnectionRequest(user);
                return user;
            }
        }

        public async Task<User> SearchOrCreateRegistration(User user)
        {
            var connection = user.Connections.First();
            var registration = await GetRegistration(connection.ConnectionKey, user.Username, email: user.EmailId,
                token: connection.Token, refresh: connection.RefreshToken, picture: user.ProfilePicUrl, type: connection.Type);
            return _<User>(registration.UserId, "Registration", "Roles", "Connections");
        }

        public async Task<User> SearchOrCreateRegistration(string platformUserId, string userName,
            ConnectionType type, string picture = "", bool forceMerge = false)
        {
            //check if another user has the same connection. 
            var connection = _<Connection>().FirstOrDefault(c => c.ConnectionKey == platformUserId &&
                                                                 c.Type == type);

            var registration = await GetRegistration(platformUserId, userName, type: type, picture: picture);
            return _<User>(registration.UserId, "Registration", "Roles", "Connections");
        }



        private async Task<Registration> GetRegistration(string platformUserId, string username, ConnectionType type, string email = "",
            string token = "", string refresh = "", string picture = "")
        {
            Registration registration = null;
            List<Connection> connections = null;
            User currentUser = _<User>("Registration", "Connections")
                .FirstOrDefault(u => u.Connections.Any(c => c.Type == type && c.ConnectionKey == platformUserId));
            if (currentUser != null)
                registration = currentUser.Registration;
            if (registration == null)
            {
                registration = new Registration() { Key = platformUserId, Name = username, Step = RegistrationStepType.Started };
                var userid = _<Connection>().FirstOrDefault(c => c.ConnectionKey == platformUserId && c.Type == type)?.UserId;
                if (userid == null)
                {
                    User user = new User() { Username = username, Connections = new List<Connection>(), EmailId = email };
                    user.Connections.Add(new Connection()
                    {
                        ConnectionKey = platformUserId,
                        Type = type,
                        UserName = username,
                        Token = token,
                        RefreshToken = refresh,
                        ProfilePicUrl = picture,
                        PartnerId = Partner.Id
                    });
                    user.PartnerId = Partner.Id;
                    registration.PartnerId = Partner.Id;
                    user.Registration = registration;
                    user.Roles.Add(new UserRole() { Role = UserRoleType.Player, PartnerId = Partner.Id });
                    if (string.IsNullOrEmpty(user.ProfilePicUrl))
                        user.ProfilePicUrl = picture;
                    await Save(user);
                    registration.UserId = user.Id;
                    registration.User = user;
                }
                else
                {
                    registration.UserId = userid.Value;
                    var user = _<User>(registration.UserId, "Connections", "Roles");
                    user.Registration = registration;

                    var existingConnection = user.Connections.FirstOrDefault(c => c.Type == type && c.ConnectionKey == platformUserId);
                    if (existingConnection != null)
                    {
                        existingConnection.Deleted = false;
                        if (!string.IsNullOrEmpty(token))
                            existingConnection.Token = token;
                        if (!string.IsNullOrEmpty(refresh))
                            existingConnection.RefreshToken = refresh;
                        if (!string.IsNullOrEmpty(username))
                            existingConnection.UserName = username;
                        if (!string.IsNullOrEmpty(picture))
                            existingConnection.ProfilePicUrl = picture;
                    }
                    else
                        user.Connections.Add(new Connection()
                        {
                            ConnectionKey = platformUserId,
                            Type = type,
                            UserName = username,
                            Token = token,
                            RefreshToken = refresh
                        });
                    if (string.IsNullOrEmpty(user.ProfilePicUrl))
                        user.ProfilePicUrl = picture;
                    await Save(user);
                    registration.User = user;
                    registration.UserId = user.Id;
                }

            }
            else
            {

                var user = currentUser;
                user.Username = username;

                Connection connection = null;
                if (user.Connections.Any(c => c.Type == type && c.ConnectionKey == platformUserId))
                {
                    connection = user.Connections.First(c => c.Type == type && c.ConnectionKey == platformUserId);
                    if (!string.IsNullOrEmpty(token))
                        connection.Token = token;
                    if (!string.IsNullOrEmpty(refresh))
                        connection.RefreshToken = refresh;
                    if (!string.IsNullOrEmpty(username))
                        connection.UserName = username;
                    if (!string.IsNullOrEmpty(picture))
                        connection.ProfilePicUrl = picture;
                    connection.Deleted = false;
                }
                else
                {
                    connection = new Connection()
                    {
                        ConnectionKey = platformUserId,
                        Type = ConnectionType.Discord,
                        UserName = username,
                        Token = token,
                        UserId = registration.UserId,
                        RefreshToken = refresh,
                        ProfilePicUrl = picture
                    };
                    user.Connections.Add(connection);
                }
                if (email?.Length > 0)
                    user.EmailId = email;
                if (string.IsNullOrEmpty(user.ProfilePicUrl))
                    user.ProfilePicUrl = picture;
                await GetNew<RegistrationService>().Save(user);
            }
            if (currentUser == null)
                currentUser = _<User>(registration.UserId);
            try
            {
                //await GetNew<ZulipService>().GetOrCreateConnection(currentUser);
            }
            catch (Exception x)
            {
                Log.Error("Could not add zulip chat user for the user {u} due to {e}", currentUser.Id, x.Message);
            }
            return registration;
        }


        public async Task<User> HandleConnectionRequest(User user)
        {
            var loggedInUser = _<User>(user.Id, "Registration", "Connections");
            var registration = loggedInUser.Registration;
            var connections = loggedInUser.Connections;
            var loggedInConnection = user.Connections.First();
            //connection exists in the same user. Need to update connection and login with original user
            if (connections.Any(c => c.ConnectionKey == loggedInConnection.ConnectionKey
            && c.Type == loggedInConnection.Type))
            {
                //connection exists.
                var connection = connections.First(c => c.ConnectionKey == loggedInConnection.ConnectionKey
                                                    && c.Type == loggedInConnection.Type);


                //Modify some values and move

                if (connection.Token.IsNotEqualTo(loggedInConnection.Token, allowEmpty: false))
                    connection.Token = loggedInConnection.Token;
                if (connection.RefreshToken.IsNotEqualTo(loggedInConnection.RefreshToken, allowEmpty: false))
                    connection.RefreshToken = loggedInConnection.RefreshToken;
                if (connection.UserName.IsNotEqualTo(loggedInConnection.UserName, allowEmpty: false))
                    connection.UserName = loggedInConnection.UserName;
                if (connection.ProfilePicUrl.IsNotEqualTo(user.ProfilePicUrl, allowEmpty: false))
                    connection.ProfilePicUrl = user.ProfilePicUrl;
                if (connection.Key.IsNotEqualTo(loggedInConnection.Key, allowEmpty: false))
                    connection.Key = loggedInConnection.Key;
                connection.Deleted = false;
                await Save(connection);

            }
            else
            {
                //if the loggedInConnection belongs to some other user, then throw
                await HandleNewConnectionOnDevice(loggedInConnection, loggedInUser);

            }
            if (string.IsNullOrEmpty(loggedInUser.ProfilePicUrl))
            {
                loggedInUser.ProfilePicUrl = user.ProfilePicUrl;
                await Save(loggedInUser);
            }
            return _<User>(user.Id, "Registration", "Connections"); ;
        }

        private async Task HandleNewConnectionOnDevice(Connection loggedInConnection, User loggedInUser)
        {

            var usersWithConnection = _<User>("Connections").Where(u => u.Connections
            .Any(c => c.ConnectionKey == loggedInConnection.ConnectionKey &&
            c.Type == loggedInConnection.Type && !c.Deleted)).ToList();
            //this is a fresh connection, add to logged in user
            if (usersWithConnection.Count == 0)
            {
                loggedInConnection.UserId = loggedInUser.Id;
                loggedInConnection.ProfilePicUrl = loggedInUser.ProfilePicUrl;
                await Save(loggedInConnection);
                loggedInUser.Connections.Add(loggedInConnection);
            }
            //connection exists for different users
            else
            {
                if (usersWithConnection.Any(u => u.Id == loggedInUser.Id))
                {
                    foreach (var user in usersWithConnection)
                    {
                        ;//await MergeUsers(user, loggedInUser);
                    }
                }
                else
                {

                    //do error handling for this
                }
            }

        }

        public async Task AddPlayer(int userId, Player player)
        {
            await Get<PlayerService>().AddPlayer(player);
        }

        public async Task UpsertRegistration(Registration registration)
        {
            _eco.Upsert(registration);
            await _eco.SaveChanges();
        }

        private bool IsPhoneNumber(string input)
        {
            return Regex.IsMatch(input, @"^[\d]{10}$");
        }
        private bool IsEmail(string input)
        {
            return Regex.IsMatch(input, @"^([\w]+)@([\w]+).([\w]+)$");
        }



        private bool IsUpi(string input)
        {
            return Regex.IsMatch(input, @"^([\w]+)@([\w]+)$");
        }

        private bool VerifyPaymentOption(string input, Registration registration)
        {
            if (Registration.PaymentOptions.Any(p => p.ToLower() == input.ToLower()))
                return true;
            return false;
        }

        public async Task<Player> VerifyProfilePicFromStream(Stream input, int userId, int gameId)
        {
            var gameService = Get<GameService>();
            Player player = new Player() { GameId = gameId, ImageStream = input };
            player = await gameService.VerifyPlayer(player);
            player.GameId = gameId;
            return player;
        }






        public void SetClaims(ClaimsIdentity identity, User user)
        {
            identity.AddClaim(new Claim("authorized", "yes"));
            foreach (var role in user.Roles.Where(r => r.Deleted == false))
            {
                identity.AddClaim(new Claim("role", role.Role.ToString()));

            }

            if (user.ProfilePicUrl == null)
                user.ProfilePicUrl = user.Connections?.FirstOrDefault(c => c.ProfilePicUrl != null)?.ProfilePicUrl;
            if (user.ProfilePicUrl != null)
                identity.AddClaim(new Claim("Avatar", user.ProfilePicUrl));
            identity.AddClaim(new Claim("UserId", user.Id.ToString()));

        }



        public async Task<User> HandleVerification(User user, int refererUserId = 0)
        {
            var loggedInConnection = user.Connections.First();
            if (loggedInConnection.Type != ConnectionType.Whatsapp)
                throw new Exception("This function can only be called for whatsapp verification");
            var existingConnection = _<Connection>().FirstOrDefault(c => c.ConnectionKey == loggedInConnection.ConnectionKey &&
                c.Type == ConnectionType.Whatsapp && !c.Deleted);
            User existingUser = null;

            if (existingConnection != null)
            {
                existingUser = _<User>(existingConnection.UserId, "Connections");
            }

            //new verification
            if (existingUser == null)
            {
                loggedInConnection.UserId = user.Id;

                loggedInConnection.ProfilePicUrl = user.ProfilePicUrl;
                await Save(loggedInConnection);
                await Get<VirtualCurrencyService>().AwardJoiningVirtualCurrency(user.Id, refererUserId);
                var verifiedUser = _<User>(user.Id, "Connections", "Registration");

                return verifiedUser;
            }
            //already verified same user
            if (user.Id == existingUser?.Id)
            {
                return await HandleConnectionRequest(user);
            }
            //already verified different user
            //throw

            else
            {
                throw new Exception($"{existingUser.VerifiedNumber} is already verified by a different account. You have to provide a different number to verify");
                //loggedInConnection.UserId = user.Id;

                //loggedInConnection.ProfilePicUrl = user.AvatarUrl;
                //await Save(loggedInConnection);

                //var existingConnection = existingUser.Connections.FirstOrDefault(c => c.ConnectionKey == user.Connections.First().ConnectionKey);
                //existingConnection.Deleted = true;
                //existingConnection.MergedWithConnectionId = loggedInConnection.Id;
                //await Save(existingConnection);
                //existingUser.VerifiedNumber = "";
                //await Save(existingUser);
            }
            return _<User>(user.Id, "Connections", "Registration");
        }

        public async Task UpdateProfilePic(User user)
        {
            //var user = _<User>(userid, "Connections");
            if (string.IsNullOrEmpty(user.ProfilePicUrl))
            {
                var connections = user.Connections;
                if (connections.IsNullOrEmpty())
                    connections = _<Connection>().Where(c => c.UserId == user.Id).OrderByDescending(c => c.Id).ToList();
                foreach (var connection in connections)
                {
                    if (string.IsNullOrEmpty(connection.ProfilePicUrl))
                    {
                        continue;
                    }
                    user.ProfilePicUrl = connection.ProfilePicUrl;
                    await Save(user);
                    break;
                }
            }
            user.ProfilePicUrl = user.ProfilePicUrl;
        }


    }

    public class ReferralDataRecord
    {
        public int ReferralJoinees { get; internal set; }
        public int ReferralParticipants { get; internal set; }
        public int ReferralResults { get; internal set; }
    }
}
