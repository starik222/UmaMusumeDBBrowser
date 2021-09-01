using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace UmaResHelper
{
    public class ResManager : IDisposable
    {
        private List<string> fileList;
        private SQLiteTableReader tableReader;
        private string currentFilter = "";
        public ResManager()
        {
            fileList = new List<string>();
            tableReader = new SQLiteTableReader(null, Program.DbPath);
            tableReader.Connect();
        }

        public void Dispose()
        {
            tableReader.Disconnect();
            fileList = null;
        }

        public DataTable GetFilesOnFilter(string filter, bool needLikePercent)
        {
            currentFilter = filter;
            fileList.Clear();

            string query = "";
            if (needLikePercent)
            {
                query = "SELECT n, h, m FROM a WHERE n LIKE '%" + filter + "%'";
            }
            else
            {
                query = "SELECT n, h, m FROM a WHERE n LIKE '" + filter + "'";
            }
            var table = tableReader.ExecuteQuery(query);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                fileList.Add((string)table.Rows[i]["h"]);
            }
            return table;
        }
        public void CopyFileFromTo(string srcDir, string destDir)
        {
            string fullDestDir = Path.Combine(destDir, CheckInvalidChars(currentFilter));
            
            Directory.CreateDirectory(fullDestDir);
            foreach (var item in fileList)
            {
                string prefix = item.Substring(0, 2);
                string filePath = Path.Combine(srcDir, prefix, item);
                string outPath = Path.Combine(fullDestDir, item);
                File.Copy(filePath, outPath, true);
            }
        }

        private string CheckInvalidChars(string text)
        {
            char[] chr = Path.GetInvalidPathChars();
            var textArray = text.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (!chr.Contains(text[i]))
                    sb.Append(text[i]);
            }
            return sb.ToString();

        }

    }
}
