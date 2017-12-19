using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HawkeyeInjector;

namespace HawkeyeBootstrap
{
    internal static class Program
    {
        //private static readonly string logFileName;

        //static Program()
        //{
        //    // Determine where to log; should be C:\ProgramData\Hawkeye\logs\HawkeyeInjector.log on Win7
        //    var hawkeyeDirectory = Path.Combine(Environment.GetFolderPath(
        //        Environment.SpecialFolder.CommonApplicationData), "Hawkeye");
        //    if (!Directory.Exists(hawkeyeDirectory))
        //        Directory.CreateDirectory(hawkeyeDirectory);

        //    var logFileDirectory = Path.Combine(hawkeyeDirectory, "logs");
        //    if (!Directory.Exists(logFileDirectory))
        //        Directory.CreateDirectory(logFileDirectory);

        //    logFileName = Path.Combine(logFileDirectory, "HawkeyeInjector.log");
        //}

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        [STAThread]
        private static void Main(string[] args)
        {
            var log = SimpleLogManager.GetLogger(typeof(Program), "Main");
            log.Info($"Command line: {Process.GetCurrentProcess().ProcessName} {string.Join(" ", args)}");

            InjectorParameters parameters = null;
            try
            {
                parameters = new InjectorParameters(args);
            }
            catch (Exception ex)
            {
                log.Error($"Wrong command-line arguments: {ex.Message}.\r\n{ex}");
                return;
            }

            try
            {
                Injector.Launch(parameters);

                // Make sure we were injected; it not, retry with the main window handle.
                var process = GetProcessFromWindowHandle(parameters.WindowHandle);
                if (process != null && !CheckInjectedStatus(process) && process.MainWindowHandle != parameters.WindowHandle)
                {
                    log.Debug("Could not inject with current handle... retrying with MainWindowHandle");
                    parameters.WindowHandle = process.MainWindowHandle;
                    Injector.Launch(parameters);
                    CheckInjectedStatus(process);
                }
            }
            catch (Exception ex)
            {
                log.Error($"There was an error during the injection process: {ex.Message}.\r\n{ex}");
                return;
            }

            log.Debug(new string('-', 120));
        }

        private static Process GetProcessFromWindowHandle(IntPtr windowHandle)
        {
            var log = SimpleLogManager.GetLogger(typeof(Program), "GetProcessFromWindowHandle");
            IntPtr processId = IntPtr.Zero;
            GetWindowThreadProcessId(windowHandle, out processId);
            if (processId == IntPtr.Zero)
            {
                log.Error($"could not get process for window handle {windowHandle}");
                return null;
            }

            var process = Process.GetProcessById(processId.ToInt32());
            if (process == null)
            {
                log.Error($"could not get process for PID = {processId}");
                return null;
            }
            return process;
        }

        private static bool CheckInjectedStatus(Process process)
        {
            var log = SimpleLogManager.GetLogger(typeof(Program), "CheckInjectedStatus");

            var containsFile = false;
            process.Refresh();
            foreach (ProcessModule module in process.Modules)
            {
                if (module.FileName.Contains("HawkeyeInjector"))
                {
                    containsFile = true;
                    break;
                }
            }

            if (containsFile) log.Info(
                $"Successfully injected Hawkeye for process {process.ProcessName} (PID = {process.Id})");
            else log.Error($"Failed to inject for process {process.ProcessName} (PID = {process.Id})");

            return containsFile;
        }

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, [Out]out IntPtr processId);
    }
}
