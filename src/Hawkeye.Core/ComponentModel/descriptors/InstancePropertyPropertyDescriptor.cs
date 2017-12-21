using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Hawkeye.ComponentModel
{
    internal class InstancePropertyPropertyDescriptor : BasePropertyPropertyDescriptor
    {
        private readonly object _component;
        private readonly bool _keepOriginalCategory = true;
        private readonly Type _type;
        private string _criticalGetError;
        private string _criticalSetError;

        /// <summary>
        ///     Initializes a new <paramref name="instance" /> of the
        ///     <see cref="InstancePropertyPropertyDescriptor" /> class.
        /// </summary>
        /// <param name="instance">
        ///     The
        ///     <see cref="Hawkeye.ComponentModel.InstancePropertyPropertyDescriptor._component" />
        ///     instance.
        /// </param>
        /// <param name="ownerType">Type of the owner.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="keepOriginalCategoryAttribute">
        ///     if set to <c>true</c> [keep original category attribute].
        /// </param>
        public InstancePropertyPropertyDescriptor(object instance, Type ownerType, PropertyInfo propertyInfo,
            bool keepOriginalCategoryAttribute = true)
            : base(propertyInfo)
        {
            _type = ownerType;
            _component = instance;
            _keepOriginalCategory = keepOriginalCategoryAttribute;
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether
        ///     this property is read-only.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the property is read-only; otherwise,
        ///     false.
        /// </returns>
        public override bool IsReadOnly => !PropertyInfo.CanWrite;

        /// <summary>
        ///     When overridden in a derived class, gets the
        ///     <see cref="Hawkeye.ComponentModel.InstancePropertyPropertyDescriptor._type" />
        ///     of the
        ///     <see cref="Hawkeye.ComponentModel.InstancePropertyPropertyDescriptor._component" />
        ///     this property is bound to.
        /// </summary>
        /// <exception cref="NotImplementedException" />
        /// <returns>
        ///     A <see cref="Type" /> that represents the
        ///     <see cref="Hawkeye.ComponentModel.InstancePropertyPropertyDescriptor._type" />
        ///     of
        ///     <see cref="Hawkeye.ComponentModel.InstancePropertyPropertyDescriptor._component" />
        ///     this property is bound to. When the
        ///     <see cref="PropertyDescriptor.GetValue" /> or
        ///     <see cref="PropertyDescriptor.SetValue" /> methods are invoked, the
        ///     object specified might be an instance of this type.
        /// </returns>
        public override Type ComponentType => PropertyInfo.PropertyType;

        /// <summary>
        ///     When overridden in a derived class, gets the
        ///     <see cref="Hawkeye.ComponentModel.InstancePropertyPropertyDescriptor._type" />
        ///     of the property.
        /// </summary>
        /// <returns>
        ///     A <see cref="Type" /> that represents the
        ///     <see cref="Hawkeye.ComponentModel.InstancePropertyPropertyDescriptor._type" />
        ///     of the property.
        /// </returns>
        public override Type PropertyType =>
            PropertyInfo.PropertyType == typeof(object) ? typeof(string) : PropertyInfo.PropertyType;

        /// <summary>
        ///     When overridden in a derived class, gets the current value of the
        ///     property on a component.
        /// </summary>
        /// <param name="component">
        ///     The component with the property for which to retrieve the value.
        /// </param>
        /// <exception cref="NotImplementedException" />
        /// <returns>
        ///     The value of a property for a given component.
        /// </returns>
        public override object GetValue(object component)
        {
            component = component.GetInnerObject(); // Make sure we are working on a real object.
            return !PropertyInfo.CanRead ? null : PropertyInfo.Get(component, ref _criticalGetError);
        }

        public override void SetValue(object component, object value)
        {
            component = component.GetInnerObject(); // Make sure we are working on a real object0
            value = value.GetInnerObject(); // Make sure we are affecting a real object.
            PropertyInfo.Set(component, value, ref _criticalSetError);

            //if (WarningsHelper.Instance.SetPropertyWarning(propInfo.DeclaringType.Name, value))
            //{
            //    //LoggingSystem.Instance.LogSet(propInfo.DeclaringType.Name, propInfo.Name, value);
            //    CodeChangeLoggingSystem.Instance.LogSet(HawkeyeUtils.GetControlName(component), propInfo.Name, value);
            //    propInfo.SetValue(component, value, new object[] { });
            //}
        }

        protected override void FillAttributes(IList attributeList)
        {
            base.FillAttributes(attributeList);
            if (!_keepOriginalCategory)
            {
                attributeList.Add(new CategoryAttribute("(instance: " + _type.Name + ")"));
            }
        }

        protected override bool IsFiltered(Attribute attribute)
        {
            return !_keepOriginalCategory && attribute is CategoryAttribute;
        }
    }
}