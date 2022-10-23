using Microsoft.EntityFrameworkCore;

namespace OrderBot.Core
{
    /// <summary>
    /// The Entity Framework context used to interact with the database.
    /// </summary>
    /// <remarks>
    /// Useful: https://learn.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#many-to-many
    /// </remarks>
    public class OrderBotDbContext : DbContext
    {
        /// <summary>
        /// Create a new <see cref="OrderBotDbContext"/>.
        /// </summary>
        /// <param name="dbContextOptions">
        /// Configuration options.
        /// </param>
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
        public DbSet<Carrier> Carriers { get; protected set; } = null!;
        public DbSet<DiscordGuildMinorFaction> DiscordGuildMinorFactions { get; protected set; } = null!;

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

                entity.Property(e => e.Name);

                entity.Property(e => e.CarrierMovementChannel);
            });

            modelBuilder.Entity<DiscordGuild>()
                        .HasMany(e => e.SupportedMinorFactions)
                        .WithMany(e => e.SupportedBy)
                        .UsingEntity<DiscordGuildMinorFaction>();

            modelBuilder.Entity<DiscordGuildStarSystemMinorFactionGoal>(entity =>
            {
                entity.ToTable("DiscordGuildStarSystemMinorFactionGoal");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Goal);
            });

            modelBuilder.Entity<DiscordGuildStarSystemMinorFactionGoal>()
                        .HasOne(e => e.DiscordGuild)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<DiscordGuildStarSystemMinorFactionGoal>()
                        .HasOne(e => e.StarSystemMinorFaction)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Carrier>(entity =>
            {
                entity.ToTable("Carrier");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Name)
                      .IsRequired();

                entity.Property(e => e.SerialNumber)
                      .IsRequired();

                entity.Property(e => e.Owner);

                entity.Property(e => e.FirstSeen);
            });

            modelBuilder.Entity<Carrier>()
                        .HasOne(e => e.StarSystem)
                        .WithMany();

            modelBuilder.Entity<IgnoredCarrier>(entity =>
            {
                entity.ToTable("IgnoredCarrier");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();
            });

            modelBuilder.Entity<IgnoredCarrier>()
                        .HasOne(e => e.DiscordGuild)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Carrier>()
                        .HasMany(e => e.IgnoredBy)
                        .WithMany(e => e.IgnoredCarriers)
                        .UsingEntity<IgnoredCarrier>();

            modelBuilder.Entity<DiscordGuildMinorFaction>(entity =>
            {
                entity.ToTable("DiscordGuildMinorFaction");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();
            });

            modelBuilder.Entity<DiscordGuildMinorFaction>()
                        .HasOne(e => e.DiscordGuild)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<DiscordGuildMinorFaction>()
                        .HasOne(e => e.MinorFaction)
                        .WithMany()
                        .IsRequired();

            // May need to configure correct pluralization of many-to-many field names.
            // Possibly related to https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/whatsnew#less-configuration-for-many-to-many-relationships.
        }
    }
}
