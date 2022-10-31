# Development

## Setup
This project is currently hosted in Azure. Run the [Deploy](../../../actions/workflows/deploy.yml) action to update the deployed version.

To setup locally:
1. Install Docker Desktop.
2. Download the SQL Server instance using `docker pull mcr.microsoft.com/mssql/server:2019-latest`
3. Create and run a new SQL Server container using `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<password>" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest`, substituting `<password>` with a strong password.
4. Set a password for the "OrderBot" user around line 60 in `deploy/db.sql` then run that SQL command to create the database, such as via SQL Management Studio.
5. Run `deploy/tables.sql` to create the table structure.
6. Download a base image for the applications using `docker pull mcr.microsoft.com/dotnet/runtime:6.0`.
7. Create a file calked "ApiKeys.env" in `src/OrderBot`. Create two enties, one for `ConnectionStrings__OrderBot`, containing the SQL server connection string, and `DiscordApiKey`, containing the Discord Bot's API key.
8. Download, build and run the code.

## References
1. Using Docker with .Net Core: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/visual-studio-tools-for-docker?view=aspnetcore-6.0
2. Github action to build SQL server database: https://github.com/ankane/setup-sqlserver
3. Discord.Net documentation: https://discordnet.dev/
4. Using Log Analytics with Container Instances: https://learn.microsoft.com/en-us/azure/container-instances/container-instances-log-analytics
5. CsvHelper quickstart: https://joshclose.github.io/CsvHelper/getting-started/
