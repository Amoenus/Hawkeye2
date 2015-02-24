using System;
using System.Collections.Generic;
using System.Text;

namespace Hawkeye.Scripting.Errors
{
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
}
