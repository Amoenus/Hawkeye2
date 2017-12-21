using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace Hawkeye.ComponentModel
{
    internal class InstanceEventPropertyDescriptor : BaseEventPropertyDescriptor
    {
        private readonly object _component;
        private readonly bool _keepOriginalCategory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceEventPropertyDescriptor" /> class.
        /// </summary>
        /// <param name="instance">The component instance.</param>
        /// <param name="ownerType">Type of the owner.</param>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="keepOriginalCategoryAttribute">if set to <c>true</c> [keep original category attribute].</param>
        public InstanceEventPropertyDescriptor(object instance, Type ownerType, EventInfo eventInfo, bool keepOriginalCategoryAttribute = true)
            : base(ownerType, eventInfo)
        {
            _component = instance;
            _keepOriginalCategory = keepOriginalCategoryAttribute;
        }

        protected override void FillAttributes(IList attributeList)
        {
            base.FillAttributes(attributeList);
            if (!_keepOriginalCategory)
                attributeList.Add(new CategoryAttribute("(instance: " + ComponentType.Name + ")"));
        }

        protected override bool IsFiltered(Attribute attribute)
        {
            return !_keepOriginalCategory && attribute is CategoryAttribute;
        }
    }
}
