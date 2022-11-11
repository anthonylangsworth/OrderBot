namespace OrderBot.Audit
{
    /// <summary>
    /// A "do nothing" implemntation of <see cref="IAuditLogger"/>.
    /// </summary>
    internal class NullAuditLogger : IAuditLogger
    {
        /// <inheritdoc/>
        public void Audit(string message)
        {
            // Do nothing
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do nothing
        }
    }
}
