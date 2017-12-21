using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

/////////////////////////////////////////////////////////////////////
// Large parts and overall inspiration for this code come from the
// Snoop project (http://xxx). Cory Plotts and Xxx should be
// granted credit for this.
/////////////////////////////////////////////////////////////////////
namespace FxDetector
{
    internal static class NativeMethods
    {
        [Flags]
        public enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F
        }

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern ToolHelpHandle CreateToolhelp32Snapshot(SnapshotFlags dwFlags, int th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Module32First(ToolHelpHandle hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        public static extern bool Module32Next(ToolHelpHandle hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

        /// <summary>
        ///     Safe handle wrapper for Module32* and
        ///     <see cref="CreateToolhelp32Snapshot" /> Win32 functions.
        /// </summary>
        public class ToolHelpHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            /// <summary>
            ///     Initializes a new instance of the
            ///     <see cref="NativeMethods.ToolHelpHandle" /> class.
            /// </summary>
            private ToolHelpHandle() : base(true)
            {
            }

            /// <summary>
            ///     When overridden in a derived class, executes the code required
            ///     to free the handle.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the handle is released successfully;
            ///     otherwise, in the event of a catastrophic failure, false. In
            ///     this case, it generates a releaseHandleFailed MDA Managed
            ///     Debugging Assistant.
            /// </returns>
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        [TypeConverter(typeof(ModuleEntryConverter))]
        public struct MODULEENTRY32 : IEquatable<MODULEENTRY32>
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            private IntPtr modBaseAddr;
            public uint modBaseSize;
            private IntPtr hModule;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;

            /// <inheritdoc />
            public bool Equals(MODULEENTRY32 other)
            {
                return dwSize == other.dwSize && th32ModuleID == other.th32ModuleID &&
                       th32ProcessID == other.th32ProcessID && GlblcntUsage == other.GlblcntUsage &&
                       ProccntUsage == other.ProccntUsage && modBaseAddr.Equals(other.modBaseAddr) &&
                       modBaseSize == other.modBaseSize && hModule.Equals(other.hModule) &&
                       string.Equals(szModule, other.szModule) && string.Equals(szExePath, other.szExePath);
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is MODULEENTRY32 moduleentry32 && Equals(moduleentry32);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (int) dwSize;
                    hashCode = (hashCode * 397) ^ (int) th32ModuleID;
                    hashCode = (hashCode * 397) ^ (int) th32ProcessID;
                    hashCode = (hashCode * 397) ^ (int) GlblcntUsage;
                    hashCode = (hashCode * 397) ^ (int) ProccntUsage;
                    hashCode = (hashCode * 397) ^ modBaseAddr.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int) modBaseSize;
                    hashCode = (hashCode * 397) ^ hModule.GetHashCode();
                    hashCode = (hashCode * 397) ^ (szModule != null ? szModule.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (szExePath != null ? szExePath.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}