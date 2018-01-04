using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Hawkeye.Reflection
{
    /// <summary>
    ///     Specific assembly resolver used when hawkeye is injected into another
    ///     process.
    /// </summary>
    internal static class AssemblyResolver
    {
        // We cache unresolved assemblies to speed up things a bit (for instance
        // when looking for inexistent resource assemblies).
        private static readonly List<string> UnresolvedAssemblies = new List<string>();

        // Let's also cache resolved ones
        private static readonly Dictionary<string, string> ResolvedAssemblies = new Dictionary<string, string>();

        /// <summary>
        ///     Resolves the specified assembly name.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>
        /// </returns>
        public static Assembly Resolve(string assemblyName)
        {
            // First try caches
            if (UnresolvedAssemblies.Contains(assemblyName))
            {
                return null;
            }

            if (ResolvedAssemblies.ContainsKey(assemblyName))
            {
                return Assembly.LoadFrom(ResolvedAssemblies[assemblyName]);
            }

            // RequestingAssembly exists in .NET 4 but not before
            Assembly requestingAssembly = Assembly.GetExecutingAssembly();
            string directory = Path.GetDirectoryName(requestingAssembly.Location);
            // see http://stackoverflow.com/questions/1373100/how-to-add-folder-to-assembly-search-path-at-runtime-in-net
            string file = Path.Combine(directory, new AssemblyName(assemblyName).Name + ".dll");

            if (!File.Exists(file))
            {
                UnresolvedAssemblies.Add(assemblyName);
                return null; // will throw!
            }

            ResolvedAssemblies.Add(assemblyName, file);
            return Assembly.LoadFrom(file);
        }
    }
}