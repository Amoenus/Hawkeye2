namespace Hawkeye.Configuration
{
    internal class SettingsStore : ISettingsStore
    {
        #region ISettingsStore Members

        /// <summary>
        ///     Gets or sets the store content.
        /// </summary>
        /// <value>
        ///     The content.
        /// </value>
        public string Content // TODO: get/set settings store raw content
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets a value indicating whether this store is read only.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this store is read only; otherwise, <c>false</c>
        ///     .
        /// </value>
        public bool IsReadOnly => false;

        #endregion
    }
}