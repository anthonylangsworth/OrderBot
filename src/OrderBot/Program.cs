using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderBot;
using OrderBot.Core;
using OrderBot.MessageProcessors;

const string environmentVariablePrefix = ""; // "OB__"
const string databaseEnvironmentVariable = "OrderBot";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddEnvironmentVariables(environmentVariablePrefix))
    .ConfigureServices((hostContext, services) =>
    {
        string dbConnectionString = hostContext.Configuration.GetConnectionString(databaseEnvironmentVariable);
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new InvalidOperationException(
                $"Database connection string missing from environment variable `{environmentVariablePrefix}ConnectionStrings__{databaseEnvironmentVariable}`. " +
                "Usually in the form of `Server=server;Database=OrderBot;User ID=OrderBot;Password=password`.");
        }
        services.AddDbContextFactory<OrderBotDbContext>(
            dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(dbConnectionString)); // "Server=localhost;Database=OrderBot;User ID=OrderBot;Password=password"
                                                                                                  // options => options.EnableRetryOnFailure()));
        services.AddHostedService<EddnMessageBackgroundService>();
        services.AddSingleton<MinorFactionNameFilter, FixedMinorFactionNameFilter>(sp => new FixedMinorFactionNameFilter(new[] { "EDA Kunti League" }));
        services.AddSingleton<EddnMessageProcessor, SystemMinorFactionMessageProcessor>();
    })
    .Build();

await host.RunAsync();