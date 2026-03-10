using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using EFEM.GUIControls;

namespace EFEMGUI
{
    static class Program
    {
        static string MuxString = "HMI_SW_EFEM2015";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int reTryCounter = 10;

            retry:
            bool bIsOwnder = false;
            using (Mutex mux = new Mutex(false, MuxString, out bIsOwnder))
            {
                if (!bIsOwnder)
                {
                    Thread.Sleep(100);
                    if (reTryCounter > 0)
                    {
                        reTryCounter--;
                        goto retry;
                    }
                }
            }
            
            using (Mutex mux = new Mutex(false, MuxString, out bIsOwnder))
            {
                if (!bIsOwnder)
                {
                    MessageBox.Show("EFEM GUI is already running!\nOnly one instance is allow.", "EFEM GUI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                #region Exception handler
                //Help to restart EFEM GUI automatically while crashed
                EFEMForm.RegisterApplicationRecoveryAndRestart();

                Application.ThreadException += new ThreadExceptionEventHandler(EFEMForm.UI_UnhandledException);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(EFEMForm.CurrentDomain_UnhandledException);
                #endregion

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new EFEMForm()); //TestLogForm());

                mux.ReleaseMutex();
            }
        }
    }
}
