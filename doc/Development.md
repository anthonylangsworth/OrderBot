# Development

## Setup
This project is currently hosted in Azure. Run the [Deploy](../../../actions/workflows/deploy.yml) action to update the deployed version.

To setup locally:
1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop/).
2. Download the code.
3. Create the database container:
    1. Download the SQL Server instance using `docker pull mcr.microsoft.com/mssql/server:2019-latest`
    2. Create and run a new SQL Server container using `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<password>" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest`, substituting `<password>` with a strong, unique password.
    3. Set the new `OrderBot` password in `deploy/db.sql` login around line 7. 
    4. Run `deploy/db.sql` as sa, such as via [SQL Management Studio](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16), to create the database.
    5. Run `deploy/tables.sql` as sa to create the table structure.
4. Create the application container:
    1. (Optional - Visual Studio does this automatically) Download a base image for the application using `docker pull mcr.microsoft.com/dotnet/runtime:8.0`.
    2. Create a `.env` file in `src/OrderBot`. Create four entries inside it:
        1. `ConnectionStrings__OrderBot`, containing the SQL server connection string.
        2. `Discord__ApiKey`, containing the Discord Bot's API key.
        3. `LogAnalytics__WorkspaceId`, containing an Azure LogAnalytics workspace ID.
        4. `LogAnalytics__WorkspaceKey`, containing an Azure LogAnalytics primary key.
5. Build and run the code.

## Infrastructure

An overview of the BGS Bot's deployed infrastructure is:
```mermaid
flowchart TB
  edmc["Elite Dangerous<br/>Market Connector (EDMC)"] -->|Journal Entries| eddn["Elite Dangerous<br/>Data Network (EDDN)"]
  edd["ED Discovery"] -->|Journal Entries| eddn
  gameglass["Game Glass"] -->|Journal Entries| eddn
  eddn -->|Journal Entries| Azure
  eddn -->|Journal Entries| Inara
  eddn -->|Journal Entries| EDSM
  eddn -->|Journal Entries| EDDB
  subgraph Azure
    direction TB
    ci[Azure Container Instance] <-->|Data| db[(Azure SQL Database)]
    ci -->|Logs| logs[(Azure Log Analytics)]
  end
  Azure <-->|Commands and Messages| discord["Discord Server Infrastructure"]
  discord <--> discordClient["Discord Clients"]
```

Key points:
1. The BGS Order Bot receives data from Elite Dangerous Data Network (EDDN) via an [AMQP](https://www.amqp.org/about/what) queue. EDDN receives data from common ED companion applications and is used by many popular ED resources.
2. The BGS Order Bot consists of a container, hosted in Azure Container Instance, an Azure SQL database for data and an Azure Log Analytics store for the logs. Deployment is automated via the Github repository `Deploy` action. The container is configured by environment variables and is deployed from an Azure Container Registry (not shown for clarity).
3. The BGS Order Bot's primary user interface is via Discord. Individual Discord guilds (tenants) can invite the bot to their servers, configure it using commands then receive suggestions and carrier movement notifications.

## Writing Discord Commands

Overview:
```mermaid
sequenceDiagram
  DiscordClient->>BotHostedService : Client_InteractionCreated()
  BotHostedService->>InteractionService: ExecuteCommandAsync()
  InteractionService->>+CommandsModule: Call method with [SlashCommand()]
  CommandsModule--)-InteractionService: void 
  InteractionService--)BotHostedService: void
  BotHostedService--)DiscordClient: void
```

Key points:
1. `CommandsModule` refers to a class derived from `InteractionModuleBase<SocketInteractionContext>`. There are currently three:
    1. `AdminCommandsModule`, which handles administrative commands like audit and role management.
    2. `CarrierMovementCommandsModule`, which handles commands to ignore or track carrier movements. 
    3. `ToDoListCommandsModule`, which handles viewing the To-Do list, supporting minor factions and adding goals. 
2. The `InteractionService` provided by Discord.Net provides a nice wrapper over manually parsing and handling commands.

`Client_InteractionCreated` in [BotHostedService](../../../tree/main/src/OrderBot/Discord/BotHostedService.cs) provides the following:
1. Creates an `IServiceScope` so scoped DI services can be returned and cleaned up.
2. Adds a logging scope with common details such as the user, guild and command details. This is done here and not in `BaseCommandsModule<T>` so errors captured here are logged with the same scope.
3. Shows an access denied-style error messages for unmet preconditions.
4. Logs details of other errors and exceptions.

Best practice for writing slash (application) commands:
1. Derive command modules classes from `BaseCommandsModule<T>`. This class handles common tasks like creating database connections, audit logs and a `Result` object.
2. Wrap the code for each command in a `try ... catch` block with an `Exception` handler containing `Result.Exception`. This handles any unexpected exceptions. While Discord.Net will catch and log unthrown exceptions, it will not notify the user.
3. Use `Result` methods to communicate with the user and wraps logging and auditing for most situations. Specifically:
    1.  `Information` for responses to queries or acknowledgements. These are logged as Information by default but not audited.
    2.  `Success` for successful changes or actions. These are audited by default and logged as Information.
    3.  `Error` for unsuccessful changes or actions, such as invalid command parameter values. These are logged as Warnings. The error message has three parts: what, why and a fix. This encourages better error messages and separates the loggable portion (why).
    4.  `Exception` for unhandled or unknown exceptions. These are logged as Errors.
4. The `Result` object also does some housekeeping like (1) calling `DeferAsync` early to ensure long-running commands do not time out and (2) capping the message length to the max ephemeral response length.
5. Auditing is usually handled through the `Result` object but you can still audit directly using `AuditLogger`. Keep it to one audit message per command execution.
6. Logging is usually handled also through the `Result` object but you can still log directly using `Logger`. Keep it to one non-verbose/diagnostic log message per command execution.
7. Use `TransactionScope.Complete()` as the last statement to save any database work. Otherwise, results will not be saved.
8. Remember that the class housing the command handler is instantiated for each interaction.
9. Do not duplicate work in `BaseCommandsModule` or `BotHostedService.Client_InteractionCreated`. The general goal is to move as much work to there as possible. This standardizes behaviour and prevents code repetition.

## Message Processing
To provide data for the Discord bot, this system listens for [Elite Dangerous Data Network (EDDN)](https://eddn.edcd.io/) messages via the `EddnMessageHostedService`, which are handled by `EddnMessageMessageProcessor` subclasses. There are currently two: `TodoListMessageProcessor`, which captures system BGS data, and `CarrierMovementMessageProcessor`, which looks for carrier movements and notifies Discord guilds which have registered a carrier movement channel. These classes are instantiated for each message.

This structure provides separation of responsibilities. Classes for each message processor are in separate namespaces to further emphasize this.

An overview:
```mermaid
sequenceDiagram
  participant EddnMessageHostedService
  participant ToDoListMessageProcessor
  participant CarrierMovementMessageProcessor
  participant Caches
  participant TextChannelWriter
  activate EddnMessageHostedService
  activate Caches
  par
    EddnMessageHostedService-)+ToDoListMessageProcessor: ProcessAsync()
    ToDoListMessageProcessor->>Caches: Get Value
    Caches-->>ToDoListMessageProcessor: Result
    ToDoListMessageProcessor--)-EddnMessageHostedService: void
  and 
    EddnMessageHostedService-)+CarrierMovementMessageProcessor: ProcessAsync()
    CarrierMovementMessageProcessor->>Caches: Get Value
    Caches-->>CarrierMovementMessageProcessor: Result
    CarrierMovementMessageProcessor->>+TextChannelWriter: WriteLine()
    TextChannelWriter-->>-CarrierMovementMessageProcessor: void
    CarrierMovementMessageProcessor--)-EddnMessageHostedService: void
  end
  deactivate EddnMessageHostedService
  deactivate Caches
```

Key points:
1. `EddnMessageHostedService` is started from Program.cs and runs for the container's lifetime.
2. `Caches` includes various classes inherited from `MessageProcessorCache`. Singleton objects instantiated from these cache classes minimize database access when processing and eliminating messages. 
    1. `TodoListMessageProcessor` uses `SupportedMinorFactionsCache` and `GoalStarSystemsCache`. 
    2. `CarrierMovementMessageProcessor` uses `StarSystemToDiscordGuildCache`, `IgnoredCarriersCache` and `CarrierMovementChannelCache`.
4. Technically, the `TextChannelWriter` is a `TextWriter` created via a `TextChannelWritterFactory`. This is used to write to carrier movement channel(s).
5. Database or ORM classes like `OrderbotDbContext` are omitted for clarity.

Regarding `Caches`, there is currently no cache invalidation mechanism, but the cache durations are short: five minutes. An unfinished invalidation pattern is in `MessageProcessorCacheInvalidator`.

## References
1. Using Docker with .Net Core: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/visual-studio-tools-for-docker?view=aspnetcore-6.0
2. Github action to build SQL server database: https://github.com/ankane/setup-sqlserver
3. Discord.Net documentation: https://discordnet.dev/
4. Using Log Analytics with Container Instances: https://learn.microsoft.com/en-us/azure/container-instances/container-instances-log-analytics
5. CsvHelper quickstart: https://joshclose.github.io/CsvHelper/getting-started/
6. Avoid record types with Entity Framework: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record
7. Mermaid Sequence diagrams: https://mermaid-js.github.io/mermaid/#/sequenceDiagram
