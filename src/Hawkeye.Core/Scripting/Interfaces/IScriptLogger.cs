using Hawkeye.Scripting.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hawkeye.Scripting.Interfaces
{
	public interface IScriptLogger
	{
		void InitLog();

		void TryLog(string expression, object value);

		void EndLog();

		void ShowErrors(params ScriptError[] errors);

		void ShowErrors(params Exception[] errors);
	}
}
