To setup:
1. Download the SQL Server instance using `docker pull mcr.microsoft.com/mssql/server:2019-latest`
2. Start the SQL Server instance using `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<password>" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest`, substituting `<password>` with a strong password.
3. Download a base image for the applications using `docker pull mcr.microsoft.com/dotnet/runtime:6.0`