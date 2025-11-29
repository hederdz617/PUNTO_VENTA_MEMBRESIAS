using System;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Forms;

namespace NuevoAPPwindowsforms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            NuevoAPPwindowsforms.Services.DatabaseService.InitializeDatabase();
            NuevoAPPwindowsforms.Services.DatabaseService.DesactivarMembresiasExpiradas();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
