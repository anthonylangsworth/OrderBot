using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.ToDo;
public abstract class BaseTodoListCommandsModule<T> : InteractionModuleBase<SocketInteractionContext>
{
    protected BaseTodoListCommandsModule(OrderBotDbContext dbContext,
        ILogger<T> logger, ToDoListApiFactory toDoListApiFactory,
        TextChannelAuditLoggerFactory auditLogFactory,
        ResponseFactory responseFactory)
    {
        DbContext = dbContext;
        Logger = logger;
        ApiFactory = toDoListApiFactory;
        AuditLogFactory = auditLogFactory;
        ResponseFactory = responseFactory;
        Api = null!;
        Response = null!;
        AuditLogger = null!;
        TransactionScope = null!;
    }

    protected OrderBotDbContext DbContext { get; }
    protected ILogger<T> Logger { get; }
    private ToDoListApiFactory ApiFactory { get; }
    private TextChannelAuditLoggerFactory AuditLogFactory { get; }
    private ResponseFactory ResponseFactory { get; }
    protected ToDoListApi Api { get; set; }
    protected EphemeralResponse Response { get; private set; }
    protected IAuditLogger AuditLogger { get; set; }
    protected TransactionScope TransactionScope { get; private set; }

    public override Task BeforeExecuteAsync(ICommandInfo command)
    {
        base.BeforeExecuteAsync(command);

        Api = ApiFactory.CreateApi(Context.Guild);
        IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
        Response = ResponseFactory.GetResponse(Context, auditLogger, Logger);
        TransactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        return Task.CompletedTask;
    }

    public override Task AfterExecuteAsync(ICommandInfo command)
    {
        AuditLogger.Dispose();
        TransactionScope.Dispose();

        return base.AfterExecuteAsync(command);
    }
}
