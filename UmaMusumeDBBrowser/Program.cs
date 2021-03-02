using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Translator;

namespace UmaMusumeDBBrowser
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
            DictonariesDir = Path.Combine(Application.StartupPath, "Dictonaries");
            if (!Directory.Exists(DictonariesDir))
                Directory.CreateDirectory(DictonariesDir);
            TransDict = new TranslationDictonary();
            tools = new TextTool(Application.StartupPath);

            Application.Run(new Form1());
        }

        public static List<TableSettings> TableDisplaySettings;

        public static string DictonariesDir;

        public static TranslationDictonary TransDict;

        public static TextTool tools;
    }
}
