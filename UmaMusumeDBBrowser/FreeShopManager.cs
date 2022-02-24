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
    public class FreeShopManager
    {


        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/〇○]";

        public List<FreeShopItemData> FreeShopDataList { get; set; }

        private List<NAMES> transDict;

        public FreeShopManager()
        {
            FreeShopDataList = new List<FreeShopItemData>();
            transDict = new List<NAMES>();
        }

        public void FillData()
        {
            SQLiteTableReader reader = new SQLiteTableReader(null, Program.DbPath);
            reader.Connect();
            DataTable shopItemTable = reader.ExecuteQuery("SELECT tm.item_id, tm.coin_num, tm.motion_id, t0.text as ItemName, t1.text as ItemDesc FROM (SELECT item_id, coin_num, motion_id FROM single_mode_free_shop_item) as tm, (SELECT \"index\", text FROM text_data WHERE category=225) as t0, (SELECT \"index\", text FROM text_data WHERE category=238) as t1 WHERE tm.item_id = t0.\"index\" AND tm.item_id = t1.\"index\"");
            reader.Disconnect();
            for (int i = 0; i < shopItemTable.Rows.Count; i++)
            {
                FreeShopItemData data = new FreeShopItemData();
                data.ItemId = (long)shopItemTable.Rows[i]["item_id"];
                data.ItemPrice = (long)shopItemTable.Rows[i]["coin_num"];
                data.SetItemName(((string)shopItemTable.Rows[i]["ItemName"]).Replace("\\n", ""));
                data.ItemDesc = ((string)shopItemTable.Rows[i]["ItemDesc"]).Replace("\\n", "");
                if (!FreeShopDataList.Exists(a => a.ItemName.Equals(data.ItemName)))
                {
                    string transName = Program.TransDict.GetTranslation(transDict, data.ItemName);
                    if (string.IsNullOrEmpty(transName))
                        transName = string.Empty;
                    data.ItemNameTrans = transName;
                    string transDesc = Program.TransDict.GetTranslation(transDict, data.ItemDesc);
                    if (string.IsNullOrEmpty(transDesc))
                        transDesc = string.Empty;
                    data.ItemDescTrans = transDesc;
                    FreeShopDataList.Add(data);
                }
            }

        }

        public void FillTransDict(string path)
        {
            transDict = Program.TransDict.LoadDictonary(path);
        }


        public FreeShopItemData FindFreeShopItemByTextDice(string text, float confidence = 0.5f)
        {
            List<KeyValuePair<float, FreeShopItemData>> datas = new List<KeyValuePair<float, FreeShopItemData>>();
            string preparedText = EventManager.PrepareText(text);

            foreach (var item in FreeShopDataList)
            {
                float c = (float)item.ItemNameToCheck.DiceCoefficient(preparedText);
                if (c >= confidence)
                    datas.Add(new KeyValuePair<float, FreeShopItemData>(c, item));
            }

            if (datas.Count > 1)
                return GetShopItemWithMaxConfidence(datas);
            else if (datas.Count == 1)
                return datas[0].Value;
            else
                return null;

        }

        private FreeShopItemData GetShopItemWithMaxConfidence(List<KeyValuePair<float, FreeShopItemData>> datas)
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





        public class FreeShopItemData
        {
            //public long Id { get; set; }
            public long ItemId { get; set; }
            public long ItemPrice { get; set; }
            public string ItemName { get; set; }
            public string ItemNameToCheck { get; set; }
            public string ItemNameTrans { get; set; }
            public string ItemDesc { get; set; }
            public string ItemDescTrans { get; set; }

            public FreeShopItemData()
            {

            }

            public void SetItemName(string text)
            {
                ItemName = text;
                ItemNameToCheck = Regex.Replace(text, pattern, "");
            }

        }
    }
}
