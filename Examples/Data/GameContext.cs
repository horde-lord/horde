using Horde.Core.Domains.Games.Entities;
using Horde.Core.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Examples.Data
{
    public class GameContext : EfCoreContext
    {

        //public GameContext(ILifetimeScope scope) : base(scope) { }
        //public GameContext(DbContextOptions options) : base(options) { }
        public GameContext(IConfiguration configuration) : base(configuration) { }
        public override ContextNames Name => ContextNames.Game;
        public DbSet<Game> Games { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Alias> Aliases { get; set; }
        public DbSet<GameMode> Modes { get; set; }
        public DbSet<GameModeConfiguration> Configurations { get; set; }
        public DbSet<GamePlayStep> PlaySteps { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("game");
            

            modelBuilder.Entity<Player>()
                .HasIndex(g => new { g.UserId, g.GameId })
                .IsUnique();

            SetQueryFilters(modelBuilder);
            

        }
        
        private void SetQueryFilters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()/*.HasQueryFilter(x => x.PartnerId == id)*/;
            modelBuilder.Entity<Squad>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Player>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Alias>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<GameMode>()/*.HasQueryFilter(x => x.PartnerId == id)*/;
            modelBuilder.Entity<GameModeConfiguration>()/*.HasQueryFilter(x => x.PartnerId == id)*/;
            modelBuilder.Entity<GamePlayStep>().HasQueryFilter(x => x.PartnerId == id);
            modelBuilder.Entity<Category>()/*.HasQueryFilter(x => x.PartnerId == id)*/;
            modelBuilder.Entity<Tag>()/*.HasQueryFilter(x => x.PartnerId == id)*/;

        }
    }



}
