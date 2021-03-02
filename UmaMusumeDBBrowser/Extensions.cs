using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaMusumeDBBrowser
{
    class Extensions
    {

    }


    public class FilterData
    {
        public string Column;
        public string Op;
        public string Value;

        public FilterData() { }

        public override string ToString()
        {
            switch (Op)
            {
                case "Equals":
                    {
                        return $"{Column}='{Value}'";
                    }
                case "Contains":
                    {
                        return string.Format((string)Column + " LIKE '%{0}%'", Value);
                    }
                case "Length":
                    {
                        return $"LEN({Column})={Value}";
                    }
                default:
                    return "";
            }
        }
    }
}
