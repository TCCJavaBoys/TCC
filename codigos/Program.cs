using System;
using System.Windows.Forms;
using PotirendabaApp.Data;

namespace PotirendabaApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Garante que o banco e TODAS as tabelas existam antes de abrir qualquer tela
            DatabaseHelper.InicializarBanco();

            Application.Run(new MainForm());
        }
    }
}
