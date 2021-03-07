using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UmaMusumeDBBrowser
{
    public class SettingsLoader
    {

        private string IniFile;

        public SettingsLoader(string iniFile = "Settings.ini")
        {
            IniFile = iniFile;
        }

        public List<TableSettings> LoadSettings()
        {
            List<TableSettings> settings = new List<TableSettings>();
            StreamReader reader = new StreamReader(IniFile);
            while (!reader.EndOfStream)
            {
                if (reader.Peek() == '#')
                {
                    reader.ReadLine();
                    continue;
                }
                if (reader.Peek() != '[')
                    throw new Exception("Формат файла нарушен!");
                TableSettings ts = new TableSettings();
                ts.TableName = reader.ReadLine();
                ts.TableName = ts.TableName.Substring(1, ts.TableName.Length - 2);
                while (!reader.EndOfStream && reader.Peek() != '[')
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;
                    int splitIndex = line.IndexOf("=");
                    if (splitIndex == -1)
                        throw new Exception("Ошибка разбора строки ключ=параметр");
                    string[] subItems = new string[2];
                    subItems[0] = line.Substring(0, splitIndex).Trim();
                    subItems[1] = line.Substring(splitIndex+1).Trim();
                    switch (subItems[0].Trim())
                    {
                        case "TextIndexColumn":
                            {
                                ts.TextIndexColumn = subItems[1];
                                break;
                            }
                        case "DisplayColumms":
                            {
                                string[] listColumn = subItems[1].Split(new char[] { ',' });
                                foreach (var item in listColumn)
                                    ts.DisplayColumms.Add(item.Trim());
                                break;
                            }
                        case "IconSettings":
                            {
                                string[] listParam = subItems[1].Split(new char[] { ',' });
                                if (listParam.Length < 2)
                                    throw new Exception("В секции IconSettings должно быть два параметра и более.");
                                List<string> dirs = new List<string>();
                                for (int i = 1; i < listParam.Length; i++)
                                {
                                    dirs.Add(listParam[i]);
                                }
                                ts.IconSettings.Add(new KeyValuePair<string, List<string>>(listParam[0], dirs));
                                break;
                            }
                        case "TextTypeAndName":
                            {
                                string[] listParam = subItems[1].Split(new char[] { ',' });
                                if(listParam.Length!=2)
                                    throw new Exception("В секции TextTypeAndName должно быть два параметра.");
                                ts.TextTypeAndName.Add(new KeyValuePair<int, string>(Convert.ToInt32(listParam[0].Trim()), listParam[1].Trim()));
                                break;
                            }
                        case "CustomQueryMainTable":
                            {
                                ts.CustomQueryMainTable = subItems[1];
                                break;
                            }
                        case "RowHeight":
                            {
                                ts.RowHeight = Convert.ToInt32(subItems[1]);
                                break;
                            }
                        case "ColumnWidth":
                            {
                                string[] listParam = subItems[1].Split(new char[] { ',' });
                                if (listParam.Length != 2)
                                    throw new Exception("В секции ColumnWidth должно быть два параметра.");
                                ts.ColumnWidth.Add(new KeyValuePair<string, int>(listParam[0].Trim(), Convert.ToInt32(listParam[1].Trim())));
                                break;
                            }
                    }
                }
                settings.Add(ts);
            }

            return settings;
        }
    }
}
