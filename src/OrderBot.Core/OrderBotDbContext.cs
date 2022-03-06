using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core
{
    public class OrderBotDbContext: DbContext
    {
        public OrderBotDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
            // Do nothing
        }

        public DbSet<StarSystem> StarSystems { get; protected set; } = null!;
        public DbSet<MinorFaction> MinorFactions { get; protected set; } = null!;
        public DbSet<State> State { get; protected set; } = null!;
        public DbSet<StarSystemMinorFaction> SystemMinorFactions { get; protected set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StarSystem>(entity =>
            {
                entity.ToTable("StarSystem");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.HasKey(e => e.Id);

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

                entity.HasKey(e => e.Id);   

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.ToTable("State");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();
            });

            modelBuilder.Entity<StarSystemMinorFaction>(entity =>
            {
                entity.ToTable("StarSystemMinorFaction");

                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.HasKey(e  => e.Id);

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
                        .WithMany(e => e.StarSystemMinorFactions)
                        .UsingEntity<StarSystemMinorFactionState>();
        }
    }
}
