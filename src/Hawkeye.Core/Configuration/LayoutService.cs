using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Hawkeye.Logging;

namespace Hawkeye.Configuration
{
    internal class LayoutService
    {
        private static readonly ILogService Log = LogManager.GetLogger<LayoutService>();
        private readonly Dictionary<string, Form> _forms = new Dictionary<string, Form>();

        private readonly Func<XmlNode> _getLayoutsRootNode;
        private readonly Dictionary<string, FormLayoutData> _layouts = new Dictionary<string, FormLayoutData>();
        private readonly List<string> _loadedList = new List<string>();

        public LayoutService(Func<XmlNode> layoutsRootNodeFunc)
        {
            _getLayoutsRootNode = layoutsRootNodeFunc;
            LoadLayouts();
        }

        private bool EventsLocked { get; set; }

        /// <summary>
        ///     Suspends the forms events from being observed by this service.
        /// </summary>
        /// <returns>
        ///     An <see cref="IDisposable" /> object that when released, resumes
        ///     events observation.
        /// </returns>
        private IDisposable SuspendEvents()
        {
            return new EventsSuspender(this);
        }

        #region ILayoutService Members

        /// <summary>
        ///     Registers a <paramref name="form" /> with the service.
        /// </summary>
        /// <param name="key">
        ///     The key identifying the <paramref name="form" /> class.
        /// </param>
        /// <param name="form">The form instance.</param>
        public void RegisterForm(string key, Form form)
        {
            if (_forms.ContainsKey(key))
            {
                _forms.Remove(key);
            }

            _forms.Add(key, form);
            LoadForm(key);
            form.Load += (s, e) => LoadForm(key);
        }

        #endregion

        private class FormLayoutData
        {
            /// <summary>
            ///     Initializes a new instance of the
            ///     <see cref="LayoutService.FormLayoutData" /> class.
            /// </summary>
            public FormLayoutData() : this(null)
            {
            }

            /// <summary>
            ///     Initializes a new instance of the
            ///     <see cref="LayoutService.FormLayoutData" /> class from the data
            ///     in the specified form.
            /// </summary>
            public FormLayoutData(Form form)
            {
                if (form == null)
                {
                    AdditionalData = string.Empty;
                }
                else
                {
                    if (form is IAdditionalLayoutDataProvider provider)
                    {
                        AdditionalData = provider.GetAdditionalLayoutData();
                    }

                    Bounds = form.Bounds;
                    WindowState = form.WindowState;
                    ScreenName = Screen.PrimaryScreen.DeviceName;
                }
            }

            public Rectangle? Bounds { get; set; }
            public FormWindowState? WindowState { get; set; }
            public string ScreenName { get; set; }
            public string AdditionalData { get; set; }
        }

        private class EventsSuspender : IDisposable
        {
            private readonly LayoutService _service;

            /// <summary>
            ///     Initializes a new instance of the
            ///     <see cref="LayoutService.EventsSuspender" /> class.
            /// </summary>
            /// <param name="layoutService">The layout service.</param>
            public EventsSuspender(LayoutService layoutService)
            {
                _service = layoutService;
                _service.EventsLocked = true;
            }

            #region IDisposable Members

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _service.EventsLocked = false;
            }

            #endregion
        }

        #region Xml deserialization

        private void LoadLayouts()
        {
            LoadLayouts(_getLayoutsRootNode());
        }

        private void LoadLayouts(XmlNode rootLayoutsNode)
        {
            foreach (XmlNode layoutNode in rootLayoutsNode.ChildNodes.Cast<XmlNode>().Where(xn => xn.Name == "layout"))
            {
                string key = GetNodeValue(layoutNode, "key");
                if (key == null)
                {
                    continue;
                }

                try
                {
                    LoadLayout(layoutNode, key);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to deserialize layout information for key {key}.", ex);
                }
            }
        }

        private void LoadLayout(XmlNode layoutNode, string key)
        {
            if (_layouts.ContainsKey(key))
            {
                return;
            }

            var layoutData = new FormLayoutData();
            foreach (XmlNode xn in layoutNode.ChildNodes)
            {
                switch (xn.Name)
                {
                    case "bounds":
                        string bounds = GetNodeValue(xn);
                        if (bounds != null)
                        {
                            try
                            {
                                layoutData.Bounds = ConvertTo<Rectangle>(bounds);
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Invalid 'bounds' value for key {key}: {bounds}", ex);
                                layoutData.Bounds = null;
                            }
                        }
                        else
                        {
                            Log.Warning($"'bounds' value for key {key} was not found.");
                            layoutData.Bounds = null;
                        }

                        break;
                    case "state":
                        string state = GetNodeValue(xn);
                        if (state != null)
                        {
                            try
                            {
                                layoutData.WindowState = ConvertTo<FormWindowState>(state);
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Invalid 'state' value for key {key}: {2}", ex);
                                layoutData.WindowState = null;
                            }
                        }
                        else
                        {
                            Log.Warning($"'state' value for key {key} was not found.");
                            layoutData.WindowState = null;
                        }

                        break;
                    case "screen":
                        layoutData.ScreenName = GetNodeValue(xn);
                        break;
                    case "additionalData":
                        layoutData.AdditionalData = xn.InnerText;
                        break;
                }
            }

            _layouts.Add(key, layoutData);
        }

        private string GetNodeValue(XmlNode xn)
        {
            return GetNodeValue(xn, "value");
        }

        private string GetNodeValue(XmlNode xn, string xaName)
        {
            XmlAttribute xaValue = xn.Attributes.Cast<XmlAttribute>().FirstOrDefault(
                xa => xa.Name == xaName);
            return xaValue?.Value;
        }

        #endregion

        #region Xml serialization

        internal void SaveLayouts()
        {
            SaveLayouts(_getLayoutsRootNode());
        }

        private void SaveLayouts(XmlNode rootLayoutsNode)
        {
            XmlDocument doc = rootLayoutsNode is XmlDocument document ? document : rootLayoutsNode.OwnerDocument;

            XmlNode FindLayoutNode(string k)
            {
                IEnumerable<XmlNode> layoutNodes =
                    rootLayoutsNode.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "layout");
                foreach (XmlNode node in layoutNodes)
                {
                    XmlAttribute xaKey = node.Attributes.Cast<XmlAttribute>().SingleOrDefault(a => a.Name == "key");
                    if (xaKey.Value == k)
                    {
                        return node;
                    }
                }

                return null;
            }

            foreach (string key in _layouts.Keys)
            {
                XmlNode xn = FindLayoutNode(key);
                if (xn == null)
                {
                    xn = doc.CreateElement("layout");
                    XmlAttribute xa = doc.CreateAttribute("key");
                    xa.Value = key;
                    xn.Attributes.Append(xa);
                }

                // mandatory, if not present, skip this node
                FormLayoutData layout = _layouts[key];
                if (layout == null)
                {
                    continue;
                }

                if (layout.Bounds.HasValue)
                {
                    string boundsValue = ConvertToString(layout.Bounds);
                    XmlNode xnBounds = xn.ChildNodes.Cast<XmlNode>().SingleOrDefault(n => n.Name == "bounds");
                    if (xnBounds != null)
                    {
                        xnBounds.Attributes["value"].Value = boundsValue;
                    }
                    else
                    {
                        xnBounds = xn.AppendChild(doc.CreateElement("bounds"));
                        XmlAttribute xaBoundsValue = doc.CreateAttribute("value");
                        xaBoundsValue.Value = boundsValue;
                        xnBounds.Attributes.Append(xaBoundsValue);
                    }
                }

                if (layout.WindowState.HasValue)
                {
                    string stateValue = ConvertToString(layout.WindowState);
                    XmlNode xnState = xn.ChildNodes.Cast<XmlNode>().SingleOrDefault(n => n.Name == "state");
                    if (xnState != null)
                    {
                        xnState.Attributes["value"].Value = stateValue;
                    }
                    else
                    {
                        xnState = xn.AppendChild(doc.CreateElement("state"));
                        XmlAttribute xaStateValue = doc.CreateAttribute("value");
                        xaStateValue.Value = stateValue;
                        xnState.Attributes.Append(xaStateValue);
                    }
                }

                if (!string.IsNullOrEmpty(layout.ScreenName))
                {
                    string screenValue = layout.ScreenName;
                    XmlNode xnScreen = xn.ChildNodes.Cast<XmlNode>().SingleOrDefault(n => n.Name == "screen");
                    if (xnScreen != null)
                    {
                        xnScreen.Attributes["value"].Value = screenValue;
                    }
                    else
                    {
                        xnScreen = xn.AppendChild(doc.CreateElement("screen"));
                        XmlAttribute xaScreenValue = doc.CreateAttribute("value");
                        xaScreenValue.Value = screenValue;
                        xnScreen.Attributes.Append(xaScreenValue);
                    }
                }

                if (layout.AdditionalData != null)
                {
                    XmlNode xnAdditionalData =
                        xn.ChildNodes.Cast<XmlNode>().SingleOrDefault(n => n.Name == "additionalData") ??
                        xn.AppendChild(doc.CreateElement("additionalData"));
                    xnAdditionalData.InnerText = layout.AdditionalData;
                }

                rootLayoutsNode.AppendChild(xn);
            }

            // Regular settings have already been saved, but we happen after that. So, save once again
            SettingsManager.Save();
        }

        #endregion

        #region Registered forms events

        private bool IsLoaded(string key)
        {
            return _loadedList.Contains(key);
        }

        /// <summary>
        ///     Restores the state and bounds of the form identified by the
        ///     specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        private void LoadForm(string key)
        {
            if (IsLoaded(key))
            {
                return;
            }

            using (SuspendEvents())
            {
                if (!_forms.ContainsKey(key))
                {
                    return;
                }

                Form form = _forms[key];
                Screen defaultScreen = Screen.PrimaryScreen;

                if (_layouts.ContainsKey(key))
                {
                    FormLayoutData layout = _layouts[key];

                    // Determine screen
                    string screenName = layout.ScreenName;
                    Screen screen = defaultScreen;
                    if (!string.IsNullOrEmpty(screenName))
                    {
                        screen =
                            Screen.AllScreens.SingleOrDefault(s => s.DeviceName == screenName) ?? defaultScreen;
                    }

                    if (layout.Bounds.HasValue)
                    {
                        form.StartPosition = FormStartPosition.Manual;
                        Rectangle bounds = layout.Bounds.Value;
                        if (!screen.WorkingArea.Contains(bounds))
                        {
                            SetDefaultLayout(form);
                        }
                        else
                        {
                            form.Bounds = bounds;
                        }
                    }
                    else
                    {
                        Log.Verbose($"Could not find a 'bounds' value for layout key {key}");
                    }

                    if (layout.WindowState.HasValue)
                    {
                        FormWindowState state = layout.WindowState.Value;
                        if (state == FormWindowState.Minimized)
                        {
                            form.WindowState = FormWindowState.Normal;
                        }
                        else
                        {
                            form.WindowState = state;
                        }
                    }
                    else
                    {
                        // default to Normal
                        form.WindowState = FormWindowState.Normal;
                        Log.Verbose($"Could not find a 'state' value for layout key {key}");
                    }

                    // If state is Normal and we have no bounds, use DefaultBounds and center on screen
                    if (!layout.Bounds.HasValue && form.WindowState == FormWindowState.Normal)
                    {
                        SetDefaultLayout(form);
                    }
                }
                else
                {
                    SetDefaultLayout(form);
                    _layouts.Add(key, new FormLayoutData(form));
                }

                form.SizeChanged += (s, e) => UpdateForm(key);
                form.LocationChanged += (s, e) => UpdateForm(key);
                form.FormClosed += (s, e) => UnloadForm(key);

                _loadedList.Add(key);

                UpdateForm(key);

                // Even if no additional data was saved, call SetData.
                if (form is IAdditionalLayoutDataProvider)
                {
                    ((IAdditionalLayoutDataProvider) form).SetAdditionalLayoutData(_layouts[key].AdditionalData);
                }
            }
        }

        /// <summary>
        ///     Updates the bounds and state information of the form identified by
        ///     the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        private void UpdateForm(string key)
        {
            if (EventsLocked)
            {
                return;
            }

            if (!IsLoaded(key))
            {
                return;
            }

            using (SuspendEvents())
            {
                if (!_forms.ContainsKey(key))
                {
                    return;
                }

                Form form = _forms[key];
                FormLayoutData layout = _layouts[key];

                layout.WindowState = form.WindowState;
                layout.Bounds = form.Bounds;
                layout.ScreenName = Screen.FromControl(form).DeviceName;
            }
        }

        /// <summary>
        ///     Saves the bounds and state information of the form identified by the
        ///     specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        private void UnloadForm(string key)
        {
            UpdateForm(key);

            if (_layouts.ContainsKey(key))
            {
                FormLayoutData layout = _layouts[key];
                Form form = _forms[key];
                if (form is IAdditionalLayoutDataProvider provider)
                {
                    layout.AdditionalData = provider.GetAdditionalLayoutData();
                }
            }

            SaveLayouts();

            _loadedList.Remove(key);
        }

        #endregion

        #region Utilities

        private void SetDefaultLayout(Form form)
        {
            form.WindowState = FormWindowState.Normal;
            if (form is IDefaultLayoutProvider provider)
            {
                form.Bounds = provider.GetDefaultBounds();
            }
        }

        private static T ConvertTo<T>(string value)
        {
            return (T) ConvertToType(value, typeof(T));
        }

        /// <summary>
        ///     Converts a string into an object of type
        ///     <paramref name="targetType" /> .
        /// </summary>
        /// <remarks>
        ///     This method is used by
        ///     <see cref="Sopra.Collections.DictionarySerializer" /> .
        /// </remarks>
        /// <param name="value">The string value.</param>
        /// <param name="targetType">Type of the target object.</param>
        /// <returns>
        ///     An object of the correct type containing the converted
        ///     <paramref name="value" /> (or null).
        /// </returns>
        private static object ConvertToType(string value, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return value;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            if (!converter.CanConvertFrom(typeof(string)))
            {
                return null;
            }

            // first, we try to convert using invariant culture.
            try
            {
                return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            }
            catch (Exception ex)
            {
                Log.Verbose($"Unable to convert {value} to a {targetType} using invariant culture.", ex);

                // Then, second chance: we use the current culture.
                return converter.ConvertFrom(null, Thread.CurrentThread.CurrentCulture, value);
            }
        }

        private static string ConvertToString(object value)
        {
            switch (value)
            {
                case string s:
                    return s;
                case null:
                    return string.Empty;
                default:
                    Type objectType = value.GetType();
                    TypeConverter converter = TypeDescriptor.GetConverter(objectType);

                    // we always convert using invariant culture.
                    return converter.CanConvertTo(typeof(string))
                        ? converter.ConvertToInvariantString(value)
                        : value.ToString();
            }
        }

        #endregion
    }
}