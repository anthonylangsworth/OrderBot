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
        public OrderBotDbContext()
        {
            // Do nothing
        }

        //public OrderBotDbContext(DbContextOptions<OrderBotDbContext> dbContextOptions)
        //    : base(dbContextOptions)
        //{
        //    // Do nothing
        //}

        public DbSet<SystemMinorFaction> SystemMinorFaction { get; protected set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // TODO: Load from environment variable or configuration
            optionsBuilder.UseSqlServer(
                @"Server=localhost;Database=OrderBot;User ID=OrderBot;Password=password");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemMinorFaction>(entity =>
            {
                //entity.Property(e => e.ID)
                //      .UseIdentityColumn();

                entity.Property(e => e.StarSystem)
                      .HasColumnName("System")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.MinorFaction)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.LastUpdated)
                      .IsRequired();
            });

            modelBuilder.Entity<SystemMinorFaction>()
                        .HasMany(e => e.States);
        }
    }
}
