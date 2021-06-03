using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;

namespace UmaMusumeDBBrowser
{
    class SQLiteTableReader
    {
        private string connectionString;

        private SqliteConnection connection;
        public bool IsConnected { get; set; } = false;

        private string appPath;

        public SQLiteTableReader(string AppPath = null, string dbPath = "master.db")
        {
            connectionString = $"Data Source={dbPath}";
            connection = new SqliteConnection(connectionString);
            appPath = AppPath;
        }

        public void Connect()
        {
            connection.Open();
            IsConnected = true;
        }

        public void Disconnect()
        {
            connection.Close();
            IsConnected = false;
        }

        public DataTable ExecuteQuery(string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            DataTable table = new DataTable();

            using (var reader = command.ExecuteReader())
            {
                DataTable metadata = reader.GetSchemaTable();
                for (int i = 0; i < metadata.Rows.Count; i++)
                {
                    string cName = metadata.Rows[i][SchemaTableColumn.ColumnName].ToString();
                    Type cType = (Type)metadata.Rows[i][SchemaTableColumn.DataType];
                    table.Columns.Add(cName, cType);
                }
                while (reader.Read())
                {
                    var row = table.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string cName = reader.GetName(i);
                        row[cName] = reader[i];
                    }
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        public (DataTable table, TableSettings settings) GetDataTableByText(string text, List<TableSettings> tableSettings)
        {
            var command = connection.CreateCommand();
            string query = "SELECT \"index\", category FROM text_data WHERE text='" + text + "'";
            command.CommandText = query;
            Dictionary<int, List<Int64>> data = new Dictionary<int, List<Int64>>();//key - category, value - index
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int index = Convert.ToInt32(reader[0]);
                    int category = Convert.ToInt32(reader[1]);
                    if (data.ContainsKey(category))
                        data[category].Add(index);
                    else
                    {
                        data.Add(category, new List<Int64>(new Int64[] { index }));
                    }
                }
            }
            foreach (var item in tableSettings)
            {
                foreach (var dataItem in data)
                {
                    if (item.TextTypeAndName.Exists(a => a.Key == dataItem.Key))
                    {
                        return (GetDataTable(item, dataItem.Value), item);
                    }
                }
            }
            return (null, null);
        }

        public DataTable GetDataTable(TableSettings settings, List<Int64> filterByTextIds = null)
        {
            var command = connection.CreateCommand();
            if (!string.IsNullOrWhiteSpace(settings.TextIndexColumn) && settings.TextTypeAndName.Count > 0)
            {
                List<string> textQueries = new List<string>();
                string mainQuery = "";
                if (string.IsNullOrWhiteSpace(settings.CustomQueryMainTable))
                {
                    mainQuery = "(SELECT " + settings.TextIndexColumn + ", " + string.Join(", ", settings.DisplayColumms) +
                        " FROM " + settings.TableName + ") as tm";
                }
                else
                {
                    mainQuery = "(" + settings.CustomQueryMainTable + ") as tm";
                }
                textQueries.Add(mainQuery);
                int a = 0;
                foreach (var item in settings.TextTypeAndName)
                {
                    string query = "(SELECT \"index\", text FROM text_data WHERE category=" + item.Key + ") as t" + a++;
                    textQueries.Add(query);
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT ");
                for (int i = 0; i < settings.DisplayColumms.Count; i++)
                {
                    sb.Append("tm." + settings.DisplayColumms[i]);
                    if (i != settings.DisplayColumms.Count - 1)
                        sb.Append(", ");
                }
                for (int i = 0; i < settings.TextTypeAndName.Count; i++)
                {
                    sb.Append(", t" + i + ".text as " + settings.TextTypeAndName[i].Value);
                    sb.Append(", \"\" as " + settings.TextTypeAndName[i].Value + "_trans");
                }
                sb.Append(" FROM ");
                sb.Append(string.Join(", ", textQueries));
                sb.Append(" WHERE ");
                for (int i = 0; i < settings.TextTypeAndName.Count; i++)
                {
                    sb.Append("tm." + settings.TextIndexColumn + " = " + "t" + i + ".\"index\"");
                    if (i != settings.TextTypeAndName.Count - 1)
                        sb.Append(" AND ");
                }

                if (filterByTextIds != null)
                {
                    sb.Append(" AND (");
                    for (int i = 0; i < filterByTextIds.Count; i++)
                    {
                        sb.Append("tm." + settings.TextIndexColumn + " = " + filterByTextIds[i]);
                        if (i < filterByTextIds.Count - 1)
                            sb.Append(" OR ");
                    }
                    sb.Append(")");
                }
                command.CommandText = sb.ToString();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(settings.CustomQueryMainTable))
                {
                    command.CommandText = "SELECT " + string.Join(", ", settings.DisplayColumms) +
        " FROM " + settings.TableName;
                }
                else
                {
                    command.CommandText = settings.CustomQueryMainTable;
                }
            }
            DataTable table = new DataTable(settings.TableName);

            using (var reader = command.ExecuteReader())
            {
                bool useImage = false;
                DataTable metadata = reader.GetSchemaTable();
                for (int i = 0; i < metadata.Rows.Count; i++)
                {
                    string cName = metadata.Rows[i][SchemaTableColumn.ColumnName].ToString();
                    Type cType = (Type)metadata.Rows[i][SchemaTableColumn.DataType];
                    table.Columns.Add(cName, cType);

                    if (settings.IconSettings.Count > 0)
                    {
                        int iconIndex = settings.IconSettings.FindIndex(a => a.Key.Equals(cName));
                        if (iconIndex != -1)
                        {
                            useImage = true;
                            table.Columns.Add(cName + "_image", typeof(Image));
                            table.Columns.Add(cName + "_imagePath", typeof(string));
                        }
                    }
                }
                while (reader.Read())
                {
                    var row = table.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string cName = reader.GetName(i);
                        int iconIndex = -1;
                        if (useImage && (iconIndex = settings.IconSettings.FindIndex(a => a.Key.Equals(cName))) != -1)
                        {
                            object icon = reader[i];
                            row[cName] = icon;
                            var iconPath = FindFile(appPath, settings.IconSettings[iconIndex].Value, icon + ".png");
                            if (iconPath.fullPath != null)
                            {
                                //Image img = Image.FromFile(iconPath);

                                row[cName + "_image"] = Program.IconDB.GetImageByKey(iconPath.PathOnRoot, icon.ToString());
                                row[cName + "_imagePath"] = iconPath.fullPath;
                            }
                        }
                        else
                        {
                            if (settings.TextTypeAndName.FindIndex(b => b.Value.Equals(cName)) != -1)
                            {
                                row[cName] = ((string)reader[i]).Replace("\\n", "");
                            }
                            else
                                row[cName] = reader[i];
                        }

                    }
                    table.Rows.Add(row);
                }
            }

            TableModifier.ModifyTable(ref table);
            return table;
        }

        private (string fullPath, string PathOnRoot) FindFile(string appPath, List<string> dirs, string fName)
        {
            foreach (var item in dirs)
            {
                string filePath = Path.Combine(appPath, item, fName);
                if (File.Exists(filePath))
                    return (filePath, item);
            }
            return (null, null);
        }


    }
}
