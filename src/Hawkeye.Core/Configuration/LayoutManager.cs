using System;
using System.Windows.Forms;
using System.Xml;

namespace Hawkeye.Configuration
{
    internal static class LayoutManager
    {
        private static LayoutService _service;

        public static void Load(XmlNode rootNode)
        {
            if (_service == null)
            {
                _service = new LayoutService(() => rootNode);
            }
            else
            {
                throw new ApplicationException("LayoutManager is already initialized.");
            }
        }

        public static void RegisterForm(string key, Form form)
        {
            if (_service == null)
            {
                throw new ApplicationException("LayoutManager is not initialized.");
            }

            _service.RegisterForm(key, form);
        }
    }
}