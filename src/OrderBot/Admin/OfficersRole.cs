namespace OrderBot.Admin
{
    /// <summary>
    /// Squadron members, that can view the todo list and some settings.
    /// </summary>
    internal record OfficersRole : Role
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static OfficersRole Instance => new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        private OfficersRole()
            : base(RoleName, "Squadron officers that can change settings")
        {
            // Do nothing
        }

        public const string RoleName = "Officers";
    }
}
