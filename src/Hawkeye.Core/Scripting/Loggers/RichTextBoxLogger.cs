using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;

namespace Hawkeye.Scripting.Loggers
{
    public class TextBoxScriptLogger : BaseScriptLogger
    {
        private readonly TextBoxBase _box;

        public TextBoxScriptLogger(TextBoxBase box)
        {
            _box = box;

            box?.Clear();
        }

        public override void Log(string expression, object value)
        {
            string log = null;

            if (value != null && value.GetType() != typeof(string))
            {
                if (value is IEnumerable enumerable)
                {
                    var sb = new StringBuilder();

                    foreach (object item in enumerable)
                    {
                        sb.AppendLine(item?.ToString() ?? "(null)");
                    }

                    value = sb.ToString();
                }
            }

            if (value == null)
            {
                value = "(null)";
            }

            if (value == DBNull.Value)
            {
                value = "(DBNull)";
            }

            if (!string.IsNullOrEmpty(expression) && value != null)
            {
                log = ">> " + expression + Environment.NewLine + value;
            }
            else
            {
                if (!string.IsNullOrEmpty(expression))
                {
                    log = ">> " + expression;
                }
                else
                {
                    log = value.ToString();
                }
            }

            _box.AppendText(log + Environment.NewLine + Environment.NewLine);
        }

        public override void ShowErrors(params string[] errors)
        {
            if (_box == null)
            {
                throw new NullReferenceException("_box is not set!");
            }

            _box.Clear();
            _box.Lines = errors;
        }
    }
}