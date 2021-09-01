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

namespace UmaResHelper
{
    public class SQLiteTableReader
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
