using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Translator;

namespace UmaResHelper
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SetData();
            Application.Run(new Form1());
        }

        private static void SetData()
        {
            DbPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Appdata\\LocalLow\\Cygames\\umamusume\\meta";
            ResPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Appdata\\LocalLow\\Cygames\\umamusume\\dat";
            tools = new TextTool(Application.StartupPath);
            if (!Directory.Exists(ResPath) || !File.Exists(DbPath))
            {
                MessageBox.Show("Каталог с ресурсами не найден!");
                Application.Exit();
            }
        }

        public static string DbPath;
        public static string ResPath;

        public static TextTool tools;
    }
}
