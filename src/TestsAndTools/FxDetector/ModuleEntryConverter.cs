using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FxDetector
{
    /// <summary>
    ///     A <see cref="TypeConverter" /> allowing to display objects of type
    ///     <see cref="NativeMethods.MODULEENTRY32" /> in a
    ///     <see cref="System.Windows.Forms.PropertyGrid" /> .
    /// </summary>
    internal class ModuleEntryConverter : ExpandableObjectConverter
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
            Attribute[] attributes)
        {
            Type type = typeof(NativeMethods.MODULEENTRY32);
            ModuleEntryFieldDescriptor[] descriptors = type.GetFields(bindingFlags)
                .Select(info =>
                    new ModuleEntryFieldDescriptor(
                        info.Name,
                        info.GetCustomAttributes(false)
                            .Cast<Attribute>()
                            .ToArray(),
                        info.FieldType))
                .ToArray();

            return new PropertyDescriptorCollection(descriptors);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private class ModuleEntryFieldDescriptor : SimplePropertyDescriptor
        {
            private readonly string _fieldName;
            private Type _fieldType;

            public ModuleEntryFieldDescriptor(string name, Attribute[] attributes, Type type)
                : base(typeof(NativeMethods.MODULEENTRY32), name, type, attributes)
            {
                _fieldName = name;
                _fieldType = type;
            }

            /// <summary>
            ///     Always read-only!
            /// </summary>
            public override bool IsReadOnly => true;

            /// <summary>
            ///     When overridden in a derived class, gets the current value of
            ///     the property on a component.
            /// </summary>
            /// <param name="component">
            ///     The component with the property for which to retrieve the value.
            /// </param>
            /// <returns>
            ///     The value of a property for a given component.
            /// </returns>
            public override object GetValue(object component)
            {
                if (component == null)
                {
                    return null;
                }

                Type type = component.GetType();

                FieldInfo field = type.GetField(_fieldName, bindingFlags);

                return field?.GetValue(component);
            }

            /// <summary>
            ///     Not supported.
            /// </summary>
            /// <param name="component"></param>
            /// <param name="value"></param>
            public override void SetValue(object component, object value)
            {
                throw new NotSupportedException();
            }
        }
    }
}