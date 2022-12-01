using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.Discord;

/// <summary>
/// Base class for command modules.
/// </summary>
/// <typeparam name="T">
/// The inheriting command module.
/// </typeparam>
public class BaseCommandsModule<T> : InteractionModuleBase<SocketInteractionContext>
{
    protected BaseCommandsModule(OrderBotDbContext dbContext,
        ILogger<T> logger,
        TextChannelAuditLoggerFactory auditLogFactory,
        ResultFactory resultFactory)
    {
        DbContext = dbContext;
        Logger = logger;
        AuditLogFactory = auditLogFactory;
        ResultFactory = resultFactory;
        Result = null!;
        AuditLogger = null!;
        TransactionScope = null!;
    }

    protected OrderBotDbContext DbContext { get; }
    protected ILogger<T> Logger { get; }
    private TextChannelAuditLoggerFactory AuditLogFactory { get; }
    private ResultFactory ResultFactory { get; }
    protected EphemeralResult Result { get; private set; }
    protected IAuditLogger AuditLogger { get; set; }
    protected TransactionScope TransactionScope { get; private set; }

    public override Task BeforeExecuteAsync(ICommandInfo command)
    {
        base.BeforeExecuteAsync(command);

        IAuditLogger auditLogger = AuditLogFactory.CreateAuditLogger(Context);
        Result = ResultFactory.GetResponse(Context, auditLogger, Logger);
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
