using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data;
using Translator;
using System.IO;

namespace UmaMusumeDBBrowser
{
    public class MissionManager
    {
        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/〇○]";

        public List<MissionData> MissionDataList { get; set; }

        private List<NAMES> transDict;

        public MissionManager()
        {
            MissionDataList = new List<MissionData>();
            transDict = new List<NAMES>();
        }

        public void FillData()
        {
            SQLiteTableReader reader = new SQLiteTableReader(null, Program.DbPath);
            reader.Connect();
            DataTable missionTable = reader.ExecuteQuery("SELECT tm.id as id, tm.item_id as item_id, tm.item_num as item_num, t0.text as MissionText FROM (SELECT id, item_id, item_num FROM mission_data) as tm, (SELECT \"index\", text FROM text_data WHERE category=67) as t0 WHERE tm.id = t0.\"index\"");
            reader.Disconnect();
            for (int i = 0; i < missionTable.Rows.Count; i++)
            {
                MissionData data = new MissionData();
                data.Id = (long)missionTable.Rows[i]["id"];
                data.ItemId = (long)missionTable.Rows[i]["item_id"];
                data.ItemCount = (long)missionTable.Rows[i]["item_num"];
                data.SetMissionText(((string)missionTable.Rows[i]["MissionText"]).Replace("\\n", ""));
                if (!MissionDataList.Exists(a => a.MissionText.Equals(data.MissionText)))
                {
                    string transText = Program.TransDict.GetTranslation(transDict, data.MissionText);
                    if (string.IsNullOrEmpty(transText))
                        transText = string.Empty;
                    data.TransMissionText = transText;
                    MissionDataList.Add(data);
                }
            }

        }

        public void FillTransDict(string path)
        {
            transDict = Program.TransDict.LoadDictonary(path);
        }


        public MissionData FindMissionItemByTextDice(string text, float confidence = 0.5f)
        {
            List<KeyValuePair<float, MissionData>> datas = new List<KeyValuePair<float, MissionData>>();
            string preparedText = EventManager.PrepareText(text);

            foreach (var item in MissionDataList)
            {
                float c = (float)item.MissionTextToCheck.DiceCoefficient(preparedText);
                if (c >= confidence)
                    datas.Add(new KeyValuePair<float, MissionData>(c, item));
            }

            if (datas.Count > 1)
                return GetMissionWithMaxConfidence(datas);
            else if (datas.Count == 1)
                return datas[0].Value;
            else
                return null;

        }

        private MissionData GetMissionWithMaxConfidence(List<KeyValuePair<float, MissionData>> datas)
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


        //public List<KeyValuePair<float, FactorData>> FindFactorByNameDiceAlg(string factorName, float confidence = 0.5f)
        //{
        //    List<KeyValuePair<float, FactorData>> datas = new List<KeyValuePair<float, FactorData>>();
        //    string prepareText = Regex.Replace(factorName, pattern, "");
        //    int startLen = prepareText.Length;
        //    if (startLen - 2 < 0)
        //        startLen = 0;
        //    else
        //        startLen = startLen - 2;
        //    int endLen = prepareText.Length;
        //    if (endLen + 2 > factorListByName.Last().Key)
        //        endLen = factorListByName.Last().Key;
        //    else
        //        endLen = endLen + 2;

        //    for (int i = startLen; i <= endLen; i++)
        //    {
        //        if (!factorListByName.ContainsKey(i))
        //            continue;
        //        var lst = factorListByName[i];
        //        foreach (var item in lst)
        //        {
        //            float confid = (float)item.Value.Text.DiceCoefficient(prepareText);
        //            if (confid > confidence)
        //                datas.Add(new KeyValuePair<float, FactorData>(confid, MissionDataList[item.Key]));
        //        }
        //    }
        //    return datas;
        //}

        public class MissionData
        {
            public long Id { get; set; }
            public long ItemId { get; set; }
            public long ItemCount { get; set; }
            public string MissionText { get; set; }
            public string MissionTextToCheck { get; set; }
            public string TransMissionText { get; set; }

            public MissionData()
            {

            }

            public void SetMissionText(string text)
            {
                MissionText = text;
                MissionTextToCheck = Regex.Replace(text, pattern, "");
            }

        }
    }
}
