namespace OrderBot.Admin
{
    public interface IAuditLogger : IDisposable
    {
        /// <summary>
        /// Write the <paramref name="message"/> to the audit log.
        /// </summary>
        /// <param name="success">
        /// <c>true</c> if the operation was successful, <c>false</c> otherwise.
        /// </param>
        /// <param name="message">
        /// The message to audit.
        /// </param>
        void Audit(bool success, string message);
    }
}