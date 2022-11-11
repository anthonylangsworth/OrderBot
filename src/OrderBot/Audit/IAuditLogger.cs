namespace OrderBot.Audit
{
    public interface IAuditLogger : IDisposable
    {
        /// <summary>
        /// Write the <paramref name="message"/> to the audit log.
        /// </summary>
        /// <param name="message">
        /// The message to audit.
        /// </param>
        void Audit(string message);
    }
}