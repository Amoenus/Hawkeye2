using System;
using System.ComponentModel;

namespace Hawkeye.ComponentModel
{
    internal class TypePropertyDescriptor : PropertyDescriptor
    {
        private const string PropertyName = "(Type)";
        private readonly Type _ownerType;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="TypePropertyDescriptor" /> class.
        /// </summary>
        /// <param name="ownerObject">The owner object.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="ownerObject" />
        /// </exception>
        /// <inheritdoc />
        public TypePropertyDescriptor(object ownerObject)
            : base(PropertyName, null)
        {
            if (ownerObject == null)
            {
                throw new ArgumentNullException(nameof(ownerObject));
            }

            _ownerType = ownerObject.GetType();
        }

        /// <inheritdoc />
        public override Type ComponentType => _ownerType;

        /// <inheritdoc />
        public override bool IsReadOnly => true;

        /// <inheritdoc />
        public override Type PropertyType => typeof(Type);

        /// <inheritdoc />
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <inheritdoc />
        public override object GetValue(object component)
        {
            return _ownerType;
        }

        /// <inheritdoc />
        public override void ResetValue(object component)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void SetValue(object component, object value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}