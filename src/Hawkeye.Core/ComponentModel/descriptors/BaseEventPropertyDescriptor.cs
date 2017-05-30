using System;
using System.Reflection;

namespace Hawkeye.ComponentModel
{
    internal abstract class BaseEventPropertyDescriptor : BaseMemberPropertyDescriptor
    {
        private readonly Type type;
        private readonly EventInfo einfo;

        public BaseEventPropertyDescriptor(Type objectType, EventInfo eventInfo)
            : base(eventInfo, eventInfo.Name)
        {
            type = objectType;
            einfo = eventInfo;
        }

        protected EventInfo EventInfo => einfo;

        public override Type ComponentType => type;

        public override object GetValue(object component)
        {
            return null;
        }

        public override bool IsReadOnly => true;

        public override Type PropertyType => einfo.EventHandlerType;
    }
}
