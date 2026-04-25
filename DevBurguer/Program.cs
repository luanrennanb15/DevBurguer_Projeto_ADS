using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevBurguer
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Handlers globais de exceção
            Application.ThreadException += (s, e) =>
            {
                DevBurguer.Services.ExceptionLogger.Log(e.Exception, "Application.ThreadException");
                MessageBox.Show("Ocorreu um erro inesperado. Consulte os logs.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                DevBurguer.Services.ExceptionLogger.Log(ex ?? new Exception("UnhandledException sem Exception"), "AppDomain.UnhandledException");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                DevBurguer.Services.ExceptionLogger.Log(e.Exception, "TaskScheduler.UnobservedTaskException");
            };

            Application.Run(new FormLogin());
        }
    }
}
