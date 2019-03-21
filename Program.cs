using System;
using System.Threading;
using System.Windows.Forms;

namespace Bing_Wallpaper
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("You already have an instance of this application open", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                try
                {
                    Application.Run(new AboutForm());
                }
                catch (Exception ex)
                {
                    AppLogger.Logger.Error(ex, "Application.Run Exception");
                    Application.Exit();
                }
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppLogger.Logger.Error(e.ExceptionObject as Exception, "CurrentDomain_UnhandledException");
            Application.Exit();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            AppLogger.Logger.Error(e.Exception, "Application_ThreadException");
            Application.Exit();
        }

        private static readonly string appGuid = "A48877F7-25AC-4A0B-B040-885C47C225B5";
    }
}
