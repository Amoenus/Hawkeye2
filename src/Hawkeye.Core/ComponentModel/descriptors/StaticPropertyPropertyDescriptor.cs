using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Hawkeye.ComponentModel
{
    internal class StaticPropertyPropertyDescriptor : BasePropertyPropertyDescriptor
    {
        private readonly bool _keepOriginalCategory;
        private readonly Type _type;
        private string _criticalGetError;
        private string _criticalSetError;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="StaticPropertyPropertyDescriptor" /> class.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="keepOriginalCategoryAttribute">
        ///     if set to <c>true</c> [keep original category attribute].
        /// </param>
        public StaticPropertyPropertyDescriptor(Type objectType, PropertyInfo propertyInfo,
            bool keepOriginalCategoryAttribute = true)
            : base(propertyInfo)
        {
            _type = objectType;
            _keepOriginalCategory = keepOriginalCategoryAttribute;
        }

        public override Type ComponentType => _type;

        public override bool IsReadOnly => !PropertyInfo.CanWrite;

        public override Type PropertyType => PropertyInfo.PropertyType;

        public override bool CanResetValue(object component)
        {
            return false;
        } //TODO: why should this be false?

        public override object GetValue(object component)
        {
            return !PropertyInfo.CanRead ? null : PropertyInfo.Get(null, ref _criticalGetError);
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            value = value.GetInnerObject(); // Make sure we are affecting a real object.
            object result = PropertyInfo.Set(component, value, ref _criticalSetError);

            //if ( WarningsHelper.Instance.SetPropertyWarning(propInfo.DeclaringType.Name, value) )
            //{
            //    CodeChangeLoggingSystem.Instance.LogSet(propInfo.DeclaringType.Name, propInfo.Name, value);
            //    propInfo.SetValue(null, value, new object[] {});
            //}
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        protected override void FillAttributes(IList attributeList)
        {
            base.FillAttributes(attributeList);
            if (!_keepOriginalCategory)
            {
                attributeList.Add(new CategoryAttribute("(static: " + _type.Name + ")"));
            }
        }

        protected override bool IsFiltered(Attribute attribute)
        {
            return !_keepOriginalCategory && attribute is CategoryAttribute;
        }
    }
}