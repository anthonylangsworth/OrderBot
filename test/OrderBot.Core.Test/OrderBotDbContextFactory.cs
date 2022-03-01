using Microsoft.EntityFrameworkCore;

namespace OrderBot.Core.Test
{
    public class OrderBotDbContextFactory : IDbContextFactory<OrderBotDbContext>
    {
        public OrderBotDbContextFactory(bool useInMemory = true)
        {
            UseInMemory = useInMemory;
        }

        public bool UseInMemory { get; }

        public OrderBotDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<OrderBotDbContext> optionsBuilder = new DbContextOptionsBuilder<OrderBotDbContext>();
            return UseInMemory
                ? new OrderBotDbContext(optionsBuilder.UseInMemoryDatabase("OrderBot").Options)
                : new OrderBotDbContext(optionsBuilder.UseSqlServer(@"Server=localhost;Database=OrderBot;User ID=OrderBot;Password=password").Options);
        }
    }
}