# Development

## Setup
This project is currently hosted in Azure. Run the [Deploy](../../../actions/workflows/deploy.yml) action to update the deployed version.

To setup locally:
1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop/).
2. Create the database container:
    1. Download the SQL Server instance using `docker pull mcr.microsoft.com/mssql/server:2019-latest`
    2. Create and run a new SQL Server container using `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<password>" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest`, substituting `<password>` with a strong password.
    3. Set the password for the `OrderBot` in `deploy/db.sql` login around line 7. 
    4. Run `deploy/db.sql`, such as via SQL Management Studio, to create the database.
    5. Run `deploy/tables.sql` to create the table structure.
3. Create the application container:
    1. Download a base image for the application using `docker pull mcr.microsoft.com/dotnet/runtime:6.0`.
    2. Create a file calked "ApiKeys.env" in `src/OrderBot`. Create four enties inside it:
        1. `ConnectionStrings__OrderBot`, containing the SQL server connection string.
        2. `DiscordApiKey`, containing the Discord Bot's API key.
        3. `LogAnalytics_WorkspaceId`, containing an Azure LogAnalytics workspace ID.
        4. `LogAnalytics_WorkspaceKey`, containing an Azure LogAnalytics primary key.
4. Download, build and run the code.

## Writing Discord Commands

`Client_InteractionCreated` in [BotHostedService](../../../tree/main/src/OrderBot/Discord/BotHostedService.cs) provides the following:
1. Creates a `IServiceScope` so scoped DI services can be returned and cleaned up.
2. Adds a logging scope with common details such as the user, guild and command details.
3. Acknowledges non-autocomplete requests using `DeferAsync`. This ensures long running commands do not time out withing three seconds. 
4. Logs a "Completed Successfully" message if the command does not throw any exceptions.
5. Shows an access denied-style error messages for unmet preconditions.
6. Responds to the user with the ephemeral contents of the `Message` property for thrown `DiscordUserInteractionExceptions`.
7. Logs details of other thrown exceptions.

Best practice for writing slash (application) commands:
1. Do not duplicate work in `BotHostedService.Client_InteractionCreated`. The general goal is to move as much work to there as possible. This standardizes behaviour and prevents code repetition.
2. Throw a `DiscordUserInteractionException` to represent a user-relevent and -solvable error, with the error message in the Message property. The error message can contain Discord markdown. 
3. Throw a different, appropriate exception for other errors.
4. Acknowledge success using an ephemeral message.
5. For success and error messages:
    1. Include `**Success**` or `**Error**` at the start to clearly indicate whether the command worked or did not.
    2. For errors, describe (1) why the error occured, (2) the resulting state and (3) how to fix or remedy.
6. Use `TransactionScope` around any database work, remembering to call `Complete()` at the end.
7. Log any modifications using an `IAuditLogger`.

## References
1. Using Docker with .Net Core: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/visual-studio-tools-for-docker?view=aspnetcore-6.0
2. Github action to build SQL server database: https://github.com/ankane/setup-sqlserver
3. Discord.Net documentation: https://discordnet.dev/
4. Using Log Analytics with Container Instances: https://learn.microsoft.com/en-us/azure/container-instances/container-instances-log-analytics
5. CsvHelper quickstart: https://joshclose.github.io/CsvHelper/getting-started/
