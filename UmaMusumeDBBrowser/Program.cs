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
           //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
           //AppDomain.CurrentDomain.UnhandledException += DumpMaker.CurrentDomain_UnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DictonariesDir = Path.Combine(Application.StartupPath, "Dictonaries");
            if (!Directory.Exists(DictonariesDir))
                Directory.CreateDirectory(DictonariesDir);
            TransDict = new TranslationDictonary();
            tools = new TextTool(Application.StartupPath);
            TessManager = new TesseractManager();
            IconDB = new IconManager(Application.StartupPath);
            ColorManager = new ColorSchemeManager();
            ColorManager.Load(Path.Combine(Application.StartupPath, "ColorScheme.json"));
            string testDbPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Appdata\\LocalLow\\Cygames\\umamusume\\master\\master.mdb";
            if (File.Exists(testDbPath))
                DbPath = testDbPath;
            else
                DbPath = "master.db";
            if (IsDebug)
            {
                fLog = new Form_log();
                fLog.Show();
            }
            Application.Run(new Form1());
        }

        public static List<TableSettings> TableDisplaySettings;

        public static string DictonariesDir;

        public static TranslationDictonary TransDict;

        public static TextTool tools;

        public static TesseractManager TessManager;

        public static IconManager IconDB;

        public static string DbPath;

        public static ColorSchemeManager ColorManager;



        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
            return false;
#endif
            }
        }

        private static Form_log fLog;

        public static void AddToLog(string text)
        {
            fLog.AddToLog(text);
        }

        public static void SaveLog()
        {
            fLog.SaveLog("ProgramLog.txt");
        }

        public static void ClearLog()
        {
            fLog.ClearLog();
        }
    }
}
