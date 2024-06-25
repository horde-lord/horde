using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{
    public class UserRole : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public UserRoleType Role { get; set; }
        public RoleLevelType Level { get; set; }



        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;
    }
}

namespace Horde.Core
{
    public enum UserRoleType
    {
        /// <summary>
        /// Every user is a player by default
        /// </summary>
        Player = 0,
        TribeAdmin = 1,
        [Obsolete]
        TribalOrganizer = 2,
        GameOwner = 3,
        Caster = 4,
        TribalMod = 5,
        God = 6,
        [Obsolete]
        TribalPlayer = 7,
        /// <summary>
        /// A member of the tenant team and can see the partner tab
        /// </summary>
        TenantTeamMember = 8,
        /// <summary>
        /// Has all the roles and accesses. Can grant access to others
        /// </summary>
        TenantAdmin = 9,
        /// <summary>
        /// Can manage a feature.
        /// </summary>
        TenantFeatureManager = 10,
    }

    public enum RoleLevelType
    {
        Standard = 0,
        Premium = 1,
        VIP = 2,
        Lieutenant = 3,
        General = 4,
        Suspend = 5
    }
}