using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Hawkeye.Scripting
{
	public static class CSharpScriptCompiler
	{
		public static CompilerResults Compile(SourceInfo sourceInfo)
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

			foreach (var reference in sourceInfo.References)
				p.ReferencedAssemblies.Add(reference);

			p.GenerateExecutable = false;
			p.GenerateInMemory = true;
			return c.CompileAssemblyFromSource(p, sourceInfo.SourceCode);
		}
	}
}
