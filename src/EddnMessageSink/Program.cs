using EddnMessageProcessor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderBot.Core;

const string environmentVariablePrefix = "OB__";
const string databaseEnvironmentVariable = "OrderBot";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddEnvironmentVariables(environmentVariablePrefix))
    .ConfigureServices((hostContext, services) =>
    {
        string dbConnectionString = hostContext.Configuration.GetConnectionString(databaseEnvironmentVariable);
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new InvalidOperationException(
                $"Database connection string missing from environment variable `{environmentVariablePrefix}ConnectionString__{databaseEnvironmentVariable}`. " +
                "Usually in the form of `Server=server;Database=OrderBot;User ID=OrderBot;Password=password`.");
        }
        services.AddDbContextFactory<OrderBotDbContext>(
            dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(dbConnectionString)); // "Server=localhost;Database=OrderBot;User ID=OrderBot;Password=password"
                                                                                                  // options => options.EnableRetryOnFailure()));
        services.AddHostedService<EddnMessageBackgroundService>();
        services.AddSingleton<MinorFactionsSource, FixedMinorFactionsSource>(sp => new FixedMinorFactionsSource(new HashSet<string>(new[] { "EDA Kunti League" })));
        services.AddSingleton<EddnMessageProcessor.EddnMessageProcessor, OrderBotMessageProcessor>();
    })
    .Build();

await host.RunAsync();