using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Translator;

namespace UmaMusumeDBBrowser
{
    public class LiveDataManager
    {
        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/〇○]";

        public List<TrainingBonusData> TrainingDataList { get; set; }

        private List<NAMES> trainingTransDict;
        private List<NAMES> LiveTransDict;

        public LiveDataManager()
        {
            TrainingDataList = new List<TrainingBonusData>();
            trainingTransDict = new List<NAMES>();
            LiveTransDict = new List<NAMES>();
        }

        //public void FillData()
        //{
        //    SQLiteTableReader reader = new SQLiteTableReader(null, Program.DbPath);
        //    reader.Connect();
        //    DataTable trainingDataTable = reader.ExecuteQuery("SELECT tm.id, t0.text as MasterName, t1.text as MasterText FROM (SELECT id FROM single_mode_live_master_bonus) as tm, (SELECT \"index\", text FROM text_data WHERE category=209) as t0, (SELECT \"index\", text FROM text_data WHERE category=207) as t1 WHERE tm.id = t0.\"index\" AND tm.id = t1.\"index\"");
        //    DataTable LiveDataTable = reader.ExecuteQuery("SELECT tm.id, tm.live_id, t0.text as SongName, t1.text as TrBobus, t2.text as LiveBonus  FROM (SELECT id,live_id,master_bonus_content_text_id FROM single_mode_live_song_list) as tm, (SELECT \"index\", text FROM text_data WHERE category=209) as t0, (SELECT \"index\", text FROM text_data WHERE category=207) as t1, (SELECT \"index\", text FROM text_data WHERE category=208) as t2 WHERE tm.id = t2.\"index\" AND tm.master_bonus_content_text_id = t0.\"index\" AND tm.master_bonus_content_text_id = t1.\"index\"");
        //    reader.Disconnect();
        //    for (int i = 0; i < shopItemTable.Rows.Count; i++)
        //    {
        //        FreeShopItemData data = new FreeShopItemData();
        //        data.ItemId = (long)shopItemTable.Rows[i]["item_id"];
        //        data.ItemPrice = (long)shopItemTable.Rows[i]["coin_num"];
        //        data.SetItemName(((string)shopItemTable.Rows[i]["ItemName"]).Replace("\\n", ""));
        //        data.ItemDesc = ((string)shopItemTable.Rows[i]["ItemDesc"]).Replace("\\n", "");
        //        if (!FreeShopDataList.Exists(a => a.ItemName.Equals(data.ItemName)))
        //        {
        //            string transName = Program.TransDict.GetTranslation(transDict, data.ItemName);
        //            if (string.IsNullOrEmpty(transName))
        //                transName = string.Empty;
        //            data.ItemNameTrans = transName;
        //            string transDesc = Program.TransDict.GetTranslation(transDict, data.ItemDesc);
        //            if (string.IsNullOrEmpty(transDesc))
        //                transDesc = string.Empty;
        //            data.ItemDescTrans = transDesc;
        //            FreeShopDataList.Add(data);
        //        }
        //    }

        //}

        //public void FillTransDict(string path)
        //{
        //    transDict = Program.TransDict.LoadDictonary(path);
        //}


        //public FreeShopItemData FindFreeShopItemByTextDice(string text, float confidence = 0.5f)
        //{
        //    List<KeyValuePair<float, FreeShopItemData>> datas = new List<KeyValuePair<float, FreeShopItemData>>();
        //    string preparedText = EventManager.PrepareText(text);

        //    foreach (var item in FreeShopDataList)
        //    {
        //        float c = (float)item.ItemNameToCheck.DiceCoefficient(preparedText);
        //        if (c >= confidence)
        //            datas.Add(new KeyValuePair<float, FreeShopItemData>(c, item));
        //    }

        //    if (datas.Count > 1)
        //        return GetShopItemWithMaxConfidence(datas);
        //    else if (datas.Count == 1)
        //        return datas[0].Value;
        //    else
        //        return null;

        //}

        //private FreeShopItemData GetShopItemWithMaxConfidence(List<KeyValuePair<float, FreeShopItemData>> datas)
        //{
        //    float c = -1;
        //    int index = -1;
        //    for (int i = 0; i < datas.Count; i++)
        //    {
        //        if (datas[i].Key > c)
        //        {
        //            index = i;
        //            c = datas[i].Key;
        //        }
        //    }
        //    if (index == -1)
        //        return null;
        //    return datas[index].Value;
        //}





        public class LiveBonusData
        {
            //public long Id { get; set; }
            public long Id { get; set; }
            public string NameToCheck { get; set; }
            public string Name { get; set; }
            public string NameTrans { get; set; }

            public LiveBonusData()
            {

            }

            public void SetName(string text)
            {
                Name = text;
                NameToCheck = Regex.Replace(text, pattern, "");
            }

        }

        public class TrainingBonusData
        {
            //public long Id { get; set; }
            public long Id { get; set; }
            public string NameToCheck { get; set; }
            public string Name { get; set; }
            public string NameTrans { get; set; }
            public string Text { get; set; }
            public string TextTrans { get; set; }

            public TrainingBonusData()
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
