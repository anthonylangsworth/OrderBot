To setup:
1. Download the SQL Server instance using `docker pull mcr.microsoft.com/mssql/server:2019-latest`
2. Create and run a new SQL Server container using `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<password>" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest`, substituting `<password>` with a strong password.
3. Set a password for the "OrderBot" user at the bottom of deploy/db.sql then run that SQL command to create the database, such as via SQL Management Studio.
4. Download a base image for the applications using `docker pull mcr.microsoft.com/dotnet/runtime:6.0`.