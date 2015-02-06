using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hawkeye.Scripting
{
	public static class RuntimeHelper
	{
		public static string[] Inspect(object o)
        {
            Type t = o.GetType();
            if (t != null)
            {
				PropertyInfo[] properties = GetProperties(t);
                //var members = t.GetMembers();
				MethodInfo[] methods = GetMethods(t);
                //var fields = t.GetFields(BindingFlags.Instance);
                EventInfo[] events = GetEvents(t);

                string[] result = new string[properties.Length + methods.Length + events.Length];

                int last = 0;

                for (int i = 0; i < properties.Length; i++)
			    {
                    object value = properties[i].GetValue(o, null);
                    
                    string valueString = "(null)";
                    if (value != null)
                        valueString = value.ToString();

                    result[i] = " [p] " + properties[i].Name + " = " + valueString;
                    
			    }

                last += properties.Length;

                for (int i = 0; i < methods.Length; i++)
			    {
                    //result[i + last] = "" [m] "" + methods[i].Name + ""("" + string.Join("", "", methods[i].GetParameters().OrderBy(p => p.Position).Select(p => p.ParameterType.Name + "" "" + p.Name))  + "")"";
					result[i + last] = " [m] " + methods[i].Name + "(" + string.Join(", ", GetMethodParameters(methods[i]))  + ")";
					//result[i + last] = "" [m] "" + methods[i].Name + ""()"";
			    }

                last += methods.Length;

                for (int i = 0; i < events.Length; i++)
			    {
                    result[i + last] = " [e] " + events[i].Name + "<" + events[i].EventHandlerType.Name + ">";
			    }

                return result;
            }

            return null;
        }

		public static MethodInfo[] GetMethods(Type t)
		{
			List<MethodInfo> result = new List<MethodInfo>();

			foreach (MethodInfo methodInfo in t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!GetIsPropertyGetterOrSetter(methodInfo))
					result.Add(methodInfo);
			}
			result.Sort(new MethodInfoComparer());

			return result.ToArray();
		}

		public static string[] GetMethodParameters(MethodInfo method)
		{
			// methods[i].GetParameters().OrderBy(p => p.Position).Select(p => p.ParameterType.Name + "" "" + p.Name))
			List<ParameterInfo> parameters = new List<ParameterInfo>(method.GetParameters());

			if (parameters.Count == 0)
				return new string[0];

			parameters.Sort(new ParameterInfoComparer());

			string[] result = new string[parameters.Count - 1];
			for (int i = 0; i < parameters.Count - 1; i++)
			{
				result[i] = parameters[i].ParameterType.Name + " " + parameters[i].Name;
			}

			return result;
		}

		public static PropertyInfo[] GetProperties(Type t)
		{
			List<PropertyInfo> result = new List<PropertyInfo>(t.GetProperties());

			result.Sort(new PropertyInfoComparer());

			return result.ToArray();
		}

		public static EventInfo[] GetEvents(Type t)
		{
			List<EventInfo> result = new List<EventInfo>(t.GetEvents());

			result.Sort(new EventInfoComparer());

			return result.ToArray();
		}

		public static bool GetIsPropertyGetterOrSetter(MethodInfo mi)
        {
            return mi.Name.StartsWith("set_") ||
                mi.Name.StartsWith("get_") ||
                mi.Name.StartsWith("add_") ||
                mi.Name.StartsWith("remove_");
        }
    }

	public class PropertyInfoComparer : IComparer<PropertyInfo>
	{
		public int Compare(PropertyInfo x, PropertyInfo y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	public class MethodInfoComparer : IComparer<MethodInfo>
	{
		public int Compare(MethodInfo x, MethodInfo y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	public class EventInfoComparer : IComparer<EventInfo>
	{
		public int Compare(EventInfo x, EventInfo y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	public class ParameterInfoComparer : IComparer<ParameterInfo>
	{
		public int Compare(ParameterInfo x, ParameterInfo y)
		{
			return x.Position.CompareTo(y.Position);
		}
	}
}
