using System;
using System.ComponentModel;

using Hawkeye.WinApi;

namespace Hawkeye.ComponentModel
{
    /// <summary>
    /// This class represents a Win32 module.
    /// </summary>
    [TypeConverter(typeof(ModuleInfoConverter))]
    internal class ModuleInfo : IModuleInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleInfo"/> class.
        /// </summary>
        /// <param name="module">The Win32 module entry data.</param>
        public ModuleInfo(MODULEENTRY32 module)
        {
            ProcessId = module.th32ProcessID;
            BaseAddress = module.modBaseAddr;
            BaseSize = module.modBaseSize;
            Handle = module.hModule;
            Name = module.szModule;
            Path = module.szExePath;

            ModuleId = module.th32ModuleID;
            StructureSize = module.dwSize;
            GlblcntUsage = module.GlblcntUsage;
            ProccntUsage = module.ProccntUsage;
        }

        #region IModuleInfo Members

        /// <inheritdoc />
        public uint ProcessId { get; private set; }

        /// <inheritdoc />
        public IntPtr BaseAddress { get; private set; }

        /// <inheritdoc />
        public uint BaseSize { get; private set; }

        /// <inheritdoc />
        public IntPtr Handle { get; private set; }

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public string Path { get; private set; }

        #endregion

        /// <summary>
        /// Gets the module identifier.
        /// </summary>
        /// <value>
        /// The module identifier.
        /// </value>
        public uint ModuleId { get; private set; }

        /// <summary>
        /// Gets the size of the structure.
        /// </summary>
        /// <value>
        /// The size of the structure.
        /// </value>
        public uint StructureSize { get; private set; }

        /// <summary>
        /// Gets the GLBLCNT usage.
        /// </summary>
        /// <value>
        /// The GLBLCNT usage.
        /// </value>
        public uint GlblcntUsage { get; private set; }

        /// <summary>
        /// Gets the proccnt usage.
        /// </summary>
        /// <value>
        /// The proccnt usage.
        /// </value>
        public uint ProccntUsage { get; private set; }
    }
}
