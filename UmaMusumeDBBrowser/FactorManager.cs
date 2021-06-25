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
    public class FactorManager
    {
        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/〇○]";

        public List<FactorData> FactorDataList { get; set; }
        private SortedDictionary<int, List<KeyValuePair<int, StringData>>> factorListByName;

        private List<NAMES> transDict;

        public FactorManager()
        {
            FactorDataList = new List<FactorData>();
            factorListByName = new SortedDictionary<int, List<KeyValuePair<int, StringData>>>();
            transDict = new List<NAMES>();
        }

        public void FillData()
        {
            SQLiteTableReader reader = new SQLiteTableReader(null, Program.DbPath);
            reader.Connect();
            DataTable factorTable = reader.ExecuteQuery("SELECT tm.factor_id as id, tm.factor_type, t0.text as FactorName, t1.text as FactorDesc FROM (SELECT factor_id, factor_type FROM succession_factor) as tm, (SELECT \"index\", text FROM text_data WHERE category=147) as t0, (SELECT \"index\", text FROM text_data WHERE category=172) as t1 WHERE tm.factor_id = t0.\"index\" AND tm.factor_id = t1.\"index\"");
            reader.Disconnect();
            for (int i = 0; i < factorTable.Rows.Count; i++)
            {
                FactorData data = new FactorData();
                data.Id = (long)factorTable.Rows[i]["id"];
                data.FactorType = (FactorType)factorTable.Rows[i]["factor_type"];
                data.Desc = (string)factorTable.Rows[i]["FactorDesc"];
                data.Desc = data.Desc.Replace("\\n", "");
                data.SetName((string)factorTable.Rows[i]["FactorName"]);
                if (!FactorDataList.Exists(a => a.Name.Equals(data.Name)))
                {
                    string transName = Program.TransDict.GetTranslation(transDict, data.Name);
                    if (string.IsNullOrEmpty(transName))
                        transName = string.Empty;
                    data.NameTrans = transName;
                    string transDesc = Program.TransDict.GetTranslation(transDict, data.Desc);
                    if (string.IsNullOrEmpty(transDesc))
                        transDesc = string.Empty;
                    data.DescTrans = transDesc;
                    FactorDataList.Add(data);
                }
            }

            for (int i = 0; i < FactorDataList.Count; i++)
            {
                KeyValuePair<int, StringData> data = new KeyValuePair<int, StringData>(i, new StringData(FactorDataList[i].NameToCheck));
                int key = FactorDataList[i].NameToCheck.Length;
                if (factorListByName.ContainsKey(key))
                    factorListByName[key].Add(data);
                else
                    factorListByName.Add(key, new List<KeyValuePair<int, StringData>>() { data });
            }

        }

        public void FillTransDict(string path)
        {
            transDict = Program.TransDict.LoadDictonary(path);
        }

        //public List<KeyValuePair<float, SkillData>> FindSkillByName(string skillName, bool useImpreciseComparison = false, float confidence = 0.7f)
        //{
        //    if (string.IsNullOrWhiteSpace(skillName))
        //        return new List<KeyValuePair<float, SkillData>>();
        //    List<KeyValuePair<float, SkillData>> data = new List<KeyValuePair<float, SkillData>>();
        //    string prepareText = Regex.Replace(skillName, pattern, "");
        //    prepareText = prepareText.Replace('〇', '○');
        //    if (!useImpreciseComparison)
        //    {
        //        var res = SkillDataList.FindAll(a => a.NameToCheck.Contains(prepareText));
        //        foreach (var item in res)
        //        {
        //            data.Add(new KeyValuePair<float, SkillData>(1, item));
        //        }
        //        return data;
        //    }
        //    else
        //    {
        //        if (skillListByName.ContainsKey(prepareText.Length))
        //        {
        //            //List<SkillData> data = new List<SkillData>();
        //            Encoding encoding = Encoding.GetEncoding(932);
        //            var lst = skillListByName[prepareText.Length];
        //            for (int i = 0; i < lst.Count; i++)
        //            {
        //                float c = prepareText.PercentageComparison(encoding, lst[i].Value.Bytes);
        //                if (c >= confidence)
        //                    data.Add(new KeyValuePair<float, SkillData>(c, SkillDataList[lst[i].Key]));
        //            }
        //            return data;
        //        }
        //        else
        //            return new List<KeyValuePair<float, SkillData>>();
        //    }
        //}

        public List<KeyValuePair<float, FactorData>> FindFactorByNameDiceAlg(string factorName, float confidence = 0.5f)
        {
            List<KeyValuePair<float, FactorData>> datas = new List<KeyValuePair<float, FactorData>>();
            string prepareText = Regex.Replace(factorName, pattern, "");
            int startLen = prepareText.Length;
            if (startLen - 2 < 0)
                startLen = 0;
            else
                startLen = startLen - 2;
            int endLen = prepareText.Length;
            if (endLen + 2 > factorListByName.Last().Key)
                endLen = factorListByName.Last().Key;
            else
                endLen = endLen + 2;

            for (int i = startLen; i <= endLen; i++)
            {
                if (!factorListByName.ContainsKey(i))
                    continue;
                var lst = factorListByName[i];
                foreach (var item in lst)
                {
                    float confid = (float)item.Value.Text.DiceCoefficient(prepareText);
                    if (confid > confidence)
                        datas.Add(new KeyValuePair<float, FactorData>(confid, FactorDataList[item.Key]));
                }
            }
            return datas;
        }

        public class FactorData
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string NameTrans { get; set; }
            public string NameToCheck { get; private set; }
            public string Desc { get; set; }
            public string DescTrans { get; set; }
            public FactorType FactorType { get; set; }

            public FactorData()
            {

            }

            public void SetName(string text)
            {
                Name = text;
                NameToCheck = Regex.Replace(text, pattern, "");
            }

        }


        public enum FactorType : long
        {
            Characteristics = 1,
            Suitability = 2,
            ParentSkill = 3,
            Skill = 4,
            Race = 5,
            URA = 6
        }
    }
}
