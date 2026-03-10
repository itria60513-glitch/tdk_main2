using System;
using System.Windows.Forms;

namespace AdvantechDIO.ManualTestGui
{
    internal static class Program
    {
        /// <summary>
        /// Main entry point for the simple manual test GUI.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
