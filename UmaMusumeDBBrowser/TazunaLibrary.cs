using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaMusumeDBBrowser
{
    public class TazunaManager
    {
        public List<HelpItem> TazunaHelpData { get; set; }

        public TazunaManager()
        {
            TazunaHelpData = new List<HelpItem>();
        }


        public HelpItem FindHelpItemByDescDice(string text, HelpType type, float confidence = 0.5f)
        {
            List<KeyValuePair<float, HelpItem>> datas = new List<KeyValuePair<float, HelpItem>>();
            string preparedText = EventManager.PrepareText(text);

            foreach (var item in TazunaHelpData)
            {
                if (item.Type != type)
                    continue;
                float c = (float)item.OriginalToCheck.DiceCoefficient(preparedText);
                if (c >= confidence)
                    datas.Add(new KeyValuePair<float, HelpItem>(c, item));
            }

            if (datas.Count > 1)
                return GetHelpTypeWithMaxConfidence(datas);
            else if (datas.Count == 1)
                return datas[0].Value;
            else
                return null;

        }

        private HelpItem GetHelpTypeWithMaxConfidence(List<KeyValuePair<float, HelpItem>> datas)
        {
            float c = -1;
            int index = -1;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i].Key > c)
                {
                    index = i;
                    c = datas[i].Key;
                }
            }
            if (index == -1)
                return null;
            return datas[index].Value;
        }

        public void LoadLibrary(string path)
        {
            var res = JsonConvert.DeserializeObject<TazunaManager>(File.ReadAllText(path));
            TazunaHelpData = new List<HelpItem>(res.TazunaHelpData);
            res.TazunaHelpData.Clear();
            res = null;

            foreach (var item in TazunaHelpData)
            {
                item.OriginalToCheck = EventManager.PrepareText(item.Original);
            }
        }

        public void SaveLibrary(string savePath)
        {
            File.WriteAllText(savePath, JsonConvert.SerializeObject(this));
        }

        public class HelpItem
        {
            public string Original { get; set; }
            public string OriginalToCheck { get; set; }
            public string Translations { get; set; }
            public HelpType Type { get; set; }

            public HelpItem() { }

        }

        public enum HelpType
        {
            AfterRaceDesc = 0,
            AfterRaceWarning = 1
        }
    }
}
