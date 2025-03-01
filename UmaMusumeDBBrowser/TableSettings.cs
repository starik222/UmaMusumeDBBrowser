using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaMusumeDBBrowser
{
    public class TableSettings
    {
        public string TableName { get; set; }
        public string TextIndexColumn { get; set; }
        public List<string> DisplayColumms { get; set; }
        public List<string> HideColumms { get; set; }
        public List<KeyValuePair<string, List<string>>> IconSettings { get; set; }
        //public string IconPath { get; set; }
        public List<KeyValuePair<int, string>> TextTypeAndName { get; set; }
        public string CustomQueryMainTable { get; set; }
        public int RowHeight { get; set; }
        public List<KeyValuePair<string, int>> ColumnWidth { get; set; }


        public TableSettings()
        {
            IconSettings = new List<KeyValuePair<string, List<string>>>();
            DisplayColumms = new List<string>();
            HideColumms = new List<string>();
            TextTypeAndName = new List<KeyValuePair<int, string>>();
            RowHeight = 40;
            ColumnWidth = new List<KeyValuePair<string, int>>();
        }

        public override string ToString()
        {
            return TableName;
        }
    }
}
