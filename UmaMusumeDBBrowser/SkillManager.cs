using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data;

namespace UmaMusumeDBBrowser
{
    public class SkillManager
    {
        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/]";

        public List<SkillData> SkillDataList { get; set; }
        private SortedDictionary<int, List<KeyValuePair<int, StringData>>> skillListByName;

        public SkillManager()
        {
            SkillDataList = new List<SkillData>();
            skillListByName = new SortedDictionary<int, List<KeyValuePair<int, StringData>>>();
        }

        public void FillData()
        {
            SQLiteTableReader reader = new SQLiteTableReader(null, Program.DbPath);
            reader.Connect();
            DataTable skillTable = reader.ExecuteQuery("SELECT tm.id, tm.icon_id,tm.rarity, t0.text as SkillName, t1.text as SkillDesc FROM (SELECT id, rarity, icon_id FROM skill_data) as tm, (SELECT \"index\", text FROM text_data WHERE category=47) as t0, (SELECT \"index\", text FROM text_data WHERE category=48) as t1 WHERE tm.id = t0.\"index\" AND tm.id = t1.\"index\"");
            reader.Disconnect();
            for (int i = 0; i < skillTable.Rows.Count; i++)
            {
                SkillData data = new SkillData();
                data.Id = (long)skillTable.Rows[i]["id"];
                data.IconId = (long)skillTable.Rows[i]["icon_id"];
                data.Rarity = (long)skillTable.Rows[i]["rarity"];
                data.Desc = (string)skillTable.Rows[i]["SkillDesc"];
                data.Desc = data.Desc.Replace("\\n", "");
                data.SetName((string)skillTable.Rows[i]["SkillName"]);
                SkillDataList.Add(data);
            }

            for (int i = 0; i < SkillDataList.Count; i++)
            {
                if (SkillDataList[i].Rarity > 2)
                    continue;
                KeyValuePair<int, StringData> data = new KeyValuePair<int, StringData>(i, new StringData(SkillDataList[i].NameToCheck));
                int key = SkillDataList[i].NameToCheck.Length;
                if (skillListByName.ContainsKey(key))
                    skillListByName[key].Add(data);
                else
                    skillListByName.Add(key, new List<KeyValuePair<int, StringData>>() { data });
            }

        }

        public List<KeyValuePair<float, SkillData>> FindSkillByName(string skillName, bool useImpreciseComparison = false, float confidence = 0.7f)
        {
            if (string.IsNullOrWhiteSpace(skillName))
                return new List<KeyValuePair<float, SkillData>>();
            List <KeyValuePair<float, SkillData>> data = new List<KeyValuePair<float, SkillData>>();
            string prepareText = Regex.Replace(skillName, pattern, "");
            prepareText = prepareText.Replace('〇', '○');
            if (!useImpreciseComparison)
            {
                var res = SkillDataList.FindAll(a => a.NameToCheck.Contains(prepareText));
                foreach (var item in res)
                {
                    data.Add(new KeyValuePair<float, SkillData>(1, item));
                }
                return data;
            }
            else
            {
                if (skillListByName.ContainsKey(prepareText.Length))
                {
                    //List<SkillData> data = new List<SkillData>();
                    Encoding encoding = Encoding.GetEncoding(932);
                    var lst = skillListByName[prepareText.Length];
                    for (int i = 0; i < lst.Count; i++)
                    {
                        float c = prepareText.PercentageComparison(encoding, lst[i].Value.Bytes);
                        if (c >= confidence)
                            data.Add(new KeyValuePair<float, SkillData>(c, SkillDataList[lst[i].Key]));
                    }
                    return data;
                }
                else
                    return new List<KeyValuePair<float, SkillData>>();
            }
        }

        public List<KeyValuePair<float, SkillData>> FindEventByNameDiceAlg(string skillName, float confidence = 0.5f)
        {
            List<KeyValuePair<float, SkillData>> datas = new List<KeyValuePair<float, SkillData>>();
            string prepareText = Regex.Replace(skillName, pattern, "");
            int startLen = prepareText.Length;
            if (startLen - 2 < 0)
                startLen = 0;
            else
                startLen = startLen - 2;
            int endLen = prepareText.Length;
            if (endLen + 2 > skillListByName.Last().Key)
                endLen = skillListByName.Last().Key;
            else
                endLen = endLen + 2;

            for (int i = startLen; i <= endLen; i++)
            {
                if (!skillListByName.ContainsKey(i))
                    continue;
                var lst = skillListByName[i];
                foreach (var item in lst)
                {
                    float confid = (float)item.Value.Text.DiceCoefficient(prepareText);
                    if (confid > confidence)
                        datas.Add(new KeyValuePair<float, SkillData>(confid, SkillDataList[item.Key]));
                }
            }
            return datas;
        }

        public class SkillData
        {
            public long Id { get; set; }
            public long IconId { get; set; }
            public string Name { get; set; }
            public string NameToCheck { get; private set; }
            public string Desc { get; set; }
            public long Rarity { get; set; }

            public SkillData()
            {

            }

            public void SetName(string text)
            {
                Name = text;
                NameToCheck = Regex.Replace(text, pattern, "");
            }

        }
    }
}
