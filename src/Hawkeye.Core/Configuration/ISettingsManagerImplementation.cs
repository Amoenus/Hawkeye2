namespace Hawkeye.Configuration
{
    internal interface ISettingsManagerImplementation
    {
        /// <summary>
        /// Gets the store.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>null</c></returns>
        ISettingsStore GetStore(string key);

        void CreateDefaultSettingsFile(string filename);
        void Load(string filename);

        /// <summary>
        ///     Saves the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        void Save(string filename);
    }
}