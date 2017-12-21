using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hawkeye.Scripting
{
    public static class RuntimeHelper
    {
        public static string[] Inspect(object o)
        {
            Type t = o?.GetType();
            if (t == null)
            {
                return null;
            }

            var list = new List<string>();
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            IOrderedEnumerable<MemberInfo> members =
                t.GetMembers(bindingFlags).OrderBy(m => m.MemberType).ThenBy(m => m.Name);
            MethodInfo[] methods = t.GetMethods(bindingFlags);

            int longestMemberCategory = members.Max(m => m.MemberType.ToString().Length);

            foreach (MemberInfo member in members)
            {
                var isPrivate = true;

                PropertyInfo pi = member.GetType().GetProperty("BindingFlags", bindingFlags);
                if (pi != null)
                {
                    var memberBindingFlags = (BindingFlags) pi.GetValue(member, null);
                    isPrivate = (memberBindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic;
                }

                var shouldSkip = false;
                string memberName = member.Name;

                if (member.MemberType == MemberTypes.Method)
                {
                    shouldSkip = true;
                    if (!GetIsPropertyGetterOrSetter(memberName))
                    {
                        MethodInfo[] methodInfos = methods.Where(m => m.Name == memberName).ToArray();

                        foreach (MethodInfo mi in methodInfos)
                        {
                            memberName += "(" + string.Join(", ", GetMethodParameters(mi)) + ")";
                            AddMember(ref list, longestMemberCategory, MemberTypes.Method, isPrivate, memberName);
                        }
                    }
                }

                if (!shouldSkip)
                {
                    AddMember(ref list, longestMemberCategory, member, isPrivate, memberName);
                }
            }

            return list.ToArray();

        }

        private static void AddMember(ref List<string> list, int longestMemberCategory, MemberInfo member,
            bool isPrivate, string memberName)
        {
            AddMember(ref list, longestMemberCategory, member.MemberType, isPrivate, memberName);
        }

        private static void AddMember(ref List<string> list, int longestMemberCategory, MemberTypes memberType,
            bool isPrivate, string memberName)
        {
            string memberCategory = (memberType + ":").PadRight(longestMemberCategory + 1, ' ');
            list.Add($"[{(isPrivate ? "-" : "+")}] {memberCategory} {memberName}");
        }

        private static string getMemberValueString(object value)
        {
            if (value != null)
            {
                if (value is char && char.IsControl((char) value))
                {
                    return "0x" + Convert.ToByte((char) value).ToString("X2");
                }

                return value.ToString().Replace(@"\", @"\\");
            }

            return "(null)";
        }

        public static object Resolve(object startObj, string accessor)
        {
            string[] accessors = accessor.Split('.');

            if (accessors.Length == 0)
            {
                return startObj;
            }

            object nextObj = startObj;

            foreach (string accsr in accessors)
            {
                string member = accsr;

                if (member.StartsWith("!"))
                {
                    member = member.Substring(1);
                }

                nextObj = GetAccessorValue(nextObj, member);
            }

            return nextObj;
        }

        public static List<string> Resolve2(string line)
        {
            /* TODO
             * ASSIGNMENTS WONT WORK
             * abc.!Name = def.!Name;
             *
             * METHODS WONT WORK
             * ?abc.!GetType().!Name (arguments?!)
            */

            line = line.Trim();
            if (line.EndsWith(";"))
            {
                line = line.Substring(0, line.Length - 1);
            }

            if (!line.Contains(".!"))
            {
                return new List<string> {line};
            }

            var result = new List<string>();


            string[] expressions = line.Split('=').Select(s => s.Trim()).ToArray();
            var resultExpressions = new string[expressions.Length];

            // not supported -> "abc.!Name = def.!Name = ghi.!Name"
            if (expressions.Length > 2)
            {
                throw new NotSupportedException(
                    "Forced member access is not supported within codelines with more than one value assignment!");
            }

            for (var i = 0; i < expressions.Length; i++)
            {
                bool isAssignment = expressions.Length == 2 && i == 0;

                string[] accessors = expressions[i].Split('.');

                string prevAccessor = accessors[0];
                for (var x = 1; x < accessors.Length; x++)
                {
                    bool isLastAccessor = x == accessors.Length - 1;

                    if (accessors[x].StartsWith("!"))
                    {
                        prevAccessor = GetAccessorValueString(prevAccessor, accessors[x].Substring(1), isAssignment,
                            isLastAccessor, ref result);
                    }
                    else
                    {
                        prevAccessor = GetAccessorValueString(prevAccessor, accessors[x], isAssignment, isLastAccessor,
                            ref result);
                    }
                    //prevAccessor += "." + accessors[x];

                    resultExpressions[i] = prevAccessor;
                }
            }

            string lastLine = null;
            lastLine = resultExpressions.Length == 1
                ? resultExpressions[0]
                : resultExpressions[0].Replace("%DYNAMIC_GET_EXPRESSION%", resultExpressions[1]);

            result.Add(lastLine);

            return result;
        }

        private static string GetAccessorValueString(string objectName, string accessor, bool isAssignment,
            bool isLastAccessor, ref List<string> lines)
        {
            //string methodName = isAssignment ? "SetMemberValue" : "GetMemberValue";
            //return string.Format("{0}(\"{1}\")", methodName, accessor);

            if (isAssignment && isLastAccessor)
            {
                return $"{GenerateReflectionWriteAccess(objectName, accessor, ref lines)}";
            }

            return GenerateReflectionReadAccess(objectName, accessor, ref lines);
        }

        private static string GenerateReflectionWriteAccess(string objectName, string accessor, ref List<string> lines)
        {
            string accessorWithQuotes = "\"" + accessor + "\"";

            string notFoundMessage = $"\"Could not resolve member \\\"{accessor}\\\"\"";
            return string.Format(
                "GetType().GetProperty({2}) == null ? {0} : {1}.GetType().GetProperty({2}).SetValue({1}, %DYNAMIC_GET_EXPRESSION%)",
                notFoundMessage, objectName, accessorWithQuotes);
        }

        private static string GenerateReflectionReadAccess(string objectName, string accessor, ref List<string> lines)
        {
            string typeVar = "T" + Guid.NewGuid().ToString("n");
            string propertyVar = "P" + Guid.NewGuid().ToString("n");
            string methodVar = "M" + Guid.NewGuid().ToString("n");
            string fieldVar = "F" + Guid.NewGuid().ToString("n");
            string resultVar = "R" + Guid.NewGuid().ToString("n");


            string accessorWithQuotes = "\"" + accessor + "\"";

            lines.Add($"System.Type {typeVar} = {objectName}.GetType();");
            lines.Add(
                $"System.Reflection.PropertyInfo {propertyVar} = {typeVar}.GetProperty({accessorWithQuotes}, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);");
            lines.Add($"System.Reflection.MethodInfo {methodVar} = {typeVar}.GetMethod({accessorWithQuotes});");
            lines.Add(
                $"System.Reflection.FieldInfo {fieldVar} = {typeVar}.GetField({accessorWithQuotes}, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);");

            string notFoundMessage = $"\"Could not resolve member \\\"{accessor}\\\"\"";
            //return string.Format("GetType().GetProperty({2}) == null ? {0} : {1}.GetType().GetProperty({2}).GetValue({1}, null)", notFoundMessage, objectName, "\"" + accessor + "\"");
            string exp =
                string.Format(
                    "({0} != null ? {0}.GetValue({3}, null) : ({1} != null ? {1}.Invoke({3}, new System.Object[0]) : ({2} != null ? {2}.GetValue({3}) : null)));",
                    propertyVar, methodVar, fieldVar, objectName);
            lines.Add($"object {resultVar} = {exp}");
            return resultVar;
        }


        private static object GetAccessorValue(object obj, string member, object[] indexer = null)
        {
            return GetAccessorValue(obj, obj.GetType(), member, indexer);
        }

        private static object GetAccessorValue(object obj, Type t, string member, object[] indexer = null)
        {
            if (member.EndsWith(")"))
            {
                return GetMethodValue(obj, t, member);
            }

            if (member.EndsWith("]"))
            {
                return GetIndexedValue(obj, t, member);
            }

            PropertyInfo pi = t.GetProperty(member);
            if (pi != null)
            {
                return pi.GetValue(obj, indexer);
            }

            FieldInfo fi = t.GetField(member);

            return fi?.GetValue(obj);

            //var mi = t.GetMethod(member);
            //if (mi != null)
            //	return mi.Invoke(obj, null);
        }

        private static object GetMethodValue(object obj, Type t, string member)
        {
            int firstStop = member.IndexOf('(');
            Match argumentsMatch = Regex.Match(member, @"\(.*(\w+).*\)");

            string methodName = member.Substring(0, firstStop);

            MethodInfo mi = t.GetMethod(methodName);
            return mi?.Invoke(obj, null);
        }

        private static object GetIndexedValue(object obj, Type t, string member)
        {
            int firstStop = member.IndexOf('[');
            object[] arguments = null;
            Match argumentsMatch = Regex.Match(member, @"\[.*(\w+).*\]");
            if (argumentsMatch.Success)
            {
                string a = argumentsMatch.ToString();
                a = a.Substring(1, a.Length - 2);
                arguments = a.Split('.').Select(ChangeType).ToArray();
            }

            member = member.Substring(0, firstStop);

            object resolvedObj = GetAccessorValue(obj, t, member);
            MemberInfo memberInfo = resolvedObj.GetType().GetDefaultMembers().FirstOrDefault();
            return memberInfo != null ? GetAccessorValue(resolvedObj, memberInfo.Name, arguments) : null;
        }

        private static object ChangeType(string value)
        {
            if (int.TryParse(value, out int a))
            {
                return a;
            }

            if (bool.TryParse(value, out bool b))
            {
                return b;
            }

            if (char.TryParse(value, out char c))
            {
                return c;
            }

            if (double.TryParse(value, out double d))
            {
                return d;
            }

            return value;
        }

        public static MethodInfo[] GetMethods(Type t)
        {
            var result = new List<MethodInfo>();

            foreach (MethodInfo methodInfo in t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!GetIsPropertyGetterOrSetter(methodInfo.Name))
                {
                    result.Add(methodInfo);
                }
            }

            return result.ToArray();
        }

        public static string[] GetMethodParameters(MethodInfo method)
        {
            // methods[i].GetParameters().OrderBy(p => p.Position).Select(p => p.ParameterType.Name + "" "" + p.Name))
            List<ParameterInfo> parameters = method.GetParameters().OrderBy(p => p.Position).ToList();

            if (parameters.Count == 0)
            {
                return new string[0];
            }

            var result = new string[parameters.Count - 1];
            for (var i = 0; i < parameters.Count - 1; i++)
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