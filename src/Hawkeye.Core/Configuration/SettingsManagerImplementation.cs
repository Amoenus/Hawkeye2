using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Hawkeye.Logging;

namespace Hawkeye.Configuration
{
    internal class SettingsManagerImplementation
    {
        private const string ImplementationVersion = "1.0.0";

        private static readonly ILogService Log = LogManager.GetLogger<SettingsManagerImplementation>();

        private readonly Dictionary<string, SettingsStore> _stores = new Dictionary<string, SettingsStore>();
        private XmlDocument _settingsDocument;

        public ISettingsStore GetStore(string key)
        {
            return null;
        }

        public void CreateDefaultSettingsFile(string filename)
        {
            const string defaultContent = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<settings version=""1.0.0"">
  <hawkeye>
    <layouts></layouts>
    <configuration></configuration>
  </hawkeye>
  <plugins>
    <snapshot></snapshot>
    <reflector></reflector>
  </plugins>
</settings>";

            CreateBackup(filename);
            File.WriteAllText(filename, defaultContent, Encoding.UTF8);
        }

        public void Load(string filename)
        {
            if (_settingsDocument == null)
            {
                _settingsDocument = new XmlDocument();
                _settingsDocument.Load(filename);
            }

            XmlNode rootNode = _settingsDocument.ChildNodes.Cast<XmlNode>()
                .SingleOrDefault(xn => xn.Name == "settings");
            if (rootNode == null)
            {
                rootNode = _settingsDocument.CreateElement("settings");
                XmlAttribute versionNode = _settingsDocument.CreateAttribute("version");
                versionNode.Value = ImplementationVersion;
                rootNode.Attributes.Append(versionNode);
                return;
            }

            IEnumerable<XmlNode> children = rootNode.ChildNodes.Cast<XmlNode>();
            if (!children.Any())
            {
                // Add Hawkeye node
                rootNode.AppendChild(_settingsDocument.CreateElement(DefaultConfigurationProvider.HawkeyeStoreKey));
                return;
            }

            foreach (XmlNode node in children)
            {
                if (node.Name == DefaultConfigurationProvider.HawkeyeStoreKey)
                {
                    LoadHawkeyeSettings(node);
                }
                else
                {
                    string prefix = node.Name;
                    foreach (XmlNode subnode in node.ChildNodes.Cast<XmlNode>())
                    {
                        LoadSettings(subnode, $"{prefix}/{subnode.Name}");
                    }
                }
            }
        }

        private void LoadHawkeyeSettings(XmlNode node)
        {
            IEnumerable<XmlNode> children = node.ChildNodes.Cast<XmlNode>();
            if (!children.Any())
            {
                return;
            }

            XmlNode configurationNode = children.SingleOrDefault(n => n.Name == "configuration");
            if (configurationNode != null)
            {
                LoadSettings(configurationNode, DefaultConfigurationProvider.HawkeyeStoreKey);
            }

            XmlNode layoutNode = children.SingleOrDefault(n => n.Name == "layouts");
            if (layoutNode != null)
            {
                LayoutManager.Load(layoutNode);
            }
        }

        private void LoadSettings(XmlNode node, string storeKey)
        {
            var store = new SettingsStore();
            store.Content = node.InnerText;
            _stores.Add(storeKey, store);
        }

        /// <summary>
        ///     Saves the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Save(string filename)
        {
            CreateBackup(filename);
            // TODO: save settings
            _settingsDocument.Save(filename);
        }

        private void CreateBackup(string filename)
        {
            if (File.Exists(filename))
            {
                // Create a backup copy
                string backup = filename + ".bak";
                try
                {
                    File.Copy(filename, backup, true);
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not create backup copy of settings file: {ex.Message}", ex);
                }
            }
        }
    }
}