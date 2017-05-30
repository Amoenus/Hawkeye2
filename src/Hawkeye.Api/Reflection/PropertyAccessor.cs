using System;
using System.Reflection;

namespace Hawkeye.Reflection
{
    internal class PropertyAccessor
    {
        private static readonly BindingFlags[] FlagsToExamine;

        private readonly string _name;
        private readonly Type _targetType;
        private readonly object _target;
        private readonly PropertyInfo _info;
        
        /// <summary>
        /// Initializes the <see cref="PropertyAccessor"/> class.
        /// </summary>
        static PropertyAccessor()
        {
            FlagsToExamine = new[]
            {
                BindingFlags.Default,
                BindingFlags.Instance | BindingFlags.FlattenHierarchy,
                BindingFlags.Static | BindingFlags.FlattenHierarchy,
                
                BindingFlags.NonPublic | BindingFlags.Instance,

                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetProperty,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty,
                
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn | BindingFlags.Instance | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty | BindingFlags.SetProperty | BindingFlags.Static,

                BindingFlags.NonPublic | BindingFlags.Static,
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.GetProperty,
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty                
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessor"/> class.
        /// </summary>
        /// <param name="propertyTarget">The property target.</param>
        /// <param name="propertyName">Name of the property.</param>
        public PropertyAccessor(object propertyTarget, string propertyName) :
            this(propertyTarget.GetType(), propertyTarget, propertyName) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessor"/> class.
        /// </summary>
        /// <param name="propertyTargetType">Type of the property target.</param>
        /// <param name="propertyName">Name of the property.</param>
        public PropertyAccessor(Type propertyTargetType, string propertyName) :
            this(propertyTargetType, null, propertyName) { }

        /// <summary>
        /// Prevents a default instance of the <see cref="PropertyAccessor"/> class from being created.
        /// </summary>
        /// <param name="propertyTargetType">Type of the property target.</param>
        /// <param name="propertyTarget">The property target.</param>
        /// <param name="propertyName">Name of the property.</param>
        private PropertyAccessor(Type propertyTargetType, object propertyTarget, string propertyName)
        {
            _target = propertyTarget;
            _targetType = propertyTargetType;
            _name = propertyName;

            do
            {
                foreach (var flagToExamine in FlagsToExamine)
                {
                    var candidate = FindProperty(flagToExamine);
                    if (candidate != null)
                    {
                        _info = candidate;
                        break;
                    }
                }

                if (_info == null)
                {
                    _targetType = _targetType.BaseType;
                    if (_targetType == typeof(object))
                        break;
                }

            } while (_info == null);
        }

        public object Target => _target;

        public bool IsValid => _info != null;

        public object Get(object operationTarget = null)
        {
            return _info.GetValue(operationTarget ?? _target, null);
        }

        public void Set(object newValue, object operationTarget = null)
        {
            _info.SetValue(operationTarget ?? _target, newValue, null);
        }

        private PropertyInfo FindProperty(BindingFlags flags)
        {
            var propertyInfo = _targetType.GetProperty(_name, flags);
            if (propertyInfo != null)
                return propertyInfo;

            var allProperties = _targetType.GetProperties(flags);
            foreach (var pinfo in allProperties)
            {
                if (pinfo.Name == _name)
                    return pinfo;
            }

            return null;
        }
    }
}
