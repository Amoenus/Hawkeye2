using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hawkeye.Scripting
{
    public static class ScriptGenerator
    {
        private const string SOURCE = @"
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using Hawkeye.Scripting.Errors;
using Hawkeye.Scripting.Interfaces;
using Hawkeye.Scripting.Loggers;
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

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <returns></returns>
        public static SourceInfo GetSource(string[] lines)
        {
            var info = new SourceInfo();

            const string indent = "\t\t";

            var sb = new StringBuilder();

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line?.Trim()))
                {
                    continue;
                }

                string expressionString = line.TrimStart();
                string valueString = expressionString;

                var useLogger = true;

                // skip "//......"
                if (expressionString.StartsWith("//"))
                {
                    continue;
                }

                if (expressionString.Length > 1 && expressionString.StartsWith("!"))
                {
                    // "!......"
                    string codeString = expressionString.Substring(1);
                    //if (!codeString.TrimEnd().EndsWith(";", StringComparison.OrdinalIgnoreCase))
                    //	codeString += ";";
                    sb.AppendLine(indent + codeString);
                }
                else if (expressionString.Length > 1 && expressionString.StartsWith("#"))
                {
                    // insert namespace "#System.Windows.Forms"
                    string usingString = expressionString.Substring(1).Trim();
                    if (!usingString.StartsWith("using", StringComparison.OrdinalIgnoreCase))
                    {
                        usingString = "using " + usingString;
                    }

                    //if (!usingString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                    //	usingString += ";";
                    if (!info.Usings.Contains(usingString))
                    {
                        info.Usings.Add(usingString);
                    }
                }
                else if (expressionString.Length > 1 && expressionString.StartsWith("$"))
                {
                    // insert namespace "$System.Windows.Forms.dll"
                    string reference = expressionString.Substring(1).Trim();
                    if (!info.References.Contains(reference))
                    {
                        info.References.Add(reference);
                    }
                }
                else
                {
                    // "....::....."
                    if (line.Contains("::"))
                    {
                        string[] parts = line.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);

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
                        int end = valueString.LastIndexOf(';');
                        if (end > 0)
                        {
                            valueString = valueString.Substring(0, end).TrimEnd();
                        }

                        expressionString = valueString;
                    }
                    else if (expressionString.Length > 1 && expressionString.StartsWith("*"))
                    {
                        // "*..."

                        string viewString = expressionString.Substring(1).Trim();

                        expressionString = "Inspect: " + viewString;

                        //if (!viewString.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                        //	viewString += ";";

                        int end = viewString.LastIndexOf(';');
                        if (end > 0)
                        {
                            viewString = viewString.Substring(0, end).TrimEnd();
                        }

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
                        //if (!expressionString.TrimEnd().EndsWith(";", StringComparison.OrdinalIgnoreCase))
                        //	expressionString += ";";

                        sb.AppendLine(indent + expressionString);
                        useLogger = false;
                    }

                    if (useLogger)
                    {
                        expressionString = expressionString.Replace("\"", "\\" + "\"");

                        if (expressionString.Contains(".!"))
                        {
                            List<string> resolvedLines = RuntimeHelper.Resolve2(expressionString);
                            for (var i = 0; i < resolvedLines.Count - 1; i++)
                            {
                                sb.AppendLine(indent + resolvedLines[i]);
                            }

                            sb.AppendLine(indent + $"logger.TryLog(\"{expressionString}\", {resolvedLines.Last()});");
                        }
                        else
                        {
                            sb.AppendLine(indent + $"logger.TryLog(\"{expressionString}\", {valueString});");
                        }
                    }
                }
            }

            info.SourceCode = sb.ToString();

            ResolveSource(ref info);

            return info;
        }


        /// <summary>
        /// Resolves the source.
        /// </summary>
        /// <param name="sourceInfo">The source information.</param>
        public static void ResolveSource(ref SourceInfo sourceInfo)
        {
            var usingString = "";
            if (sourceInfo.Usings.Any())
            {
                usingString = string.Join(Environment.NewLine, sourceInfo.Usings.ToArray());
            }

            sourceInfo.SourceCode = SOURCE.Replace("%LINES%", sourceInfo.SourceCode).Replace("%USINGS%", usingString);
        }
    }
}