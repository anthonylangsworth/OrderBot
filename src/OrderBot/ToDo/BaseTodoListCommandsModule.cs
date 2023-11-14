using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OrderBot.Discord;
using OrderBot.EntityFramework;

namespace OrderBot.ToDo;

/// <summary>
/// Base class for Todo-List command modules.
/// </summary>
/// <typeparam name="T">
/// The inheriting command module.
/// </typeparam>
public abstract class BaseTodoListCommandsModule<T> : BaseCommandsModule<T>
{
    protected BaseTodoListCommandsModule(OrderBotDbContext dbContext,
        ILogger<T> logger,
        ToDoListApiFactory toDoListApiFactory,
        TextChannelAuditLoggerFactory auditLogFactory,
        ResultFactory resultFactory)
        : base(dbContext, logger, auditLogFactory, resultFactory)
    {
        ApiFactory = toDoListApiFactory;
        Api = null!;
    }

    private ToDoListApiFactory ApiFactory { get; }
    protected ToDoListApi Api { get; set; }

    public override Task BeforeExecuteAsync(ICommandInfo command)
    {
        base.BeforeExecuteAsync(command);

        Api = ApiFactory.CreateApi();

        return Task.CompletedTask;
    }

    public override Task AfterExecuteAsync(ICommandInfo command)
    {
        return base.AfterExecuteAsync(command);
    }
}
