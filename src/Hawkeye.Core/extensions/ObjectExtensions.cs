using Hawkeye.ComponentModel;

namespace Hawkeye
{
    internal static class ObjectExtensions
    {
        /// <summary>
        ///     Recursively inspect the provided <c>object</c> in case it is a
        ///     <see cref="IProxy" /> to return its inner value.
        /// </summary>
        /// <param name="proxy">The potential proxy object.</param>
        /// <returns>
        ///     The specified item or its inner value.
        /// </returns>
        public static object GetInnerObject(this object proxy)
        {
            switch (proxy)
            {
                case null:
                    return null;
                case IProxy prxy:
                    return GetInnerObject(prxy.Value);
            }

            return proxy;
        }
    }
}