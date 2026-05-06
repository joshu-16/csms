using System;
using System.Windows.Forms;

namespace csms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start with welcome page instead of login directly
            Application.Run(new Form1());
        }
    }
}