using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Hawkeye.Configuration;
using Hawkeye.Extensibility;
using Hawkeye.Logging;
using Hawkeye.Logging.log4net;
using Hawkeye.UI;
using Hawkeye.WinApi;
using __HawkeyeAttacherNamespace__;

namespace Hawkeye
{
    internal class Shell : IHawkeyeHost
    {
        private readonly Guid _hawkeyeId;
        private ILogServiceFactory _logFactory;
        private MainControl _mainControl;

        private MainForm _mainForm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Shell" /> class.
        /// </summary>
        public Shell()
        {
            _hawkeyeId = Guid.NewGuid();
            ApplicationInfo = new HawkeyeApplicationInfo();

            // Do nothing else here, otherwise, HawkeyeApplication static constructor may fail.
        }

        /// <summary>
        ///     Gets the plugin manager.
        /// </summary>
        /// <value>
        ///     The plugin manager.
        /// </value>
        public PluginManager PluginManager { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is injected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is injected; otherwise, <c>false</c> .
        /// </value>
        public bool IsInjected { get; private set; }

        /// <summary>
        ///     Runs the Hawkeye application.
        /// </summary>
        public void Run()
        {
            Run(IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        ///     Runs the Hawkeye application.
        /// </summary>
        /// <param name="windowToSpy">The window to spy.</param>
        /// <param name="windowToKill">The window to kill.</param>
        public void Run(IntPtr windowToSpy, IntPtr windowToKill)
        {
            // close original window: must be done before we try to log anything because the log file is locked
            // by the previous Hawkeye instance.
            if (windowToKill != IntPtr.Zero)
            {
                NativeMethods.SendMessage(
                    windowToKill, WindowMessages.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }

            int appId = _hawkeyeId.GetHashCode();
            LogInfo("Running Hawkeye in its own process.", appId);
            LogDebug($"Parameters: {windowToSpy}, {windowToKill}.", appId);
            Initialize();
            LogDebug("Hawkeye initialization is complete", appId);

            InitializeMainForm(windowToSpy);
            LogDebug("Hawkeye Main Form initialization is complete", appId);

            Application.Run(_mainForm);
        }

        /// <summary>
        ///     Operations that should be realized before we close Hawkeye.
        /// </summary>
        public void Close()
        {
            LogDebug(new string('-', 80));

            // Save settings & layout
            SettingsManager.Save();

            // Release resources (the log file for example) held by log4net.
            LogManager.Shutdown();
        }

        /// <summary>
        ///     Determines whether Hawkeye can be injected given the specified
        ///     window info.
        /// </summary>
        /// <param name="info">The window info.</param>
        /// <returns>
        ///     <c>true</c> if Hawkeye can be injected; otherwise, <c>false</c> .
        /// </returns>
        public bool CanInject(IWindowInfo info)
        {
            if (info == null)
            {
                return false;
            }

            // Same process, don't inject.
            if (info.ProcessId == Process.GetCurrentProcess().Id)
            {
                return false;
            }

            // Not a .NET process
            switch (info.Clr)
            {
                case Clr.None:
                    return false;
                case Clr.Unsupported:
                    return false;
                case Clr.Undefined:
                    return HawkeyeApplication.CurrentBitness == Bitness.x86 && info.Bitness == Bitness.x64;
            }

            // Not a supported CLR

            // Don't know! But maybe this is because we tried to inspect a x64 process and we are x86...

            // Otherwise, ok
            return true;
        }

        /// <summary>
        ///     Attaches the specified info.
        /// </summary>
        /// <param name="info">The info.</param>
        public void Inject(IWindowInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            IntPtr handle = info.Handle;
            string bootstrapExecutable = GetBootstrap(info.Clr, info.Bitness);
            Type hawkeyeAttacherType = typeof(HawkeyeAttacher);
            var arguments = new[]
            {
                handle.ToString(), // Target window
                _mainForm.Handle.ToString(), // Original Hawkeye
                "\"" + hawkeyeAttacherType.Assembly.Location + "\"", // This assembly
                "\"" + hawkeyeAttacherType.FullName +
                "\"", // The name of the class responsible for attaching to the process
                "Attach" // Attach method
            };

            string args = string.Join(" ", arguments);
            var startInfo = new ProcessStartInfo(bootstrapExecutable, args);

            LogInfo($"Starting a new instance of Hawkeye: {bootstrapExecutable}");
            LogDebug($"Command is: {bootstrapExecutable} {args}");

            // Close Hawkeye; i.e. clean everything before it is really killed in the Attach method.
            Close();

            Process.Start(startInfo);
        }

        /// <summary>
        ///     Injects the specified target window.
        /// </summary>
        /// <param name="windowToSpy">The target window.</param>
        /// <param name="originalHawkeyeWindow">
        ///     The original hawkeye window.
        /// </param>
        public void Attach(IntPtr windowToSpy, IntPtr originalHawkeyeWindow)
        {
            // Because native c++ code called Attach, we now know we are injected.
            IsInjected = true;

            // close original window: must be done before we try to log anything because the log file is locked
            // by the previous Hawkeye instance.
            NativeMethods.SendMessage(originalHawkeyeWindow, WindowMessages.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

            // Let's get the target window associated processId.
            NativeMethods.GetWindowThreadProcessId(windowToSpy, out int processId);

            int appId = _hawkeyeId.GetHashCode();
            LogInfo($"Running Hawkeye attached to application {Application.ProductName} (processId={processId})",
                appId);
            Initialize();
            LogDebug("Hawkeye initialization is complete", appId);

            InitializeMainForm(windowToSpy);
            LogDebug("Hawkeye Main Form initialization is complete", appId);

            // Show new window
            _mainForm.Show();
        }

        /// <summary>
        ///     Gets the default log service factory.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="ILogServiceFactory" /> .
        /// </returns>
        public ILogServiceFactory GetLogServiceFactory()
        {
            if (_logFactory == null)
            {
                // when injected, log4net won't find its configuration where it expects it to be
                // so we suppose we have a log4net.config file in the root directory of hawkeye.
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string log4netConfigFile = Path.Combine(directory, "log4net.config");
                _logFactory = new Log4NetServiceFactory(log4netConfigFile);
            }

            return _logFactory;
        }

        private void Initialize()
        {
            // Load Hawkeye settings
            SettingsManager.Initialize();

            // Now discover then load plugins
            PluginManager = new PluginManager();

            LogDebug("Discovering Hawkeye plugins.");
            PluginManager.DiscoverAll();
            int discoveredCount = PluginManager.PluginDescriptors.Length;

            LogDebug("Loading Hawkeye plugins.");
            PluginManager.LoadAll(this);
            int loadedCount = PluginManager.Plugins.Length;

            LogDebug($"{loadedCount}/{discoveredCount} Hawkeye plugins were successfully loaded.");
        }

        private void InitializeMainForm(IntPtr windowToSpy)
        {
            _mainForm?.Close();
            _mainForm = new MainForm();
            if (windowToSpy != IntPtr.Zero)
            {
                _mainForm.SetTarget(windowToSpy);
            }

            _mainControl = _mainForm.MainControl;
            _mainControl.CurrentInfoChanged += (s, _) =>
                RaiseCurrentWindowInfoChanged();

            RaiseCurrentWindowInfoChanged();
        }

        private void RaiseCurrentWindowInfoChanged()
        {
            CurrentWindowInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        private static string GetBootstrap(Clr clr, Bitness bitness)
        {
            string bitnessVersion;
            switch (bitness)
            {
                case Bitness.x86:
                    bitnessVersion = "x86";
                    break;
                case Bitness.x64:
                    bitnessVersion = "x64";
                    break;
                default: throw new ArgumentException($"Bitness Value {bitness} is invalid.", nameof(bitness));
            }

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Very special case: Hawkeye is x86 and the spied process is x64: we can't know for sure
            // whether the process is .NET 2 or 4 or none.
            // So, we must simply re-run Hawkeye.exe (which is compiled as Any Cpu and therefore
            // will run as x64 in a x64 environment) passing it the handle of the spied window so that another
            // detection is achieved, this time from a x64 process.
            // Note that because we run Hawkeye.exe, we won't inject anything.
            if (clr == Clr.Undefined && HawkeyeApplication.CurrentBitness == Bitness.x86 && bitness == Bitness.x64)
            {
                return Path.Combine(directory, "Hawkeye.exe");
            }

            string clrVersion = string.Empty;
            switch (clr)
            {
                case Clr.Net2:
                    clrVersion = "N2";
                    break;
                case Clr.Net4:
                    clrVersion = "N4";
                    break;
                default:
                    throw new ArgumentException(
                        $"Clr Value {clr} is invalid.", nameof(clr));
            }

            string exe = $"HawkeyeBootstrap{clrVersion}{bitnessVersion}.exe";
            return Path.Combine(directory, exe);
        }

        #region IHawkeyeHost Members

        /// <inheritdoc />
        public event EventHandler CurrentWindowInfoChanged;

        /// <inheritdoc />
        public ILogService GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }

        /// <inheritdoc />
        public ISettingsStore GetSettings(string key)
        {
            if (string.IsNullOrEmpty(key) || key == DefaultConfigurationProvider.HawkeyeStoreKey)
            {
                // Let's get a read-only version of Hawkeye settings
                ISettingsStore hawkeyeSettings = SettingsManager.GetHawkeyeStore();
                return new ReadOnlyStoreWrapper(hawkeyeSettings);
            }

            return SettingsManager.GetStore(key);
        }

        /// <inheritdoc />
        public IHawkeyeApplicationInfo ApplicationInfo { get; }

        /// <inheritdoc />
        public IWindowInfo CurrentWindowInfo => _mainControl?.CurrentInfo;

        #endregion

        #region Logging

        private static void LogInfo(string message, int applicationId = 0)
        {
            Log(LogLevel.Info, message, applicationId);
        }

        private static void LogDebug(string message, int applicationId = 0)
        {
            Log(LogLevel.Debug, message, applicationId);
        }

        private static void Log(LogLevel level, string message, int applicationId)
        {
            ILogService log = LogManager.GetLogger<Shell>();
            log.Log(level, applicationId == 0 ? message : $"{applicationId.GetHashCode()} - {message}");
        }

        #endregion
    }
}