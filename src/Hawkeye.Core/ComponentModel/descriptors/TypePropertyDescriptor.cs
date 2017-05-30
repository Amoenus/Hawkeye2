using System;
using System.ComponentModel;

namespace Hawkeye.ComponentModel
{
    internal class TypePropertyDescriptor : PropertyDescriptor
    {
        private Type ownerType = null;
        private const string propertyName = "(Type)";

        public TypePropertyDescriptor(object ownerObject)
            : base(propertyName, null)
        {
            if (ownerObject == null) throw new ArgumentNullException(nameof(ownerObject));
            ownerType = ownerObject.GetType();
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType => ownerType;

        public override object GetValue(object component)
        {
            return ownerType;
        }

        public override bool IsReadOnly => true;

        public override Type PropertyType => typeof(Type);

        public override void ResetValue(object component)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object component, object value)
        {
            throw new NotSupportedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
