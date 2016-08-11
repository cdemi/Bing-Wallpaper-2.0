using System;
using System.Threading;
using System.Windows.Forms;

namespace Bing_Wallpaper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
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
                Application.Run(new AboutForm());
            }
        }

        private static string appGuid = "A48877F7-25AC-4A0B-B040-885C47C225B5";
    }
}
