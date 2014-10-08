using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hawkeye.Scripting
{
	public static class CSharpScriptCompiler
	{
		public static CompilerResults Compile(string code)
		{
			var c = new CSharpCodeProvider();
			var p = new CompilerParameters();

			//var rf = Assembly.GetAssembly(typeof(IScriptLogger));
			var rf = Assembly.GetExecutingAssembly();

			p.ReferencedAssemblies.Add("System.dll");
			//p.ReferencedAssemblies.Add("System.Core.dll");
			//p.ReferencedAssemblies.Add("System.IO.dll");
			p.ReferencedAssemblies.Add("System.Drawing.dll");
			p.ReferencedAssemblies.Add("System.Xml.dll");
			//p.ReferencedAssemblies.Add("System.Linq.dll");
			p.ReferencedAssemblies.Add("System.Windows.Forms.dll");
			p.ReferencedAssemblies.Add(rf.Location);

			p.GenerateExecutable = false;
			p.GenerateInMemory = true;
			return c.CompileAssemblyFromSource(p, code);
		}
	}
}
