using Hawkeye.Scripting;
using Hawkeye.Scripting.Errors;
using Hawkeye.Scripting.Interfaces;
using Hawkeye.Scripting.Loggers;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Hawkeye.UI.Controls
{
	public class ScriptBox : UserControl
	{
		private object _currentThreadLock = new object();
		private RichTextBox txtScript;
		private RichTextBox txtLog;
		private SplitContainer splitContainer1;
		private int _currentThreadId = -1;
		private SplitContainer splitContainer2;
		private RichTextBox txtCode;
		private IControlInfo _controlInfo = null;

		public ScriptBox() : base()
		{
			InitializeComponent();
		}

		private void txtScript_TextChanged(object sender, EventArgs e)
		{
			generateDelayed(GenerationMode.All);
		}

		private void generateDelayed(GenerationMode mode)
		{
			Action generateSync = () =>
			{
				Thread.Sleep(1000);

				bool canRun = false;

				lock (_currentThreadLock)
					canRun = _currentThreadId == Thread.CurrentThread.ManagedThreadId;

				if (canRun)
				{
					Action x = () => generate(mode);
					this.Invoke(x);
				}
			};

			var t = new Thread(() => generateSync());
			lock (_currentThreadLock)
				_currentThreadId = t.ManagedThreadId;
			t.Start();
		}

		private void generate(GenerationMode mode)
		{
			var logger = createLogger();

			string[] lines = txtScript.Lines;

			if (mode == GenerationMode.Selection)
				lines = txtScript.SelectedText.Split(new string[] { "\n" }, StringSplitOptions.None);

			string code = ScriptGenerator.GetSource(lines);

			txtCode.Text = code;

			var res = CSharpScriptCompiler.Compile(code);
			
			if (res.Errors.HasErrors)
			{
				//var errors = res.Errors.OfType<CompilerError>().Select(ce => new ScriptError() { Line = ce.Line, Message = ce.ErrorText }).ToArray();

				// #lambda

				var errors = new List<ScriptError>();

				foreach (var item in res.Errors)
				{
					var compilerError = item as CompilerError;

					if (compilerError != null)
					{
						var error = new ScriptError() { ErrorNumber = compilerError.ErrorNumber, Line = compilerError.Line, Message = compilerError.ErrorText };
						ScriptErrorExtender.TryExtend(error);
						errors.Add(error);
					}
				}

				logger.ShowErrors(errors.ToArray());
			}
			else
				testCode(res.CompiledAssembly, logger);
		}


		private IScriptLogger createLogger()
		{
			return new TextBoxScriptLogger(txtLog);
		}

		private void showErrors(List<CompilerError> list)
		{
			// #lambda string message = string.Join(Environment.NewLine, list.Select(e => e.ErrorText));

			var sb = new StringBuilder();
			foreach (var item in list)
				sb.AppendLine(item.ErrorText);

			MessageBox.Show(this, sb.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void testCode(Assembly assembly, IScriptLogger logger)
		{
			//var scriptLoggerType = assembly.GetTypes().FirstOrDefault(t => t.GetInterfaces().Any(i => i.Equals(typeof(IScriptLoggerHost))));
			var scriptLoggerType = GetFirstLoggerHost(assembly.GetTypes());
			if (scriptLoggerType != null)
			{
				var scriptLogger = Activator.CreateInstance(scriptLoggerType) as IScriptLoggerHost;
				scriptLogger.Execute(logger);
			}
		}

		private Type GetFirstLoggerHost(Type[] assemblyTypes)
		{ 
			var loggerHostType = typeof(IScriptLoggerHost);

			foreach (var type in assemblyTypes)
			{
				 foreach (var face in type.GetInterfaces())
				 {
					 if (face.Equals(loggerHostType))
						 return type;
				 }
			}

			return null;
		}

		private void updateControlInfo(IControlInfo info)
		{
			_controlInfo = info;

			string[] lines = null;

			if (info != null && info.Control != null)
			{
				string controlInjectorLine = String.Format("System.Windows.Forms.Control _target = System.Windows.Forms.Control.FromHandle((IntPtr){0});", _controlInfo.Control.Handle);

				lines = new string[6];
				lines[0] = "// These lines were injected dynamically to script against the target object with the variable [_target].";
				lines[1] = controlInjectorLine;
				lines[2] = "?_target.GetType().FullName";
				lines[3] = "?_target.Name";
				lines[4] = "*_target";
				lines[5] = "";
			}
			else
			{
				lines = new string[2];
				lines[0] = "// No target control info found.";
				lines[1] = "";
			}

			txtScript.Lines = lines;
			txtScript.SelectionStart = txtScript.Text.Length;
			txtScript.SelectionLength = 0;
			generate(GenerationMode.All);
		}

		public IControlInfo ControlInfo
		{
			get { return _controlInfo; }
			set { updateControlInfo(value); }
		}

		private void InitializeComponent()
		{
			this.txtScript = new System.Windows.Forms.RichTextBox();
			this.txtLog = new System.Windows.Forms.RichTextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.txtCode = new System.Windows.Forms.RichTextBox();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtScript
			// 
			this.txtScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtScript.Font = new System.Drawing.Font("Consolas", 8.25F);
			this.txtScript.Location = new System.Drawing.Point(0, 0);
			this.txtScript.Name = "txtScript";
			this.txtScript.Size = new System.Drawing.Size(589, 165);
			this.txtScript.TabIndex = 0;
			this.txtScript.Text = "";
			this.txtScript.TextChanged += new System.EventHandler(this.txtScript_TextChanged);
			// 
			// txtLog
			// 
			this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLog.Location = new System.Drawing.Point(0, 0);
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.Size = new System.Drawing.Size(585, 180);
			this.txtLog.TabIndex = 1;
			this.txtLog.Text = "";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.txtScript);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(589, 349);
			this.splitContainer1.SplitterDistance = 165;
			this.splitContainer1.TabIndex = 2;
			// 
			// txtCode
			// 
			this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtCode.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtCode.Location = new System.Drawing.Point(0, 0);
			this.txtCode.Name = "txtCode";
			this.txtCode.ReadOnly = true;
			this.txtCode.Size = new System.Drawing.Size(0, 180);
			this.txtCode.TabIndex = 2;
			this.txtCode.Text = "";
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.txtLog);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.txtCode);
			this.splitContainer2.Panel2MinSize = 0;
			this.splitContainer2.Size = new System.Drawing.Size(589, 180);
			this.splitContainer2.SplitterDistance = 585;
			this.splitContainer2.TabIndex = 3;
			// 
			// ScriptBox
			// 
			this.Controls.Add(this.splitContainer1);
			this.Name = "ScriptBox";
			this.Size = new System.Drawing.Size(589, 349);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}

	public enum GenerationMode
	{
		All = 0,
		Selection
	}
}
