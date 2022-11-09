using Microsoft.EntityFrameworkCore;
using OrderBot.Core;

namespace OrderBot.EntityFramework
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
        public DbSet<Presence> Presences { get; protected set; } = null!;
        public DbSet<DiscordGuild> DiscordGuilds { get; protected set; } = null!;
        public DbSet<DiscordGuildPresenceGoal> DiscordGuildPresenceGoals { get; protected set; } = null!;
        public DbSet<Carrier> Carriers { get; protected set; } = null!;
        public DbSet<DiscordGuildMinorFaction> DiscordGuildMinorFactions { get; protected set; } = null!;
        public DbSet<Conflict> Conflicts { get; protected set; } = null!;

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

                entity.Property(e => e.LastUpdated);
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

            modelBuilder.Entity<Presence>()
                        .HasOne(e => e.StarSystem)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Presence>(entity =>
            {
                entity.ToTable("Presence");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Influence);

                entity.Property(e => e.SecurityLevel)
                      .HasMaxLength(100);
            });

            modelBuilder.Entity<Presence>()
                        .HasOne(e => e.StarSystem)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Presence>()
                        .HasOne(e => e.MinorFaction)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Presence>()
                        .HasMany(e => e.States)
                        .WithMany(e => e.Presence);

            modelBuilder.Entity<DiscordGuild>(entity =>
            {
                entity.ToTable("DiscordGuild");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.GuildId)
                      .IsRequired();

                entity.Property(e => e.Name)
                      .HasMaxLength(100);

                entity.Property(e => e.CarrierMovementChannel);

                entity.Property(e => e.AuditChannel);
            });

            modelBuilder.Entity<DiscordGuild>()
                        .HasMany(e => e.SupportedMinorFactions)
                        .WithMany(e => e.SupportedBy)
                        .UsingEntity<DiscordGuildMinorFaction>();

            modelBuilder.Entity<DiscordGuildPresenceGoal>(entity =>
            {
                entity.ToTable("DiscordGuildPresenceGoal");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Goal)
                      .IsRequired();
            });

            modelBuilder.Entity<DiscordGuildPresenceGoal>()
                        .HasOne(e => e.DiscordGuild)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<DiscordGuildPresenceGoal>()
                        .HasOne(e => e.Presence)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Carrier>(entity =>
            {
                entity.ToTable("Carrier");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.SerialNumber)
                      .HasMaxLength(7)
                      .IsRequired();

                entity.Property(e => e.Owner)
                      .HasMaxLength(100);

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

            modelBuilder.Entity<Conflict>(entity =>
            {
                entity.ToTable("Conflict");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.MinorFaction1WonDays);

                entity.Property(e => e.MinorFaction2WonDays);

                entity.Property(e => e.Status)
                      .HasMaxLength(100);

                entity.Property(e => e.WarType)
                      .HasMaxLength(100);
            });

            modelBuilder.Entity<Conflict>()
                        .HasOne(e => e.StarSystem)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Conflict>()
                        .HasOne(e => e.MinorFaction1)
                        .WithMany()
                        .IsRequired();

            modelBuilder.Entity<Conflict>()
                        .HasOne(e => e.MinorFaction2)
                        .WithMany()
                        .IsRequired();

            // May need to configure correct pluralization of many-to-many field names.
            // Possibly related to https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/whatsnew#less-configuration-for-many-to-many-relationships.
        }
    }
}
