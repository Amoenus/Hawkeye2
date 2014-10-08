using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hawkeye.Scripting
{
    public interface IScriptLoggerHost
    {
        void Execute(IScriptLogger logger);    
    }

    public interface IScriptLogger
    {
        void InitLog();

        void TryLog(string expression, object value);

        void EndLog();

        void ShowErrors(params ScriptError[] errors);

        void ShowErrors(params Exception[] errors);
    }

    public class ScriptError
    {
        public string Message { get; set; }
        public int Line { get; set; }
    }

    public static class ScriptLoggerSource
    {
        private const string SOURCE = @"
using System;
//using System.IO;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
%USINGS%

namespace Hawkeye.Scripting
{
    public class DynamicScriptLogger : IScriptLoggerHost
    {
        public DynamicScriptLogger()
            : base()
        {
        }

        public void Execute(IScriptLogger logger)
        {
            try
            {

                logger.InitLog();
        
%LINES%
            }
            catch(Exception ex)
            {
                logger.ShowErrors(ex);
            }
            finally
            {
                logger.EndLog();
            }
        }

        private string[] Inspect(object o)
        {
            var t = o.GetType();
            if (t != null)
            {
                var properties = t.GetProperties().OrderBy(p => p.Name).ToArray();
                //var members = t.GetMembers();
                var methods = t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => !isPropertyGetterOrSetter(p)).OrderBy(p => p.Name).ToArray();
                //var fields = t.GetFields(System.Reflection.BindingFlags.Instance);
                var events = t.GetEvents().OrderBy(p => p.Name).ToArray();

                string[] result = new string[properties.Length + methods.Length + events.Length];

                int last = 0;

                for (int i = 0; i < properties.Length; i++)
			    {
                    object value = properties[i].GetValue(o);
                    
                    string valueString = ""(null)"";
                    if (value != null)
                        valueString = value.ToString();

                    result[i] = "" [p] "" + properties[i].Name + "" = "" + valueString;
                    
			    }

                last += properties.Length;

                for (int i = 0; i < methods.Length; i++)
			    {
                    result[i + last] = "" [m] "" + methods[i].Name + ""("" + string.Join("", "", methods[i].GetParameters().OrderBy(p => p.Position).Select(p => p.ParameterType.Name + "" "" + p.Name))  + "")"";
			    }

                last += methods.Length;

                for (int i = 0; i < events.Length; i++)
			    {
                    result[i + last] = "" [e] "" + events[i].Name + ""<"" + events[i].EventHandlerType.Name + "">"";
			    }

                return result;
            }

            return null;
        }

        private bool isPropertyGetterOrSetter(System.Reflection.MethodInfo mi)
        {
            return mi.Name.StartsWith(""set_"") ||
                mi.Name.StartsWith(""get_"") ||
                mi.Name.StartsWith(""add_"") ||
                mi.Name.StartsWith(""remove_"");
        }
    }
}
";


        public static string GetSource(string[] lines)
        {
            List<string> additionalUsings = new List<string>();

            string indent = "\t\t";

            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                if (line == null)
                    continue;

				if (string.IsNullOrEmpty(line.Trim()))
					continue;

                string expressionString = line.TrimStart();
                string valueString = expressionString;

                if (expressionString.StartsWith("//"))
                    continue;

                if (expressionString.Length > 1 && expressionString.StartsWith("!"))
                {
                    string codeString = expressionString.Substring(1);
                    if (!codeString.TrimEnd().EndsWith(";", StringComparison.OrdinalIgnoreCase))
                        codeString += ";";
                    sb.AppendLine(indent + codeString);
                }
                else if (expressionString.Length > 1 && expressionString.StartsWith("#"))
                {
                    string usingString = expressionString.Substring(1).Trim();
                    if (!usingString.StartsWith("using", StringComparison.OrdinalIgnoreCase))
                        usingString = "using " + usingString;
                    if (!usingString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                        usingString += ";";
                    additionalUsings.Add(usingString);
                }
                else
                {
                    if (line.Contains("::"))
                    {
                        string[] parts = line.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 2)
                        {
                            expressionString = parts[0].Trim();
                            valueString = parts[1].TrimStart();
                        }
                    }
                    else if (expressionString.Length > 1 && expressionString.StartsWith("?"))
                    {
                        string viewString = expressionString.Substring(1).Trim();

                        expressionString = "Info: " + viewString;

                        if (!viewString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                            viewString += ";";

                        var end = viewString.LastIndexOf(';');
                        if (end > 0)
                            viewString = viewString.Substring(0, end).TrimEnd();
                        viewString = "Inspect(" + viewString + ")";

                        valueString = viewString;
                    }
                    else if (expressionString.Length > 1 && expressionString.StartsWith("'"))
                    {
                        expressionString = "";
                        valueString = "\"" + "// " + line.Substring(1).TrimStart() + "\"";
                    }

                    expressionString = expressionString.Replace("\"", "\\" + "\"");

                    sb.AppendLine(string.Format(indent + "logger.TryLog(\"{0}\", {1});", expressionString, valueString));
                }
                
            }

            //return GetSource(String.Join(Environment.NewLine, sb.ToString()), additionalUsings.ToArray());
			return GetSource(sb.ToString(), additionalUsings.ToArray());
        }

        public static string GetSource(string lines, params string[] additionalUsings)
        {
            string usingString = "";
            if (additionalUsings != null && additionalUsings.Any())
                usingString = string.Join(Environment.NewLine, additionalUsings);


            


            return SOURCE.Replace("%LINES%", lines).Replace("%USINGS%", usingString);
        }


    }

    
}

