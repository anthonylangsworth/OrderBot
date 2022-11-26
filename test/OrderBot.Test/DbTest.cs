using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using System.Transactions;

namespace OrderBot.Test;

/// <summary>
/// Common base class for <see cref="MessageProcessorCache"/> tests.
/// </summary>
internal abstract class DbTest
{
    public OrderBotDbContextFactory DbContextFactory { get; set; } = null!;
    public OrderBotDbContext DbContext { get; set; } = null!;
    public IMemoryCache MemoryCache { get; set; } = null!;
    public TransactionScope TransactionScope { get; set; } = null!;

    [SetUp]
    public virtual void SetUp()
    {
        DbContextFactory = new();
        DbContext = DbContextFactory.CreateDbContext();
        MemoryCache = new MemoryCache(new MemoryCacheOptions());
        TransactionScope = new();
    }

    [TearDown]
    public virtual void TearDown()
    {
        TransactionScope.Dispose();
        DbContext.Dispose();
        MemoryCache.Dispose();
        DbContextFactory.Dispose();
    }
}
