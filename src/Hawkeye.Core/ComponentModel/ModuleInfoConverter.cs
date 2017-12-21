using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Hawkeye.ComponentModel
{
    internal class ModuleInfoConverter : ExpandableObjectConverter
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
            Attribute[] attributes)
        {
            Type type = typeof(IModuleInfo);
            ModuleInfoPropertyDescriptor[] descriptors = type.GetProperties(bindingFlags)
                .Select(info =>
                    new ModuleInfoPropertyDescriptor(
                        info.Name,
                        info.GetCustomAttributes(false)
                            .Cast<Attribute>()
                            .ToArray(),
                        info.PropertyType))
                .ToArray();

            return new PropertyDescriptorCollection(descriptors);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private class ModuleInfoPropertyDescriptor : SimplePropertyDescriptor
        {
            private readonly string _propName;

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the
            ///         <see cref="ModuleInfoConverter.ModuleInfoPropertyDescriptor" />
            ///     </para>
            ///     <para>class.</para>
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="attributes">The attributes.</param>
            /// <param name="type">The type.</param>
            /// <inheritdoc />
            public ModuleInfoPropertyDescriptor(string name, Attribute[] attributes, Type type)
                : base(typeof(IModuleInfo), name, type, attributes)
            {
                _propName = name;
            }

            /// <inheritdoc />
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
                Type type = component?.GetType();

                PropertyInfo propertyInfo = type?.GetProperty(_propName, bindingFlags);

                MethodInfo methodInfo = propertyInfo?.GetGetMethod();

                return methodInfo?.Invoke(component, null);
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