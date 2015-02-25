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
				var list = new List<string>();
				var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
				
				var members = t.GetMembers(bindingFlags).OrderBy(m => m.MemberType).ThenBy(m => m.Name);
				var methods = t.GetMethods(bindingFlags);

				var longestMemberCategory = members.Max(m => m.MemberType.ToString().Length);

				foreach (var member in members)
				{
					BindingFlags memberBindingFlags = (BindingFlags)member.GetType().GetProperty("BindingFlags", bindingFlags).GetValue(member, null);
					bool isPrivate = (memberBindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic;

					bool skip = false;
					string memberName = member.Name;

					if (member.MemberType == MemberTypes.Method)
					{
						skip = true;
						if (!GetIsPropertyGetterOrSetter(memberName))
						{
							var methodInfos = methods.Where(m => m.Name == memberName).ToArray();

							foreach (var mi in methodInfos)
							{
								memberName += "(" + string.Join(", ", GetMethodParameters(mi)) + ")";
								AddMember(ref list, longestMemberCategory, MemberTypes.Method, isPrivate, memberName);
							}
						}
					}

					if (!skip)
						AddMember(ref list, longestMemberCategory, member, isPrivate, memberName);
				}

				return list.ToArray();
            }

            return null;
        }

		private static void AddMember(ref List<string> list, int longestMemberCategory, MemberInfo member, bool isPrivate, string memberName)
		{
			AddMember(ref list, longestMemberCategory, member.MemberType, isPrivate, memberName);
		}

		private static void AddMember(ref List<string> list, int longestMemberCategory, MemberTypes memberType, bool isPrivate, string memberName)
		{
			string memberCategory = (memberType.ToString() + ":").PadRight(longestMemberCategory + 1, ' ');
			list.Add(string.Format("[{0}] {1} {2}", isPrivate ? "-" : "+", memberCategory, memberName));
		}

		private static string getMemberValueString(object value)
		{
			if (value != null)
			{
				if (value is char && char.IsControl((char)value))
					return "0x" + Convert.ToByte((char)value).ToString("X2");
				else
					return value.ToString().Replace(@"\", @"\\");
			}
			return "(null)";
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
				if (!GetIsPropertyGetterOrSetter(methodInfo.Name))
					result.Add(methodInfo);
			}

			return result.ToArray();
		}

		public static string[] GetMethodParameters(MethodInfo method)
		{
			// methods[i].GetParameters().OrderBy(p => p.Position).Select(p => p.ParameterType.Name + "" "" + p.Name))
			List<ParameterInfo> parameters = method.GetParameters().OrderBy(p => p.Position).ToList();

			if (parameters.Count == 0)
				return new string[0];

			string[] result = new string[parameters.Count - 1];
			for (int i = 0; i < parameters.Count - 1; i++)
			{
				result[i] = parameters[i].ParameterType.Name + " " + parameters[i].Name;
			}

			return result;
		}

		public static bool GetIsPropertyGetterOrSetter(string methodName)
        {
			return methodName.StartsWith("set_") ||
				methodName.StartsWith("get_") ||
				methodName.StartsWith("add_") ||
				methodName.StartsWith("remove_");
        }
    }
}
