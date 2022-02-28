using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddnMessageSink.Test
{
    /// <summary>
    /// An implementation of ILogger{T} that does nothing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NullLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Do nothing
        }
    }
}
