using Microsoft.EntityFrameworkCore;

namespace OrderBot.Core
{
    public class OrderBotDbContext : DbContext
    {
        public OrderBotDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
            // Do nothing
        }

        public DbSet<StarSystem> StarSystems { get; protected set; } = null!;
        public DbSet<MinorFaction> MinorFactions { get; protected set; } = null!;
        public DbSet<State> States { get; protected set; } = null!;
        public DbSet<StarSystemMinorFaction> StarSystemMinorFactions { get; protected set; } = null!;
        public DbSet<DiscordGuild> DiscordGuilds { get; protected set; } = null!;
        public DbSet<DiscordGuildStarSystemMinorFactionGoal> DiscordGuildStarSystemMinorFactionGoals { get; protected set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StarSystem>(entity =>
            {
                entity.ToTable("StarSystem");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.LastUpdated)
                      .IsRequired();
            });

            modelBuilder.Entity<MinorFaction>(entity =>
            {
                entity.ToTable("MinorFaction");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.ToTable("State");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();
            });

            modelBuilder.Entity<StarSystemMinorFaction>(entity =>
            {
                entity.ToTable("StarSystemMinorFaction");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Influence);
            });

            modelBuilder.Entity<StarSystemMinorFaction>()
                        .HasOne(e => e.StarSystem)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<StarSystemMinorFaction>()
                        .HasOne(e => e.MinorFaction)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<StarSystemMinorFaction>()
                        .HasMany(e => e.States)
                        .WithMany(e => e.StarSystemMinorFactions);

            modelBuilder.Entity<DiscordGuild>(entity =>
            {
                entity.ToTable("DiscordGuild");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.GuildId)
                      .IsRequired();
            });

            modelBuilder.Entity<DiscordGuildStarSystemMinorFactionGoal>(entity =>
            {
                entity.ToTable("DiscordGuildStarSystemMinorFactionGoal");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Goal);
            });

            modelBuilder.Entity<DiscordGuildStarSystemMinorFactionGoal>()
                        .HasOne(e => e.DiscordGuild)
                        .WithMany();

            modelBuilder.Entity<DiscordGuildStarSystemMinorFactionGoal>()
                        .HasOne(e => e.StarSystemMinorFaction)
                        .WithMany();

            // May need to configure correct pluralization of many-to-many field names.
            // Possibly related to https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/whatsnew#less-configuration-for-many-to-many-relationships.
        }
    }
}
