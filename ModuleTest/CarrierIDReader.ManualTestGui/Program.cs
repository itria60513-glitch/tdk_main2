using System;
using System.Windows.Forms;

namespace CarrierIDReader.ManualTestGui
{
    internal static class Program
    {
        /// <summary>
        /// Main entry point for the CarrierIDReader manual test GUI.
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
