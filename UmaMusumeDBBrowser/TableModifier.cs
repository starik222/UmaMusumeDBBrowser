using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

namespace UmaMusumeDBBrowser
{
    public class TableModifier
    {
        public static void ModifyTable(ref DataTable table)
        {
            if (table.TableName.Equals("skill_data"))
                ModifySkillDataTable(ref table);
        }


        private static void ModifySkillDataTable(ref DataTable table)
        {
            string pattern = "([A-Za-z0-9_]+)([\\=\\!\\<\\>]+)(\\d+)";
            MatchEvaluator evaluator = new MatchEvaluator(ParamChanger);
            if (table.Columns.Contains("condition_1"))
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    string text = (string)table.Rows[i]["condition_1"];
                    table.Rows[i]["condition_1"] = Regex.Replace(text, pattern, evaluator, RegexOptions.IgnoreCase);
                }
            }
        }

        private static string ParamChanger(Match match)
        {
            switch (match.Groups[1].Value)
            {
                case "distance_type":
                    {
                        switch (match.Groups[3].Value)
                        {
                            case "1": return " " + match.Groups[1].Value + match.Groups[2].Value + "Short ";
                            case "2": return " " + match.Groups[1].Value + match.Groups[2].Value + "Mile ";
                            case "3": return " " + match.Groups[1].Value + match.Groups[2].Value + "Medium ";
                            case "4": return " " + match.Groups[1].Value + match.Groups[2].Value + "Long ";
                        }
                        break;
                    }
                case "running_style":
                    {
                        switch (match.Groups[3].Value)
                        {
                            case "1": return " " + match.Groups[1].Value + match.Groups[2].Value + "Runner ";
                            case "2": return " " + match.Groups[1].Value + match.Groups[2].Value + "Leader ";
                            case "3": return " " + match.Groups[1].Value + match.Groups[2].Value + "Betweener ";
                            case "4": return " " + match.Groups[1].Value + match.Groups[2].Value + "Chaser ";
                        }
                        break;
                    }
                case "rotation":
                    {
                        switch (match.Groups[3].Value)
                        {
                            case "1": return " " + match.Groups[1].Value + match.Groups[2].Value + "右(Rigth) ";
                            case "2": return " " + match.Groups[1].Value + match.Groups[2].Value + "左(Left) ";
                        }
                        break;
                    }

            }
            return " " + match.Groups[0].Value + " ";
        }
    }
}
