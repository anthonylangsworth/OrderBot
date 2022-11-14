using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderBot.EntityFramework;

internal static class EntityFrameworkExtensions
{
    internal const string DatabaseEnvironmentVariable = "OrderBot";

    /// <summary>
    /// Add the sercices for a Discord Bot.
    /// </summary>
    /// <param name="services">
    /// Add services to this collection.
    /// </param>
    /// <param name="configuration">
    /// Configuration source.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Configuration is missing or invalid.
    /// </exception>
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string dbConnectionString = configuration.GetConnectionString(DatabaseEnvironmentVariable);
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new InvalidOperationException(
                $"Database connection string missing from environment variable `ConnectionStrings__{DatabaseEnvironmentVariable}`. " +
                "Usually in the form of `Server=server;Database=OrderBot;User ID=OrderBot;Password=password`.");
        }
        services.AddDbContextFactory<OrderBotDbContext>(
            dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(dbConnectionString));
        services.AddDbContext<OrderBotDbContext>(
            dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(dbConnectionString));
    }
}
