using Hawkeye.Scripting;
using Hawkeye.Scripting.Errors;
using Hawkeye.Scripting.Interfaces;
using Hawkeye.Scripting.Loggers;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Hawkeye.UI.Controls
{
	public class ScriptBox : UserControl
	{
		private readonly object _currentThreadLock = new object();
		private RichTextBox _txtScript;
		private RichTextBox _txtLog;
		private SplitContainer _splitContainer1;
		private int _currentThreadId = -1;
		private SplitContainer _splitContainer2;
		private RichTextBox _txtCode;
		private IControlInfo _controlInfo = null;

		public ScriptBox() : base()
		{
			InitializeComponent();
		}

		private void txtScript_TextChanged(object sender, EventArgs e)
		{
			GenerateDelayed(GenerationMode.All);
		}

		private void GenerateDelayed(GenerationMode mode)
		{
			Action generateSync = () => { GenerateSync(mode); };

			var t = new Thread(() => generateSync());
			lock (_currentThreadLock)
				_currentThreadId = t.ManagedThreadId;
			t.Start();
		}

	    private void GenerateSync(GenerationMode mode)
	    {
	        Thread.Sleep(1000);

	        var canRun = false;

	        lock (_currentThreadLock)
	            canRun = _currentThreadId == Thread.CurrentThread.ManagedThreadId;

	        if (!canRun) return;

	        Action x = () => Generate(mode);
	        Invoke(x);
	    }

	    private void Generate(GenerationMode mode)
		{
			var logger = CreateLogger();

			var lines = _txtScript.Lines;

			if (mode == GenerationMode.Selection)
				lines = _txtScript.SelectedText.Split(new[] { "\n" }, StringSplitOptions.None);

			SourceInfo info = ScriptGenerator.GetSource(lines);

			_txtCode.Text = info.SourceCode;

			var res = CSharpScriptCompiler.Compile(info);

			if (res.Errors.HasErrors)
			{
			    LogErrors(res, logger);
			}
			else
			{
				try
				{
					TestCode(res.CompiledAssembly, logger);
				}
				catch (Exception ex)
				{
					logger.ShowErrors(ex);
				}
			}
		}

	    private static void LogErrors(CompilerResults res, IScriptLogger logger)
	    {
	        var errors = new List<ScriptError>();

	        foreach (var item in res.Errors)
	        {
	            var compilerError = item as CompilerError;

	            if (compilerError != null)
	            {
	                var error = new ScriptError()
	                {
	                    ErrorNumber = compilerError.ErrorNumber,
	                    Line = compilerError.Line,
	                    Message = compilerError.ErrorText
	                };
	                ScriptErrorExtender.TryExtend(error);
	                errors.Add(error);
	            }
	        }

	        logger.ShowErrors(errors.ToArray());
	    }


	    private IScriptLogger CreateLogger()
		{
			return new TextBoxScriptLogger(_txtLog);
		}

	    private void TestCode(Assembly assembly, IScriptLogger logger)
		{
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
					 if (face == loggerHostType)
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
				string controlInjectorLine =
				    $"System.Windows.Forms.Control _target = System.Windows.Forms.Control.FromHandle((IntPtr){_controlInfo.Control.Handle});";

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

			_txtScript.Lines = lines;
			_txtScript.SelectionStart = _txtScript.Text.Length;
			_txtScript.SelectionLength = 0;
			Generate(GenerationMode.All);
		}

        public IControlInfo ControlInfo
		{
			get { return _controlInfo; }
			set { updateControlInfo(value); }
		}

		private void InitializeComponent()
		{
			_txtScript = new RichTextBox();
			_txtLog = new RichTextBox();
			_splitContainer1 = new SplitContainer();
			_txtCode = new RichTextBox();
			_splitContainer2 = new SplitContainer();
			_splitContainer1.Panel1.SuspendLayout();
			_splitContainer1.Panel2.SuspendLayout();
			_splitContainer1.SuspendLayout();
			_splitContainer2.Panel1.SuspendLayout();
			_splitContainer2.Panel2.SuspendLayout();
			_splitContainer2.SuspendLayout();
			SuspendLayout();
			//
			// txtScript
			//
			_txtScript.Dock = DockStyle.Fill;
			_txtScript.Font = new System.Drawing.Font("Consolas", 8.25F);
			_txtScript.Location = new System.Drawing.Point(0, 0);
			_txtScript.Name = "_txtScript";
			_txtScript.Size = new System.Drawing.Size(589, 165);
			_txtScript.TabIndex = 0;
			_txtScript.Text = "";
			_txtScript.TextChanged += new EventHandler(txtScript_TextChanged);
			//
			// txtLog
			//
			_txtLog.Dock = DockStyle.Fill;
			_txtLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			_txtLog.Location = new System.Drawing.Point(0, 0);
			_txtLog.Name = "_txtLog";
			_txtLog.ReadOnly = true;
			_txtLog.Size = new System.Drawing.Size(585, 180);
			_txtLog.TabIndex = 1;
			_txtLog.Text = "";
			//
			// splitContainer1
			//
			_splitContainer1.Dock = DockStyle.Fill;
			_splitContainer1.Location = new System.Drawing.Point(0, 0);
			_splitContainer1.Name = "_splitContainer1";
			_splitContainer1.Orientation = Orientation.Horizontal;
			//
			// splitContainer1.Panel1
			//
			_splitContainer1.Panel1.Controls.Add(_txtScript);
			//
			// splitContainer1.Panel2
			//
			_splitContainer1.Panel2.Controls.Add(_splitContainer2);
			_splitContainer1.Size = new System.Drawing.Size(589, 349);
			_splitContainer1.SplitterDistance = 165;
			_splitContainer1.TabIndex = 2;
			//
			// txtCode
			//
			_txtCode.Dock = DockStyle.Fill;
			_txtCode.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			_txtCode.Location = new System.Drawing.Point(0, 0);
			_txtCode.Name = "_txtCode";
			_txtCode.ReadOnly = true;
			_txtCode.Size = new System.Drawing.Size(0, 180);
			_txtCode.TabIndex = 2;
			_txtCode.Text = "";
			//
			// splitContainer2
			//
			_splitContainer2.Dock = DockStyle.Fill;
			_splitContainer2.FixedPanel = FixedPanel.Panel2;
			_splitContainer2.Location = new System.Drawing.Point(0, 0);
			_splitContainer2.Name = "_splitContainer2";
			//
			// splitContainer2.Panel1
			//
			_splitContainer2.Panel1.Controls.Add(_txtLog);
			//
			// splitContainer2.Panel2
			//
			_splitContainer2.Panel2.Controls.Add(_txtCode);
			_splitContainer2.Panel2MinSize = 0;
			_splitContainer2.Size = new System.Drawing.Size(589, 180);
			_splitContainer2.SplitterDistance = 585;
			_splitContainer2.TabIndex = 3;
			//
			// ScriptBox
			//
			Controls.Add(_splitContainer1);
			Name = "ScriptBox";
			Size = new System.Drawing.Size(589, 349);
			_splitContainer1.Panel1.ResumeLayout(false);
			_splitContainer1.Panel2.ResumeLayout(false);
			_splitContainer1.ResumeLayout(false);
			_splitContainer2.Panel1.ResumeLayout(false);
			_splitContainer2.Panel2.ResumeLayout(false);
			_splitContainer2.ResumeLayout(false);
			ResumeLayout(false);

		}
	}

	public enum GenerationMode
	{
		All = 0,
		Selection
	}
}
