using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Hawkeye.Logging.log4net
{
    /// <summary>
    ///     This class allows for the creation of a trace service instance based on
    ///     the log4net framework.
    /// </summary>
    internal class Log4NetServiceFactory : ILogServiceFactory, ILogServiceAppendable
    {
        public const string ConfigurationFileKey = "configurationFile";

        private static bool log4netConfiguredYet;

        private static Log4NetService rootLog4NetService;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="Log4NetServiceFactory" /> class.
        /// </summary>
        public Log4NetServiceFactory() :
            this(string.Empty)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="Log4NetServiceFactory" /> class.
        /// </summary>
        /// <param name="configFile">
        ///     The path to a log4net configuration file (or empty to use
        ///     app.config).
        /// </param>
        public Log4NetServiceFactory(string configFile)
        {
            ConfigureLog4Net(configFile);
        }

        private static Log4NetService RootLog4NetService
        {
            get
            {
                if (rootLog4NetService == null)
                {
                    ILogger rootLogger = GetRootLogger();
                    if (rootLogger == null)
                    {
                        return null;
                    }

                    rootLog4NetService = new Log4NetService(rootLogger);
                }

                return rootLog4NetService;
            }
        }

        private static string InitializeApplicationName()
        {
            // We add a property giving the current application name, for use in log4net.config files.
            Assembly entryAssembly = GetEntryAssembly();
            string appname = entryAssembly != null ? entryAssembly.GetName().Name : "unknown-app";
            GlobalContext.Properties["ApplicationName"] = appname;
            return appname;
        }

        private static void ConfigureLog4Net(string filename)
        {
            if (log4netConfiguredYet)
            {
                return;
            }

            InitializeApplicationName();
            if (!string.IsNullOrEmpty(filename))
            {
                var file = new FileInfo(filename);
                if (file != null)
                {
                    XmlConfigurator.Configure(file);
                    log4netConfiguredYet = true;
                }
            }

            // maybe we have config in the app.config file?
            if (!log4netConfiguredYet)
            {
                XmlConfigurator.Configure();
                log4netConfiguredYet = true;
            }

            // Forces initialization
            ILogService initialLogger = CreateService(null);
        }

        /// <summary>
        ///     Creates the logging service and return the newly created instance.
        /// </summary>
        /// <remarks>
        ///     The configuration file used by log4net will be the application's
        ///     configuration file (
        ///     <b>
        ///         web.config
        ///     </b>
        ///     ou
        ///     <b>
        ///         app.exe.config
        ///     </b>
        ///     ) which will have to contain a
        ///     <b>
        ///         <log4net>
        ///     </b>
        ///     section.
        /// </remarks>
        /// <param name="type">
        ///     The type that requests the creation of a log service.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="Toaster.Logging.ILogService" /> .
        /// </returns>
        private static ILogService CreateService(Type type)
        {
            return type == null ? RootLog4NetService : new Log4NetService(type);
        }

        private static ILogger GetRootLogger()
        {
            var hierarchy = (Hierarchy) global::log4net.LogManager.GetRepository();
            return hierarchy.Root;
        }

        private static Assembly GetEntryAssembly()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                return entryAssembly;
            }

            return typeof(Log4NetServiceFactory).Assembly; // fallback: should never happen.
        }

        #region ILogServiceFactory Members

        /// <summary>
        ///     Obtains an instance of the logger service for the specified
        ///     <paramref name="type" /> and optional additional data.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="additionalData">The additional data.</param>
        /// <returns>
        ///     An implementation of <see cref="ILogService" /> .
        /// </returns>
        public ILogService GetLogger(Type type, IDictionary<string, object> additionalData = null)
        {
            // We do nothing with the additional data at the moment.
            return CreateService(type);
        }

        /// <summary>
        ///     Appends the specified log service to a root log service (the current
        ///     instance).
        /// </summary>
        /// <param name="logService">The log service.</param>
        /// <param name="additionalData">The optional additional data.</param>
        /// <returns>
        ///     An implementation of <see cref="ILogLevelThresholdSelector" />
        ///     allowing to set a maximum log level to trace.
        /// </returns>
        public ILogLevelThresholdSelector AppendLogService(ILogService logService,
            IDictionary<string, object> additionalData = null)
        {
            if (logService == null)
            {
                throw new ArgumentNullException(nameof(logService));
            }

            return RootLog4NetService?.AppendLogService(logService, additionalData);
        }

        /// <summary>
        ///     Closes all the resources held by the various loggers.
        /// </summary>
        public void Shutdown()
        {
            var hierarchy = (Hierarchy) global::log4net.LogManager.GetRepository();
            hierarchy?.Shutdown();
        }

        #endregion
    }
}