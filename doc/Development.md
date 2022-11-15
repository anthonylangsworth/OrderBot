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

## References
1. Using Docker with .Net Core: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/visual-studio-tools-for-docker?view=aspnetcore-6.0
2. Github action to build SQL server database: https://github.com/ankane/setup-sqlserver
3. Discord.Net documentation: https://discordnet.dev/
4. Using Log Analytics with Container Instances: https://learn.microsoft.com/en-us/azure/container-instances/container-instances-log-analytics
5. CsvHelper quickstart: https://joshclose.github.io/CsvHelper/getting-started/
