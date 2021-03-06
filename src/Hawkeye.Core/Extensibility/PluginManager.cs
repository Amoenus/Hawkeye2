﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Hawkeye.Logging;

namespace Hawkeye.Extensibility
{
    internal class PluginManager
    {
        private static readonly ILogService Log = LogManager.GetLogger<PluginManager>();
        private const string PluginsSubDirectory = "plugins";

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        public PluginManager()
        {
            // Just so that it does not return null;
            PluginDescriptors = new IPluginDescriptor[0];
        }

        /// <summary>
        /// Gets the discovered plugin descriptors.
        /// </summary>
        public IPluginDescriptor[] PluginDescriptors { get; private set; }

        /// <summary>
        /// Gets the loaded plugin instances.
        /// </summary>
        public IPlugin[] Plugins { get; private set; }

        /// <summary>
        /// Discovers the available plugins.
        /// </summary>
        public void DiscoverAll()
        {
            var directory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginsSubDirectory);

            if (!Directory.Exists(directory))
            {
                Log.Debug($"No Plugins directory ({directory}).");
                return;
            }

            var descriptors = new List<IPluginDescriptor>();

            foreach (var file in Directory.GetFiles(directory, "*.dll"))
            {
                Log.Debug($"Examining File {file} for plugins:");
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    var types = assembly.GetTypes().Where(t => t.IsA<IPluginDescriptor>()).ToArray();

                    if (types == null || types.Length == 0)
                        Log.Debug("--> No plugins in this assembly");
                    else
                    {
                        Log.Debug($"--> {types.Length} plugins were found in this assembly");
                        foreach (var type in types)
                        {
                            try
                            {
                                var instance = (IPluginDescriptor)type.CreateInstance();
                                descriptors.Add(instance);
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"----> Could not create an instance of type {type}: {ex.Message}", ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug($"--> Not an assembly: {ex.Message}");
                }
            }

            PluginDescriptors = descriptors.ToArray();
        }

        /// <summary>
        /// Loads all the plugins (passing them the specified host)
        /// from previously discovered plugin descriptors.
        /// </summary>
        /// <param name="host">The host.</param>
        public void LoadAll(IHawkeyeHost host)
        {
            var plugins = new List<IPlugin>();

            foreach (var descriptor in PluginDescriptors)
            {
                var pluginName = "?"; // Unlikely, but descriptor.Name could throw...
                try
                {
                    pluginName = descriptor.Name;
                    Log.Debug($"Loading plugin '{pluginName}':");
                    var instance = descriptor.Create(host);
                    if (instance == null)
                        Log.Warning("--> Created plugin is null. Nothing to load.");
                    else
                    {
                        plugins.Add(instance);
                        Log.Debug("--> OK");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"--> Plugin '{pluginName}' could not be loaded: {ex.Message}", ex);
                }
            }

            Plugins = plugins.ToArray();
        }
    }
}
