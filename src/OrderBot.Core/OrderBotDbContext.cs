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

        public DbSet<SystemMinorFaction> SystemMinorFaction { get; protected set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemMinorFaction>(entity =>
            {
                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.StarSystem)
                      .HasColumnName("System")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Influence);

                entity.Property(e => e.MinorFaction)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.LastUpdated)
                      .IsRequired();
            });

            modelBuilder.Entity<SystemMinorFaction>()
                        .HasKey(e => e.Id);

            modelBuilder.Entity<SystemMinorFactionState>(entity =>
            {
                entity.Property(e => e.Id)
                      .UseIdentityColumn();

                entity.Property(e => e.State)
                      .HasMaxLength(100)
                      .IsRequired();
            });

            modelBuilder.Entity<SystemMinorFactionState>()
                        .HasKey(e => e.Id);

            modelBuilder.Entity<SystemMinorFaction>()
                        .HasMany(e => e.States)
                        .WithOne()
                        .IsRequired();// e => e.SystemMinorFaction
                        // .HasForeignKey("SytemMinorFactionID");

            //modelBuilder.Entity<SystemMinorFaction>()
            //    .Navigation(b => b.States)
            //    .UsePropertyAccessMode(Prope rtyAccessMode.Property);
        }
    }
}
