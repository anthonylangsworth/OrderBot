using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace OrderBot.Core.Test
{
    public class OrderBotDbContextFactory : IDbContextFactory<OrderBotDbContext>, IDisposable
    {
        private bool disposedValue;

        public OrderBotDbContextFactory(bool useInMemory = true)
        {
            // Share the same connection to enable transactions
            SqlConnection = new(@"Server=localhost;Database=OrderBot;User ID=OrderBot;Password=password"); 
            DbContextOptionsBuilder<OrderBotDbContext> optionsBuilder = new DbContextOptionsBuilder<OrderBotDbContext>();
            DbContextOptions = (useInMemory
                ? optionsBuilder.UseInMemoryDatabase("OrderBot").ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                : optionsBuilder.UseSqlServer(SqlConnection)).Options;
        }

        public DbContextOptions DbContextOptions { get; }

        public SqlConnection SqlConnection { get; }

        public IDbContextTransaction BeginTransaction()
        {
            return CreateDbContext().Database.BeginTransaction();
        }

        public OrderBotDbContext CreateDbContext()
        {
            return new OrderBotDbContext(DbContextOptions);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SqlConnection.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}