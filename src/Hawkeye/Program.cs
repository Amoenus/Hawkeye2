using System;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace HawkeyeApplication
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Force American English

            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            if (args.Length == 0)
                Hawkeye.HawkeyeApplication.Run();
            else
            {
                var windowHandle = (IntPtr)long.Parse(args[0]);
                var originalHandle = (IntPtr)long.Parse(args[1]);

                Hawkeye.HawkeyeApplication.Run(windowHandle, originalHandle);
            }
        }
    }
}
