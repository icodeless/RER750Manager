using System;
using System.Windows.Forms;

namespace RER750Manager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); // Ensure this matches your form class name
        }
    }
}