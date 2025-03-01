using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data;

namespace UmaMusumeDBBrowser
{
    public class LegendBuffManager
    {
        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/]";

        public List<BuffData> BuffDataList { get; set; }
        private SortedDictionary<int, List<KeyValuePair<int, StringData>>> buffListByName;

        public LegendBuffManager()
        {
            BuffDataList = new List<BuffData>();
            buffListByName = new SortedDictionary<int, List<KeyValuePair<int, StringData>>>();
        }

        public void FillData()
        {
            SQLiteTableReader reader = new SQLiteTableReader(null, Program.DbPath);
            reader.Connect();
            DataTable skillTable = reader.ExecuteQuery("SELECT tm.id, tm.buff_id, tm.buff_rank, tm.legend_id || \"_\" || tm.buff_rank as ico_rank, tm.legend_id || \"_\" || tm.icon as ico_buff, t0.text as BuffName, t1.text as BuffDesc FROM (SELECT buff_id, id, buff_id, legend_id, buff_rank, icon FROM single_mode_10_buff) as tm LEFT JOIN (SELECT \"index\", text FROM text_data WHERE category=363) as t0 ON tm.buff_id = t0.\"index\" LEFT JOIN (SELECT \"index\", text FROM text_data WHERE category=364) as t1 ON tm.buff_id = t1.\"index\"");
            reader.Disconnect();
            for (int i = 0; i < skillTable.Rows.Count; i++)
            {
                BuffData data = new BuffData();
                data.Id = (long)skillTable.Rows[i]["id"];
                data.BuffId = (long)skillTable.Rows[i]["buff_id"];
                data.BuffRank = (long)skillTable.Rows[i]["buff_rank"];
                data.IconRank = (string)skillTable.Rows[i]["ico_rank"];
                data.IconBuff = (string)skillTable.Rows[i]["ico_buff"];
                data.Desc = (string)skillTable.Rows[i]["BuffDesc"];
                data.Desc = data.Desc.Replace("\\n", "");
                data.SetName((string)skillTable.Rows[i]["BuffName"]);
                BuffDataList.Add(data);
            }

            for (int i = 0; i < BuffDataList.Count; i++)
            {
                KeyValuePair<int, StringData> data = new KeyValuePair<int, StringData>(i, new StringData(BuffDataList[i].NameToCheck));
                int key = BuffDataList[i].NameToCheck.Length;
                if (buffListByName.ContainsKey(key))
                    buffListByName[key].Add(data);
                else
                    buffListByName.Add(key, new List<KeyValuePair<int, StringData>>() { data });
            }

        }

        public List<KeyValuePair<float, BuffData>> FindBuffByName(string buffName, bool useImpreciseComparison = false, float confidence = 0.7f)
        {
            if (string.IsNullOrWhiteSpace(buffName))
                return new List<KeyValuePair<float, BuffData>>();
            List<KeyValuePair<float, BuffData>> data = new List<KeyValuePair<float, BuffData>>();
            string prepareText = Regex.Replace(buffName, pattern, "");
            if (!useImpreciseComparison)
            {
                var res = BuffDataList.FindAll(a => a.NameToCheck.Contains(prepareText));
                foreach (var item in res)
                {
                    data.Add(new KeyValuePair<float, BuffData>(1, item));
                }
                return data;
            }
            else
            {
                if (buffListByName.ContainsKey(prepareText.Length))
                {
                    //List<SkillData> data = new List<SkillData>();
                    Encoding encoding = Encoding.GetEncoding(932);
                    var lst = buffListByName[prepareText.Length];
                    for (int i = 0; i < lst.Count; i++)
                    {
                        float c = prepareText.PercentageComparison(encoding, lst[i].Value.Bytes);
                        if (c >= confidence)
                            data.Add(new KeyValuePair<float, BuffData>(c, BuffDataList[lst[i].Key]));
                    }
                    return data;
                }
                else
                    return new List<KeyValuePair<float, BuffData>>();
            }
        }

        public List<KeyValuePair<float, BuffData>> FindBuffByNameDiceAlg(string buffName, float confidence = 0.5f)
        {
            List<KeyValuePair<float, BuffData>> datas = new List<KeyValuePair<float, BuffData>>();
            string prepareText = Regex.Replace(buffName, pattern, "");
            int startLen = prepareText.Length;
            if (startLen - 2 < 0)
                startLen = 0;
            else
                startLen = startLen - 2;
            int endLen = prepareText.Length;
            if (endLen + 2 > buffListByName.Last().Key)
                endLen = buffListByName.Last().Key;
            else
                endLen = endLen + 2;

            for (int i = startLen; i <= endLen; i++)
            {
                if (!buffListByName.ContainsKey(i))
                    continue;
                var lst = buffListByName[i];
                foreach (var item in lst)
                {
                    float confid = (float)item.Value.Text.DiceCoefficient(prepareText);
                    if (confid > confidence)
                        datas.Add(new KeyValuePair<float, BuffData>(confid, BuffDataList[item.Key]));
                }
            }
            return datas;
        }

        public class BuffData
        {
            public long Id { get; set; }
            public long BuffId { get; set; }
            public long BuffRank { get; set; }
            public string IconRank { get; set; }
            public string IconBuff { get; set; }
            public string Name { get; set; }
            public string NameToCheck { get; set; }
            public string Desc { get; set; }

            public BuffData()
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
