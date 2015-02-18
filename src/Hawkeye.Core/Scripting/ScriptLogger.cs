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
		public string ErrorNumber { get; set; }
		public int Line { get; set; }
	}

	public static class ScriptErrorExtender
	{
		private static Dictionary<string, string> _additions = null;

		static ScriptErrorExtender()
		{
			_additions = new Dictionary<string, string>();
			_additions.Add("CS0117", "Try encapsulating the term with \"!(...)\"");
		}

		public static void TryExtend(ScriptError error)
		{
			var addition = GetAddition(error.ErrorNumber);
			if (!string.IsNullOrEmpty(addition))
				error.Message += ". " + addition;
		}

		public static string GetAddition(string errorNumber)
		{
			if (_additions.ContainsKey(errorNumber))
				return _additions[errorNumber];
			return null;
		}
	}

	public static class ScriptLoggerSource
	{
		// TODO: Sort methods, properties and fields and re-add the argument information for inspected methods

		private const string SOURCE = @"
using System;
//using System.IO;
//using System.Linq;
using System.Text;
using System.Reflection;
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

				var useLogger = true;

				// skip "//......"
				if (expressionString.StartsWith("//"))
					continue;

				if (expressionString.Length > 1 && expressionString.StartsWith("!"))
				{
					// "!......"
					string codeString = expressionString.Substring(1);
					if (!codeString.TrimEnd().EndsWith(";", StringComparison.OrdinalIgnoreCase))
						codeString += ";";
					sb.AppendLine(indent + codeString);
				}
				else if (expressionString.Length > 1 && expressionString.StartsWith("#"))
				{
					// insert namespace "#System.Windows.Forms"
					string usingString = expressionString.Substring(1).Trim();
					if (!usingString.StartsWith("using", StringComparison.OrdinalIgnoreCase))
						usingString = "using " + usingString;
					if (!usingString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
						usingString += ";";
					additionalUsings.Add(usingString);
				}
				else
				{
					// "....::....."
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
						// "?..."
						valueString = expressionString.Substring(1).Trim();
						var end = valueString.LastIndexOf(';');
						if (end > 0)
							valueString = valueString.Substring(0, end).TrimEnd();
						expressionString = valueString;
					}
					else if (expressionString.Length > 1 && expressionString.StartsWith("*"))
					{
						// "*..."

						string viewString = expressionString.Substring(1).Trim();

						expressionString = "Inspect: " + viewString;

						if (!viewString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
							viewString += ";";

						var end = viewString.LastIndexOf(';');
						if (end > 0)
							viewString = viewString.Substring(0, end).TrimEnd();
						viewString = "RuntimeHelper.Inspect(" + viewString + ")";

						valueString = viewString;
					}
					else if (expressionString.Length > 1 && expressionString.StartsWith("'"))
					{
						// "'....." comment
						expressionString = "";
						valueString = "\"" + "// " + line.Substring(1).TrimStart() + "\"";
					}
					else
					{
						// "......"
						if (!expressionString.TrimEnd().EndsWith(";", StringComparison.OrdinalIgnoreCase))
							expressionString += ";";
						sb.AppendLine(indent + expressionString);
						useLogger = false;
					}

					if (useLogger)
					{
						expressionString = expressionString.Replace("\"", "\\" + "\"");
						sb.AppendLine(string.Format(indent + "logger.TryLog(\"{0}\", {1});", expressionString, valueString));
					}
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
