using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Translator.Crypto;
using System.Text.RegularExpressions;

namespace UmaMusumeDBBrowser
{
    public class UmaMusumeLibrary
    {
        //public List<string> CardList { get; private set; }

        private const string pattern = "[-.?!)(,:？！+＋…･♪・『』～「」：（）/》|}]";

        public List<EventData> EventList { get; set; }

        private SortedDictionary<int, List<KeyValuePair<int, StringData>>> eventListByNameLen;
        private SortedDictionary<int, List<KeyValuePair<int, StringData>>> optionListByNameLen;


        public UmaMusumeLibrary()
        {
            EventList = new List<EventData>();
            eventListByNameLen = new SortedDictionary<int, List<KeyValuePair<int, StringData>>>();
            optionListByNameLen = new SortedDictionary<int, List<KeyValuePair<int, StringData>>>();
            //CardList = new List<CardData>();
            //eventHashMap = new Dictionary<ulong, int[]>();
        }

        public static string PrepareText(string text)
        {
            return Regex.Replace(text, pattern, "");
        }


        private void FillDictonary()
        {
            for(int i=0;i<EventList.Count;i++)
            {
                //event
                KeyValuePair<int, StringData> data = new KeyValuePair<int, StringData>(i, new StringData(EventList[i].EventNameToCheck));
                int key = EventList[i].EventNameToCheck.Length;
                if (eventListByNameLen.ContainsKey(key))
                    eventListByNameLen[key].Add(data);
                else
                    eventListByNameLen.Add(key, new List<KeyValuePair<int, StringData>>() { data });
                //options
                foreach (var option in EventList[i].EventOptionsList)
                {
                    KeyValuePair<int, StringData> op = new KeyValuePair<int, StringData>(i, new StringData(option.OptionToCheck));
                    key = option.OptionToCheck.Length;
                    if (optionListByNameLen.ContainsKey(key))
                        optionListByNameLen[key].Add(op);
                    else
                        optionListByNameLen.Add(key, new List<KeyValuePair<int, StringData>>() { op });
                }

            }
        }

        public void LoadLibrary(string path)
        {
            var res = JsonConvert.DeserializeObject<UmaMusumeLibrary> (File.ReadAllText(path));
            EventList = new List<EventData>(res.EventList);
            res.EventList.Clear();
            res = null;
            FillDictonary();
        }

        public void SaveLibrary(string savePath)
        {
            File.WriteAllText(savePath, JsonConvert.SerializeObject(this));
        }


        public List<EventData> FindEventByName(string eventName, bool useImpreciseComparison = false, float confidence = 0.7f)
        {
            string prepareText = Regex.Replace(eventName, pattern, "");
            if (!useImpreciseComparison)
                return EventList.FindAll(a => a.EventNameToCheck.Contains(prepareText));
            else
            {
                if (eventListByNameLen.ContainsKey(prepareText.Length))
                {
                    List<EventData> data = new List<EventData>();
                    Encoding encoding = Encoding.GetEncoding(932);
                    var lst = eventListByNameLen[prepareText.Length];
                    for (int i = 0; i < lst.Count; i++)
                    {
                        float c = prepareText.PercentageComparison(encoding, lst[i].Value.Bytes);
                        if (c >= confidence)
                            data.Add(EventList[lst[i].Key]);
                    }
                    return data;
                }
                else
                    return new List<EventData>();
            }
        }

        public List<EventData> FindEventByNameDiceAlg(string eventName, float confidence = 0.5f)
        {
            List<KeyValuePair<float, EventData>> datas = new List<KeyValuePair<float, EventData>>();
            string prepareText = Regex.Replace(eventName, pattern, "");
            int startLen = prepareText.Length;
            if (startLen - 2 < 0)
                startLen = 0;
            else
                startLen = startLen - 2;
            int endLen = prepareText.Length;
            if (endLen + 2 > eventListByNameLen.Last().Key)
                endLen = eventListByNameLen.Last().Key;
            else
                endLen = endLen + 2;

            for (int i = startLen; i <= endLen; i++)
            {
                if (!eventListByNameLen.ContainsKey(i))
                    continue;
                var lst = eventListByNameLen[i];
                foreach (var item in lst)
                {
                    float confid = (float)item.Value.Text.DiceCoefficient(prepareText);
                    if (confid > confidence)
                        datas.Add(new KeyValuePair<float, EventData>(confid, EventList[item.Key]));
                }
            }
            if (datas.Count > 0)
            {
                return datas.Select(a => a.Value).ToList();
            }
            else
                return new List<EventData>();
        }

        public List<EventData> FindEventByOptionsDiceAlg(string optionName, float confidence = 0.5f)
        {
            List<KeyValuePair<float, EventData>> datas = new List<KeyValuePair<float, EventData>>();
            string prepareText = Regex.Replace(optionName, pattern, "");
            int startLen = prepareText.Length;
            if (startLen - 3 < 0)
                startLen = 0;
            else
                startLen = startLen - 3;
            int endLen = prepareText.Length;
            if (endLen + 3 > optionListByNameLen.Last().Key)
                endLen = optionListByNameLen.Last().Key;
            else
                endLen = endLen + 3;

            for (int i = startLen; i <= endLen; i++)
            {
                if (!optionListByNameLen.ContainsKey(i))
                    continue;
                var lst = optionListByNameLen[i];
                foreach (var item in lst)
                {
                    float confid = (float)item.Value.Text.DiceCoefficient(prepareText);
                    if (confid > confidence)
                        datas.Add(new KeyValuePair<float, EventData>(confid, EventList[item.Key]));
                }
            }
            if (datas.Count > 0)
            {
                return datas.Select(a => a.Value).ToList();
            }
            else
                return new List<EventData>();
        }

        private EventData GetEventWithMaxConfidence(List<KeyValuePair<float, EventData>> datas)
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

        public List<EventData> FindEventByOption(string eventOption, bool useImpreciseComparison = false, float confidence=0.7f)
        {
            string prepareText = Regex.Replace(eventOption, pattern, "");
            List<EventData> data = new List<EventData>();
            if (!useImpreciseComparison)
            {
                foreach (var ev in EventList)
                {
                    if (ev.ContainsOption(prepareText))
                        data.Add(ev);
                }
            }
            else
            {
                if (optionListByNameLen.ContainsKey(prepareText.Length))
                {
                    Encoding encoding = Encoding.GetEncoding(932);
                    var lst = optionListByNameLen[prepareText.Length];
                    for (int i = 0; i < lst.Count; i++)
                    {
                        float c = prepareText.PercentageComparison(encoding, lst[i].Value.Bytes);
                        if (c >= confidence)
                            data.Add(EventList[lst[i].Key]);
                    }
                }
            }
            return data;
        }


        public class EventData
        {
            public string EventName { get; set; }
            public string EventNameToCheck { get; set; }
            public List<EventOption> EventOptionsList { get; set; }
            public CardType Type { get; set; }
            public string CardName { get; set; }

            public EventData()
            {
                EventOptionsList = new List<EventOption>();
            }

            public void SetEventName(string text)
            {
                EventName = text;
                EventNameToCheck = Regex.Replace(text, pattern, "");
            }

            public bool ContainsOption(string text, bool useImpreciseComparison = false, float confidence = 0.7f)
            {
                // string prepareText = Regex.Replace(text, pattern, "");
                if (!useImpreciseComparison)
                {
                    if (EventOptionsList.Exists(a => a.OptionToCheck.Contains(text)))
                        return true;
                    return false;
                }
                else
                {
                    if (EventOptionsList.Exists(a => a.OptionToCheck.PercentageComparison(text) > confidence))
                        return true;
                    return false;
                }
            }

            public bool ContainsOptionDice(string text, float confidence = 0.6f)
            {
                if (EventOptionsList.Exists(a => a.OptionToCheck.DiceCoefficient(text) > confidence))
                    return true;
                else
                    return false;
            }


            public override string ToString()
            {
                return EventName;
            }
        }

        public class EventOption
        {
            public string Option { get; set; }
            public string OptionToCheck { get; set; }
            public string Effect { get; set; }

            public EventOption() { }

            public void SetOption(string text)
            {
                Option = text;
                OptionToCheck = Regex.Replace(text, pattern, "");
            }

            public override string ToString()
            {
                return Option + "\n" + Effect;
            }
        }
        public enum CardType
        {
            Character,
            Support,
            MainStory
        }
    }

    public class StringData
    {
        private string _text;
        public byte[] Bytes { get; private set; }
        public string Text
        {
            get => _text;
            set
            {
                Bytes = Encoding.GetEncoding(932).GetBytes(value);
                _text = value;
            }
        }

        public StringData(string text)
        {
            Text = text;
        }


    }
    //public class UmaMusumeLibrary
    //{
    //    public List<CardData> CardList { get; private set; }

    //    //private Dictionary<ulong, int[]> eventHashMap;

    //    public UmaMusumeLibrary()
    //    {
    //        CardList = new List<CardData>();
    //        //eventHashMap = new Dictionary<ulong, int[]>();
    //    }

    //    public string[] GetCardNameList()
    //    {
    //        return CardList.Select(a => a.CardName).ToArray();
    //    }

    //    //private void GenerateEventHashMap()
    //    //{
    //    //    for (int i = 0; i < CardList.Count; i++)
    //    //    {
    //    //        for (int j = 0; j < CardList[i].EventDataList.Count; j++)
    //    //        {
    //    //            cCRC64 cCRC64 = new cCRC64();
    //    //            ulong hash = cCRC64.GenerateCRC64(CardList[i].EventDataList[j].EventName, Encoding.UTF8);
    //    //            eventHashMap.Add(hash, new int[] { i, j });
    //    //        }
    //    //    }
    //    //}

    //    public EventData FindEventByName(string CardName, string eventName)
    //    {
    //        CardData card = CardList.Find(a => a.CardName.Equals(CardName));
    //        if (card == null)
    //            return null;
    //        return card.EventDataList.Find(a => a.EventName.Contains(eventName));
    //    }

    //    public List<EventData> FindEventByName(string eventName)
    //    {
    //        List<EventData> data = new List<EventData>();
    //        foreach (var card in CardList)
    //        {
    //            var ev = card.EventDataList.Find(a => a.EventName.Contains(eventName));
    //            if (ev != null)
    //                data.Add(ev);
    //        }
    //        return data;
    //    }

    //    public List<EventData> FindEventByOption(string eventOption)
    //    {
    //        List<EventData> data = new List<EventData>();
    //        foreach (var card in CardList)
    //        {
    //            foreach (var ev in card.EventDataList)
    //            {
    //                if (ev.ContainsOption(eventOption))
    //                    data.Add(ev);
    //            }
    //            //var ev = card.EventDataList.Find(a => a.EventName.Contains(eventName));
    //            //if (ev != null)
    //            //    data.Add(ev);
    //        }
    //        return data;
    //    }

    //    //public EventData FindEventByName(string eventName)
    //    //{
    //    //    cCRC64 cCRC64 = new cCRC64();
    //    //    ulong hash = cCRC64.GenerateCRC64(eventName, Encoding.UTF8);
    //    //    if (eventHashMap.ContainsKey(hash))
    //    //    {
    //    //        var map = eventHashMap[hash];
    //    //        return CardList[map[0]].EventDataList[map[1]];
    //    //    }
    //    //    else
    //    //        return null;
    //    //}

    //    public void LoadFromFile(string fPath)
    //    {
    //        JObject data = JObject.Parse(File.ReadAllText(fPath));
    //        JToken characters = data["Charactor"];
    //        JToken supports = data["Support"];
    //        LoadCardData(characters, CardType.Character);
    //        LoadCardData(supports, CardType.Support);
    //        //GenerateEventHashMap();
    //    }

    //    private void LoadCardData(JToken token, CardType type)
    //    {
    //        foreach (JProperty rarity in token)
    //        {
    //            foreach (JProperty card in rarity.Value)
    //            {
    //                CardData data = new CardData();
    //                data.Type = type;
    //                data.CardName = card.Name;
    //                JArray eventArray = (JArray)card.Value["Event"];
    //                foreach (JProperty evData in eventArray.Values())
    //                {
    //                    EventData eventData = new EventData();
    //                    eventData.EventName = evData.Name;
    //                    foreach (JToken op in evData.Values())
    //                    {
    //                        EventOption eventOption = new EventOption();
    //                        eventOption.Option = op["Option"].ToString();
    //                        eventOption.Effect = op["Effect"].ToString();
    //                        eventData.EventOptionsList.Add(eventOption);
    //                    }
    //                    data.EventDataList.Add(eventData);
    //                }
    //                CardList.Add(data);
    //            }

    //        }
    //    }


    //    public class CardData
    //    {
    //        public string CardName { get; set; }
    //        public CardType Type { get; set; }
    //        public List<EventData> EventDataList { get; set; }

    //        public CardData()
    //        {
    //            EventDataList = new List<EventData>();
    //        }
    //        public override string ToString()
    //        {
    //            return CardName;
    //        }
    //    }

    //    public class EventData
    //    {
    //        public string EventName { get; set; }
    //        public List<EventOption> EventOptionsList { get; set; }

    //        public EventData()
    //        {
    //            EventOptionsList = new List<EventOption>();
    //        }

    //        public bool ContainsOption(string text)
    //        {
    //            if (EventOptionsList.Exists(a => a.Option.StartsWith(text)))
    //                return true;
    //            return false;
    //        }


    //        public override string ToString()
    //        {
    //            return EventName;
    //        }
    //    }

    //    public class EventOption
    //    {
    //        public string Option { get; set; }
    //        public string Effect { get; set; }

    //        public override string ToString()
    //        {
    //            return Option + "\n" + Effect;
    //        }
    //    }
    //    public enum CardType
    //    {
    //        Character,
    //        Support
    //    }
    //}
}
