using System;
using System.Linq;
using System.Reflection;

namespace Hawkeye.Reflection
{
    internal class FieldAccessor
    {
        private static readonly BindingFlags[] FlagsToExamine;

        private readonly string _name;
        private readonly Type _targetType;
        private readonly object _target;
        private readonly FieldInfo _info;

        /// <summary>
        /// Initializes the <see cref="FieldAccessor"/> class.
        /// </summary>
        static FieldAccessor()
        {
            FlagsToExamine = new[]
            {
                BindingFlags.Default,
                BindingFlags.Instance | BindingFlags.FlattenHierarchy,
                BindingFlags.Static | BindingFlags.FlattenHierarchy,

                BindingFlags.NonPublic | BindingFlags.Instance,

                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.GetField,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,

                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn | BindingFlags.Instance | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty | BindingFlags.SetField | BindingFlags.Static,

                BindingFlags.NonPublic | BindingFlags.Static,
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.GetField,
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAccessor"/> class.
        /// </summary>
        /// <param name="fieldTarget">The target.</param>
        /// <param name="fieldName">Name of the field.</param>
        public FieldAccessor(object fieldTarget, string fieldName) :
            this(fieldTarget.GetType(), fieldTarget, fieldName) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAccessor"/> class.
        /// </summary>
        /// <param name="fieldTargetType">Type of the target.</param>
        /// <param name="fieldName">Name of the field.</param>
        public FieldAccessor(Type fieldTargetType, string fieldName) :
            this(fieldTargetType, null, fieldName) { }

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldAccessor"/> class from being created.
        /// </summary>
        /// <param name="fieldTargetType">Type of the target.</param>
        /// <param name="fieldTarget">The target.</param>
        /// <param name="fieldName">Name of the field.</param>
        private FieldAccessor(Type fieldTargetType, object fieldTarget, string fieldName)
        {
            _target = fieldTarget;
            _targetType = fieldTargetType;
            _name = fieldName;

            do
            {
                foreach (var flagToExamine in FlagsToExamine)
                {
                    var candidate = FindField(flagToExamine);
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
            return _info.GetValue(operationTarget ?? _target);
        }

        public void Set(object newValue, object operationTarget = null)
        {
            _info.SetValue(operationTarget ?? _target, newValue);
        }

        private FieldInfo FindField(BindingFlags flags)
        {
            var fieldInfo = _targetType.GetField(_name, flags);
            if (fieldInfo != null)
                return fieldInfo;

            var allFields = _targetType.GetFields(flags);

            return allFields.FirstOrDefault(field => field.Name == _name);
        }
    }
}
