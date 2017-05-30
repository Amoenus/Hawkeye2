using System;
using System.Linq;
using System.Security;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

using Hawkeye.Logging;

namespace Hawkeye.ComponentModel
{
    internal static class ComponentModelExtensions
    {
        private static readonly ILogService Log = LogManager.GetLogger(typeof(ComponentModelExtensions));

        private static readonly string[] ExcludedProperties = new[]
        {
            "System.Windows.Forms.Control.ShowParams",
            "System.Windows.Forms.Control.ActiveXAmbientBackColor",
            "System.Windows.Forms.Control.ActiveXAmbientFont",
            "System.Windows.Forms.Control.ActiveXAmbientForeColor",
            "System.Windows.Forms.Control.ActiveXEventsFrozen",
            "System.Windows.Forms.Control.ActiveXHWNDParent",
            "System.Windows.Forms.Control.ActiveXInstance",
            "System.Windows.Forms.Form.ShowParams",
        };

        private static readonly string[] ExcludedEvents = new string[0];

        private static readonly BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.InvokeMethod |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly BindingFlags StaticFlags =
            BindingFlags.Static | BindingFlags.InvokeMethod |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        #region ITypeDescriptorContext extensions

        public static PropertyDescriptorCollection GetAllProperties(
            this ITypeDescriptorContext context,
            object component,
            bool inspectBaseClasses = true,
            bool retrieveStaticMembers = true,
            bool keepOriginalCategoryAttribute = true)
        {
            if (component == null || component.GetType().IsPrimitive || component is string)
                return new PropertyDescriptorCollection(new PropertyDescriptor[] { });

            // Make sure we are inspecting the real component
            component = component.GetInnerObject();

            var type = component.GetType();

            var allprops = new Dictionary<string, PropertyDescriptor>();

            Action<PropertyInfo, bool> addPropertyDescriptor = (pi, isStatic) =>
            {
                try
                {
                    if (allprops.ContainsKey(pi.Name)) return;

                    var fullName = pi.DeclaringType.FullName + "." + pi.Name;
                    if (ExcludedProperties.Contains(fullName)) return;

                    if (isStatic) allprops.Add(pi.Name, new StaticPropertyPropertyDescriptor(type, pi, keepOriginalCategoryAttribute));
                    else allprops.Add(pi.Name, new InstancePropertyPropertyDescriptor(component, type, pi, keepOriginalCategoryAttribute));
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not convert a property info into a property descriptor: {ex.Message}", ex);
                }
            };

            int depth = 1;
            do
            {
                foreach (var pi in type.GetProperties(InstanceFlags))
                    addPropertyDescriptor(pi, false);

                if (retrieveStaticMembers)
                    foreach (var pi in type.GetProperties(StaticFlags))
                        addPropertyDescriptor(pi, true);

                if (type == typeof(object) || !inspectBaseClasses)
                    break;
                type = type.BaseType;
                depth++;
            }
            while (true);

            return new PropertyDescriptorCollection(allprops.Values.ToArray());
        }

        public static PropertyDescriptorCollection GetAllEvents(
            this ITypeDescriptorContext context,
            object component,
            bool inspectBaseClasses = true,
            bool retrieveStaticMembers = true,
            bool keepOriginalCategoryAttribute = true)
        {
            if (component == null || component.GetType().IsPrimitive || component is string)
                return new PropertyDescriptorCollection(new PropertyDescriptor[] { });

            // Make sure we are inspecting the real component
            component = component.GetInnerObject();

            var type = component.GetType();

            var allevs = new Dictionary<string, PropertyDescriptor>();

            Action<EventInfo, bool> addPropertyDescriptor = AddPropertyDescriptor(component, keepOriginalCategoryAttribute, allevs, type);

            int depth = 1;
            do
            {
                foreach (var ei in type.GetEvents(InstanceFlags))
                    addPropertyDescriptor(ei, false);

                if (retrieveStaticMembers)
                    foreach (var ei in type.GetEvents(StaticFlags))
                        addPropertyDescriptor(ei, true);

                if (type == typeof(object) || !inspectBaseClasses)
                    break;

                type = type.BaseType;
                depth++;
            }
            while (true);

            return new PropertyDescriptorCollection(allevs.Values.ToArray());
        }

        private static Action<EventInfo, bool> AddPropertyDescriptor(object component, bool keepOriginalCategoryAttribute, Dictionary<string, PropertyDescriptor> allevs, Type type)
        {
            return (ei, isStatic) =>
            {
                try
                {
                    if (allevs.ContainsKey(ei.Name)) return;

                    var fullName = ei.DeclaringType.FullName + "." + ei.Name;
                    if (ExcludedEvents.Contains(fullName)) return;

                    if (isStatic) allevs.Add(ei.Name, new StaticEventPropertyDescriptor(type, ei, keepOriginalCategoryAttribute));
                    else allevs.Add(ei.Name, new InstanceEventPropertyDescriptor(component, type, ei, keepOriginalCategoryAttribute));

                }
                catch (Exception ex)
                {
                    Log.Error($"Could not convert an event info into a property descriptor: {ex.Message}", ex);
                }
            };
        }

        #endregion

        #region PropertyInfo extensions

        public static object Get(this PropertyInfo propertyInfo, object target, ref string criticalError)
        {
            if (!string.IsNullOrEmpty(criticalError))
                return criticalError;

            try
            {
                var get = propertyInfo.GetGetMethod(true);
                if (get != null)
                    return get.Invoke(target, new object[] { });
                else criticalError = "No Get Method.";
            }
            catch (SecurityException ex)
            {
                criticalError = ex.Message;
            }
            catch (TargetException ex)
            {
                criticalError = ex.Message;
            }
            catch (TargetParameterCountException ex)
            {
                criticalError = ex.Message;
            }
            catch (TargetInvocationException ex)
            {
                criticalError = ex.InnerException?.Message;
            }
            return criticalError;
        }

        public static object Set(this PropertyInfo propertyInfo, object target, object value, ref string criticalError)
        {
            if (!string.IsNullOrEmpty(criticalError))
                return criticalError;

            try
            {
                var set = propertyInfo.GetSetMethod(true);
                if (set != null)
                    return set.Invoke(target, new object[] { value });
                else criticalError = "No Set Method.";
            }
            catch (SecurityException ex)
            {
                criticalError = ex.Message;
            }
            catch (TargetException ex)
            {
                criticalError = ex.Message;
            }
            catch (TargetParameterCountException ex)
            {
                criticalError = ex.Message;
            }
            catch (TargetInvocationException ex)
            {
                criticalError = ex.InnerException?.Message;
            }
            return criticalError;
        }

        #endregion
    }
}
