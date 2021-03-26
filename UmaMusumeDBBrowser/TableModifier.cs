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
            if (table.TableName.Equals("race"))
                ModifyRaceDataTable(ref table);
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

        private static void ModifyRaceDataTable(ref DataTable table)
        {
            ModifyColumnData(GetGrade, "grade", ref table);
            ModifyColumnData(GetTurn, "turn", ref table);
            ModifyColumnData(GetRaceTrack, "race_track_id", ref table);
            ModifyColumnData(GetGround, "ground", ref table);
            ModifRaceDate(ref table);
        }

        private static void ModifRaceDate(ref DataTable table)
        {
            if (table.Columns.Contains("date"))
            {
                DataColumn Col = table.Columns.Add("month", typeof(int));
                Col.SetOrdinal(table.Columns["date"].Ordinal);
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    Int64 val = (Int64)table.Rows[i]["date"];
                    val = val / 100;
                    table.Rows[i]["month"] = (int)val;
                }
            }
        }

        private static void ModifyColumnData(Func<object, string> func, string colName, ref DataTable table)
        {
            if (table.Columns.Contains(colName))
            {
                DataColumn tempCol = table.Columns.Add(colName + "_temp", typeof(string));
                tempCol.SetOrdinal(table.Columns[colName].Ordinal);
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    object val = table.Rows[i][colName];
                    table.Rows[i][colName + "_temp"] = func(val);
                }
                table.Columns.Remove(colName);
                tempCol.ColumnName = colName;
                tempCol.Caption = colName;
            }
        }

        private static string GetGrade(object grade)
        {
            Int64 val = (Int64)grade;
            switch (val)
            {
                case 400:
                    return "Op";
                case 700:
                    return "Pre-Op";
                case 300:
                    return "G3";
                case 200:
                    return "G2";
                case 100:
                    return "G1";
                default:
                    return val.ToString();
            }
        }

        private static string GetGround(object ground)
        {
            Int64 val = (Int64)ground;
            switch (val)
            {
                case 1:
                    return "芝(Трава)";
                case 2:
                    return "ダート(Грязь)";
                default:
                    return val.ToString();
            }
        }

        private static string GetRaceTrack(object track)
        {
            Int64 val = (Int64)track;
            switch (val)
            {
                case 10001:
                    return "札幌";
                case 10002:
                    return "函館";
                case 10003:
                    return "新潟";
                case 10004:
                    return "福島";
                case 10005:
                    return "中山";
                case 10006:
                    return "東京";
                case 10007:
                    return "中京";
                case 10008:
                    return "京都";
                case 10009:
                    return "阪神";
                case 10010:
                    return "小倉";
                case 10101:
                    return "大井";
                default:
                    return val.ToString();
            }
        }

        private static string GetTurn(object turn)
        {
            string val = (string)turn;
            switch (val)
            {
                case "21":
                    return "左(Влево)";
                case "22":
                    return "左・内(Влево/внутри)";
                case "23":
                    return "左・外(Влево/снаружи)";
                case "11":
                    return "右(Вправо)";
                case "12":
                    return "右・内(Вправо/внутри)";
                case "13":
                    return "右・外(Вправо/снаружи)";
                default:
                    return val;
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
