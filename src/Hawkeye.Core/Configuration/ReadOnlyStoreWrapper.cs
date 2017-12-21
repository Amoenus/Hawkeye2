using System;

namespace Hawkeye.Configuration
{
    internal class ReadOnlyStoreWrapper : ISettingsStore
    {
        private readonly ISettingsStore _wrapped;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="ReadOnlyStoreWrapper" /> class.
        /// </summary>
        /// <param name="store">The wrapped settings store.</param>
        /// <exception cref="ArgumentNullException" />
        public ReadOnlyStoreWrapper(ISettingsStore store)
        {
            _wrapped = store ?? throw new ArgumentNullException(nameof(store));
        }

        #region ISettingsStore Members

        /// <summary>
        ///     Gets or sets the store content.
        /// </summary>
        /// <value>
        ///     The content.
        /// </value>
        /// <exception cref="NotSupportedException" />
        /// <inheritdoc />
        public string Content
        {
            get => _wrapped.Content;
            set => throw new NotSupportedException("This Settings Store is Read-Only.");
        }

        /// <inheritdoc />
        public bool IsReadOnly => true;

        #endregion
    }
}