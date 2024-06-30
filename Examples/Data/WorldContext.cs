using Horde.Core.Domains.World.Entities;
using Horde.Core.Interfaces.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataContexts
{
    public class WorldContext : EfCoreContext
    {

        //public EcosystemContext(ILifetimeScope scope) : base(scope)
        //{
            
        //}
        //public EcosystemContext() : base() { }
        public WorldContext(DbContextOptions options) : base(options) { }

        public override ContextNames Name => ContextNames.World;
        public DbSet<User> Users { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<UserConfiguration> UserConfigurations { get; set; }
        public DbSet<Tribe> Tribes { get; set; }
        public DbSet<Clan> Clans { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Incentive> Incentives { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }

        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        public DbSet<Alliance> Alliances { get; set; }
        public DbSet<AllianceMember> AllianceMembers { get; set; }
        public DbSet<OrganizerTeam> OrganizerTeams { get; set; }
        public DbSet<OrganizerTeamMember> OrganizerTeamMembers { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<CastingRequest> CastingRequests { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Talk> Talks { get; set; }
        public DbSet<DirectConversation> DirectConversations { get; set; }
        public DbSet<Bounty> Bounties { get; set; }
        public DbSet<BountyHunter> BountyHunters { get; set; }
        public DbSet<BountyReward> Rewards { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyEmployee> Employees { get; set; }
        public DbSet<UserReview> Reviews { get; set; }
        public DbSet<InviteLanding> InviteLandings { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("world");

            SetQueryFilters(modelBuilder);
            modelBuilder.Entity<UserRole>()
                .HasIndex(r => new { r.UserId, r.Role })
                .IsUnique();
            modelBuilder.Entity<Connection>()
                .HasIndex(c => new { c.ConnectionKey, c.Type, c.PartnerId })
                .IsUnique();
            modelBuilder.Entity<Tribe>()
                .HasIndex(c => new { c.TribeIdentifier, c.Type, c.PartnerId })
                .IsUnique();
            modelBuilder.Entity<Member>()
                .HasIndex(c => new { c.TribeId, c.UserId })
                .IsUnique();
            modelBuilder.Entity<Registration>()
                .HasIndex(c => new { c.UserId })
                .IsUnique();
            modelBuilder.Entity<Alliance>().HasBaseType<Team>();
            modelBuilder.Entity<OrganizerTeam>().HasBaseType<Team>();
            modelBuilder.Entity<Company>().HasBaseType<Team>();
            modelBuilder.Entity<Company>().Property("Country").HasColumnName("Country");
            modelBuilder.Entity<Alliance>().Property("Country").HasColumnName("Country");

            modelBuilder.Entity<AllianceMember>().HasBaseType<TeamMember>();
            modelBuilder.Entity<OrganizerTeamMember>().HasBaseType<TeamMember>();
            modelBuilder.Entity<CompanyEmployee>().HasBaseType<TeamMember>();
            modelBuilder.Entity<CompanyEmployee>().Property("UserId").HasColumnName("UserId");
            modelBuilder.Entity<OrganizerTeamMember>().Property("UserId").HasColumnName("UserId");

            modelBuilder.Entity<CastingRequest>().HasBaseType<Request>();
            modelBuilder.Entity<Bounty>().HasBaseType<Request>();

            modelBuilder.Entity<Tribe>().ToTable("Tribes");//.HasQueryFilter(t => t.PartnerId == _tenantManager.GetTenant().Id);
            modelBuilder.Entity<Clan>().ToTable("Clans");
            modelBuilder.Entity<Group>().ToTable("Groups");
            base.OnModelCreating(modelBuilder);

        }
        
        private void SetQueryFilters(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<User>()
                .HasQueryFilter(x => x.PartnerId == id);            
            modelBuilder.Entity<Registration>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Connection>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<UserConfiguration>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Tribe>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Member>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<UserRole>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Incentive>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<SupportRequest>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Team>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<TeamMember>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Request>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Conversation>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Talk>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<BountyHunter>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<BountyReward>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<UserReview>()
                .HasQueryFilter(x => x.PartnerId == id);


        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }


    }
}
