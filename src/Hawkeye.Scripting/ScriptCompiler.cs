using Hawkeye.Scripting.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hawkeye.Scripting
{
	[ComVisible(true)]
	[ProgId("Hawkeye.Scripting")]
	[Guid("663DB9DE-E53A-4437-A230-7281C0973EF1")]
    public class ScriptCompiler : IScriptCompiler
    {
		public ScriptCompiler()
		{

		}

		public string Ping()
		{
			return DateTime.Now.ToString();
		}
    }
}
