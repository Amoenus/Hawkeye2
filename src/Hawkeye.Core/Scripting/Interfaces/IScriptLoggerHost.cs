using System;
using System.Collections.Generic;
using System.Text;

namespace Hawkeye.Scripting.Interfaces
{
	public interface IScriptLoggerHost
	{
		void Execute(IScriptLogger logger);
	}
}
