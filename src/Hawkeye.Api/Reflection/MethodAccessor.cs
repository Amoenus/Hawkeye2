using System;
using System.Reflection;

namespace Hawkeye.Reflection
{
    internal class MethodAccessor
    {
        private static readonly BindingFlags[] FlagsToExamine;

        private readonly string _name;
        private readonly Type _targetType;
        private readonly MethodInfo _info;

        /// <summary>
        /// Initializes the <see cref="MethodAccessor"/> class.
        /// </summary>
        static MethodAccessor()
        {
            FlagsToExamine = new[]
            {
                BindingFlags.NonPublic | BindingFlags.Instance,
                BindingFlags.Public | BindingFlags.Instance,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAccessor"/> class.
        /// </summary>
        /// <param name="methodTargetType">Type of the method target.</param>
        /// <param name="methodName">Name of the method.</param>
        public MethodAccessor(Type methodTargetType, string methodName)
        {
            _name = methodName;
            _targetType = methodTargetType;

            do
            {
                foreach (var flagToExamine in FlagsToExamine)
                {
                    var candidate = FindMethod(flagToExamine);
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

        public bool IsValid => _info != null;

        public object Invoke(object target, object[] param)
        {
            return _info.Invoke(target, param);
        }

        public object Invoke(object target)
        {
            return Invoke(target, new object[] { });
        }

        public object Invoke(object target, object oneParam)
        {
            return Invoke(target, new object[] { oneParam });
        }

        private MethodInfo FindMethod(BindingFlags flags)
        {
            return _targetType.GetMethod(_name, flags);
        }
    }
}
