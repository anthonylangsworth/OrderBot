using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using OrderBot.MessageProcessors;

namespace OrderBot.Test;

/// <summary>
/// Common base class for <see cref="MessageProcessorCache"/> tests.
/// </summary>
/// <typeparam name="T">
/// The type of <see cref="MessageProcessorCache"/> to test.
/// </typeparam>
internal abstract class CacheTest<T> : DbTest
    where T : MessageProcessorCache
{
    protected CacheTest(Func<IMemoryCache, T> createCache)
    {
        CreateCache = createCache;
    }

    public T Cache { get; set; } = null!;
    protected Func<IMemoryCache, T> CreateCache { get; }

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        Cache = CreateCache(MemoryCache);
    }
}
