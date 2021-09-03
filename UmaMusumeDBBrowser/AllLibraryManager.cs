using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UmaMusumeDBBrowser
{
    public class AllLibraryManager
    {
        public EventManager EventLibrary { get; set; }
        public SkillManager SkillLibrary { get; set; }
        public TazunaManager TazunaLibrary { get; set; }
        public FactorManager FactorLibrary { get; set; }
        public MissionManager MissionLibrary { get; set; }

        public AllLibraryManager()
        {
            EventLibrary = new EventManager();
            SkillLibrary = new SkillManager();
            TazunaLibrary = new TazunaManager();
            FactorLibrary = new FactorManager();
            MissionLibrary = new MissionManager();
        }

        public void FillData()
        {
            EventLibrary.LoadLibrary(Path.Combine(Program.DictonariesDir, "EventLibrary.json"));
            SkillLibrary.FillData();
            TazunaLibrary.LoadLibrary(Path.Combine(Program.DictonariesDir, "TazunaLibrary.json"));
            FactorLibrary.FillData();
            MissionLibrary.FillData();
        }


    }
}
