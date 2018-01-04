using System;

namespace FxDetector
{
    internal static class This
    {
        /// <summary>
        /// Gets a value indicating whether this instance is X64.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is X64; otherwise, <c>false</c>.
        /// </value>
        public static bool IsX64 => IntPtr.Size == 8;
    }
}
