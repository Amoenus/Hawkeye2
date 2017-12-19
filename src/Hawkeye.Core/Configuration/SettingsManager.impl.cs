using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Hawkeye.Logging;
using System;

namespace Hawkeye.Configuration
{
    partial class SettingsManager
    {
        private class SettingsManagerImplementation
        {
            private const string ImplementationVersion = "1.0.0";

            private static readonly ILogService Log = LogManager.GetLogger<SettingsManagerImplementation>();

            private Dictionary<string, SettingsStore> _stores = new Dictionary<string, SettingsStore>();
            private XmlDocument _settingsDocument = null;

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

                var rootNode = _settingsDocument.ChildNodes.Cast<XmlNode>().SingleOrDefault(xn => xn.Name == "settings");
                if (rootNode == null)
                {
                    rootNode = _settingsDocument.CreateElement("settings");
                    var versionNode = _settingsDocument.CreateAttribute("version");
                    versionNode.Value = ImplementationVersion;
                    rootNode.Attributes.Append(versionNode);
                    return;
                }

                var children = rootNode.ChildNodes.Cast<XmlNode>();
                if (!children.Any())
                {
                    // Add Hawkeye node
                    rootNode.AppendChild(_settingsDocument.CreateElement(HawkeyeStoreKey));
                    return;
                }

                foreach (var node in children)
                {
                    if (node.Name == HawkeyeStoreKey)
                        LoadHawkeyeSettings(node);
                    else
                    {
                        var prefix = node.Name;
                        foreach (var subnode in node.ChildNodes.Cast<XmlNode>())
                            LoadSettings(subnode, $"{prefix}/{subnode.Name}");
                    }
                }
            }

            private void LoadHawkeyeSettings(XmlNode node)
            {
                var children = node.ChildNodes.Cast<XmlNode>();
                if (!children.Any()) return;

                var configurationNode = children.SingleOrDefault(n => n.Name == "configuration");
                if (configurationNode != null) LoadSettings(configurationNode, HawkeyeStoreKey);

                var layoutNode = children.SingleOrDefault(n => n.Name == "layouts");
                if (layoutNode != null) LayoutManager.Load(layoutNode);
            }

            private void LoadSettings(XmlNode node, string storeKey)
            {
                var store = new SettingsStore();
                store.Content = node.InnerText;
                _stores.Add(storeKey, store);
            }

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
                    var backup = filename + ".bak";
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
}
