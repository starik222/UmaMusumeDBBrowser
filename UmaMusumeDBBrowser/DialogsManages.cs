using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UmaMusumeDBBrowser
{
    public class DialogsManages
    {


        //private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/〇○]";
        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/》|}]";

        public List<DialogsItemData> DialogsDataList { get; set; }


        public Dictionary<int, string> NameDict { get; set; }

        public DialogsManages()
        {
            NameDict = new Dictionary<int, string>();
            DialogsDataList = new List<DialogsItemData>();
        }

        public void LoadLibrary(string path, string PlayerName)
        {
            DialogsDataList = JsonConvert.DeserializeObject<List<DialogsItemData>>(File.ReadAllText(path));
            NameDict.Clear();
            for (int i = 0; i < DialogsDataList.Count; i++)
            {
                if (DialogsDataList[i].Name == null)
                    continue;
                if (DialogsDataList[i].Name.Equals("<username>"))
                {
                    NameDict.Add(i, PlayerName);
                    DialogsDataList[i].Name = PlayerName;
                    DialogsDataList[i].NameTrans = PlayerName;
                }
                else
                {
                    NameDict.Add(i, DialogsDataList[i].Name);
                }

                
            }
        }



        public (string name, string nameTrans, DialogSubItem dialogData) FindDialogDiceAlg(string recName, string recText, float confidence = 0.5f)
        {
            int nameIndex = FindNameIndexByDice(recName);
            if (nameIndex == -1)
                return ("", "", new DialogSubItem());
            List<KeyValuePair<float, DialogSubItem>> datas = new List<KeyValuePair<float, DialogSubItem>>();

            var dialogSubItem = DialogsDataList[nameIndex];
            string prepareText = recText;//.Replace("7", "!");//Regex.Replace(recText, pattern, "");
            int startLen = prepareText.Length;
            if (startLen - 10 < 0)
                startLen = 0;
            else
                startLen = startLen - 10;
            int endLen = prepareText.Length;
            if (endLen + 10 > dialogSubItem.TextList.Last().Key)
                endLen = dialogSubItem.TextList.Last().Key;
            else
                endLen = endLen + 10;

            for (int i = startLen; i <= endLen; i++)
            {
                if (!dialogSubItem.TextList.ContainsKey(i))
                    continue;
                var lst = dialogSubItem.TextList[i];

                foreach (var item in lst)
                {
                    float confid = (float)item.Text.DiceCoefficient(prepareText);
                    if (confid > confidence)
                        datas.Add(new KeyValuePair<float, DialogSubItem>(confid, item));
                }
            }
            if (datas.Count > 0)
            {
                return (DialogsDataList[nameIndex].Name, DialogsDataList[nameIndex].NameTrans, GetDialogItemWithMaxConfidence(datas));
            }
            else
                return ("", "", new DialogSubItem());
        }


        public int FindNameIndexByDice(string text, float confidence = 0.5f)
        {
            List<KeyValuePair<float, int>> datas = new List<KeyValuePair<float, int>>();
            string preparedText = EventManager.PrepareText(text);

            foreach (var item in NameDict)
            {
                float c = (float)item.Value.DiceCoefficient(preparedText);
                if (c >= confidence)
                    datas.Add(new KeyValuePair<float, int>(c, item.Key));
            }

            if (datas.Count > 1)
                return GetNameIndexWithMaxConfidence(datas);
            else if (datas.Count == 1)
                return datas[0].Value;
            else
                return -1;

        }

        private int GetNameIndexWithMaxConfidence(List<KeyValuePair<float, int>> datas)
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
                return -1;
            return datas[index].Value;
        }


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

        private DialogSubItem GetDialogItemWithMaxConfidence(List<KeyValuePair<float, DialogSubItem>> datas)
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


        public class DialogsItemData
        {
            public string Name { get; set; }
            public string NameTrans { get; set; }

            public SortedDictionary<int, List<DialogSubItem>> TextList;

            public DialogsItemData()
            {
                TextList = new SortedDictionary<int, List<DialogSubItem>>();
            }
        }

        public class DialogSubItem
        {
            public string Text { get; set; }
            public string TransText { get; set; }
            public List<DialogSubItem> ChoiceDataList { get; set; }

            public DialogSubItem()
            {
                Text = "";
                TransText = "";
                ChoiceDataList = new List<DialogSubItem>();
            }
        }


        public class DialogsItemDataV1
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public string NameTrans { get; set; }
            public string TextTrans { get; set; }

            public List<DialogSubItem> ChoiceDataList { get; set; }

            public DialogsItemDataV1()
            {
                ChoiceDataList = new List<DialogSubItem>();
            }
        }
    }
}

