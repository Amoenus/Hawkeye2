using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Text.RegularExpressions;

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

		public static object Resolve(object startObj, string accessor)
		{
			string[] accessors = accessor.Split('.');

			if (accessors.Length == 0)
				return startObj;

			object nextObj = startObj;

			var sb = new StringBuilder();
			var lastMember = new StringBuilder();

			for (int i = 0; i < accessors.Length; i++)
			{
				string member = accessors[i];

				nextObj = GetAccessorValue(nextObj, member);

				//if (i == 0)
				//	sb.Append(member);
				//else
				//{
				//	if (lastMember.Length > 0)
				//		lastMember.Append(".");
				//	lastMember.Append(accessors[i - 1]);

				//	sb.AppendFormat(".GetType().GetProperty(\"{0}\").GetValue({1}, null)", member, lastMember.ToString());
				//}
			}

			return nextObj;
		}

		private static object GetAccessorValue(object obj, string member, object[] indexer = null)
		{
			return GetAccessorValue(obj, obj.GetType(), member, indexer);
		}

		private static object GetAccessorValue(object obj, Type t, string member, object[] indexer = null)
		{

			if (member.EndsWith(")"))
				return GetMethodValue(obj, t, member);

			if (member.EndsWith("]"))
				return GetIndexedValue(obj, t, member);

			var pi = t.GetProperty(member);
			if (pi != null)
				return pi.GetValue(obj, indexer);

			var fi = t.GetField(member);
			if (fi != null)
				return fi.GetValue(obj);

			//var mi = t.GetMethod(member);
			//if (mi != null)
			//	return mi.Invoke(obj, null);

			return null;
		}

		private static object GetMethodValue(object obj, Type t, string member)
		{
			var firstStop = member.IndexOf('(');
			var argumentsMatch = Regex.Match(member, @"\(.*(\w+).*\)");

			var methodName = member.Substring(0, firstStop);

			var mi = t.GetMethod(methodName);
			if (mi != null)
				return mi.Invoke(obj, null);

			return null;
		}

		private static object GetIndexedValue(object obj, Type t, string member)
		{
			var firstStop = member.IndexOf('[');
			object[] arguments = null;
			var argumentsMatch = Regex.Match(member, @"\[.*(\w+).*\]");
			if (argumentsMatch.Success)
			{
				string a = argumentsMatch.ToString();
				a = a.Substring(1, a.Length - 2);
				arguments = a.Split('.').Select(x => ChangeType(x)).ToArray();
			}

			member = member.Substring(0, firstStop);

			object resolvedObj = GetAccessorValue(obj, t, member);
			var memberInfo = resolvedObj.GetType().GetDefaultMembers().FirstOrDefault();
			if (memberInfo != null)
				return GetAccessorValue(resolvedObj, memberInfo.Name, arguments);

			return null;
		}

		private static object ChangeType(string value)
		{
			int a;
			if (int.TryParse(value, out a))
				return a;

			bool b;
			if (bool.TryParse(value, out b))
				return b;

			char c;
			if (char.TryParse(value, out c))
				return c;

			double d;
			if (double.TryParse(value, out d))
				return d;

			return value;
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
