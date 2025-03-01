using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace UmaMusumeDBBrowser
{
    public static class Extensions
    {

        private delegate void TextDelegate(Control textBox, string text);
        private delegate void ListDelegate1(ListBox listBox);
        private delegate void ListDelegate2(ListBox listBox, object text);
        private delegate void VisibleDelegate(Control control, bool visible);
        private delegate void HeightDelegate(Control control, int Height);
        private delegate void ImageDelegate(PictureBox control, Image img);

        private delegate void GrigDelegate(DataGridView gridView, object val);
        private delegate void GrigColDelegate(DataGridView gridView, string cName, object val);
        private delegate void GrigRowDelegate(DataGridView gridView, int rowIndex, object val);

        public static void SetGridDataSource(DataGridView gridView, object val)
        {
            if (gridView.InvokeRequired)
            {
                var d = new GrigDelegate(SetGridDataSource);
                gridView.Invoke(d, new object[] { gridView, val });
            }
            else
                gridView.DataSource = val;
        }


        public static void SetGridRowBackColor(DataGridView gridView, int rowIndex, object val)
        {
            if (gridView.InvokeRequired)
            {
                var d = new GrigRowDelegate(SetGridRowBackColor);
                gridView.Invoke(d, new object[] { gridView, rowIndex, val });
            }
            else
                gridView.Rows[rowIndex].DefaultCellStyle.BackColor = (Color)val;
        }

        public static void SetGridRowForeColor(DataGridView gridView, int rowIndex, object val)
        {
            if (gridView.InvokeRequired)
            {
                var d = new GrigRowDelegate(SetGridRowForeColor);
                gridView.Invoke(d, new object[] { gridView, rowIndex, val });
            }
            else
                gridView.Rows[rowIndex].DefaultCellStyle.ForeColor = (Color)val;
        }

        public static void SetGridColumnVisible(DataGridView gridView, string cName, object val)
        {
            if (gridView.InvokeRequired)
            {
                var d = new GrigColDelegate(SetGridColumnVisible);
                gridView.Invoke(d, new object[] { gridView, cName, val });
            }
            else
                gridView.Columns[cName].Visible = (bool)val;
        }

        public static void SetGridColumnSizeMode(DataGridView gridView, string cName, object val)
        {
            if (gridView.InvokeRequired)
            {
                var d = new GrigColDelegate(SetGridColumnSizeMode);
                gridView.Invoke(d, new object[] { gridView, cName, val });
            }
            else
                gridView.Columns[cName].AutoSizeMode = (DataGridViewAutoSizeColumnMode)val;
        }


        public static void SetTextToControl(Control textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                var d = new TextDelegate(SetTextToControl);
                textBox.Invoke(d, new object[] { textBox, text });
            }
            else
                textBox.Text = text;
        }

        public static void AddTextToList(ListBox listBox, object text)
        {
            if (listBox.InvokeRequired)
            {
                var d = new ListDelegate2(AddTextToList);
                listBox.Invoke(d, new object[] { listBox, text });
            }
            else
                listBox.Items.Add(text);
        }

        public static void ClearListBox(ListBox listBox)
        {
            if (listBox.InvokeRequired)
            {
                var d = new ListDelegate1(ClearListBox);
                listBox.Invoke(d, new object[] { listBox });
            }
            else
                listBox.Items.Clear();
        }


        public static void SetImageToPicBox(PictureBox picBox, Image img)
        {
            if (picBox.InvokeRequired)
            {
                var d = new ImageDelegate(SetImageToPicBox);
                picBox.Invoke(d, new object[] { picBox, img });
            }
            else
                picBox.Image = img;
        }

        public static void SetControlVisible(Control control, bool visible)
        {
            if (control.InvokeRequired)
            {
                var d = new VisibleDelegate(SetControlVisible);
                control.Invoke(d, new object[] { control, visible });
            }
            else
                control.Visible = visible;
        }

        public static void SetControlEnable(Control control, bool enable)
        {
            if (control.InvokeRequired)
            {
                var d = new VisibleDelegate(SetControlEnable);
                control.Invoke(d, new object[] { control, enable });
            }
            else
                control.Enabled = enable;
        }

        public static void SetControlHeight(Control control, int Height)
        {
            if (control.InvokeRequired)
            {
                var d = new HeightDelegate(SetControlHeight);
                control.Invoke(d, new object[] { control, Height });
            }
            else
                control.Height = Height;
        }


        public static dynamic GetValue(this Mat mat, int row, int col)
        {
            var value = CreateElement(mat.Depth);
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }

        public static void SetValue(this Mat mat, int row, int col, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }
        private static dynamic CreateElement(DepthType depthType, dynamic value)
        {
            var element = CreateElement(depthType);
            element[0] = value;
            return element;
        }

        private static dynamic CreateElement(DepthType depthType)
        {
            if (depthType == DepthType.Cv8S)
            {
                return new sbyte[1];
            }
            if (depthType == DepthType.Cv8U)
            {
                return new byte[1];
            }
            if (depthType == DepthType.Cv16S)
            {
                return new short[1];
            }
            if (depthType == DepthType.Cv16U)
            {
                return new ushort[1];
            }
            if (depthType == DepthType.Cv32S)
            {
                return new int[1];
            }
            if (depthType == DepthType.Cv32F)
            {
                return new float[1];
            }
            if (depthType == DepthType.Cv64F)
            {
                return new double[1];
            }
            return new float[1];
        }

        public static Image CropAtRect(this Image b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, -r.X, -r.Y);
                return nb;
            }
        }
        public static float PercentageComparison(this String orig, String text)
        {
            if (orig.Length != text.Length)
                return 0;
            byte[] origBytes = Encoding.GetEncoding(932).GetBytes(orig);
            byte[] textBytes = Encoding.GetEncoding(932).GetBytes(text);
            if (origBytes.Length != textBytes.Length)
                return 0;
            int badBytesCount = 0;
            for (int i = 0; i < origBytes.Length; i++)
            {
                if (origBytes[i] != textBytes[i])
                    badBytesCount++;
            }
            return 1 - (badBytesCount / origBytes.Length);
        }

        public static float PercentageComparison(this String orig, Encoding encodingTextBytes, byte[] textBytes)
        {
            byte[] origBytes = encodingTextBytes.GetBytes(orig);
            if (origBytes.Length != textBytes.Length)
                return 0;
            int badBytesCount = 0;
            for (int i = 0; i < origBytes.Length; i++)
            {
                if (origBytes[i] != textBytes[i])
                    badBytesCount++;
            }
            return 1.0f - ((float)badBytesCount / (float)origBytes.Length);
        }



        public static async void CheckForUpdateAsync(string currentVersion)
        {
            string data = null;
            await Task.Run(async () =>
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+jso");
                        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
                        client.DefaultRequestHeaders.Add("User-Agent", "UmaMusumeDBBrowser");
                        data = await client.GetStringAsync("https://api.github.com/repos/starik222/UmaMusumeDBBrowser/releases");
                    }
                }
                catch (Exception)
                {
                    return;
                }
            });
            if (!string.IsNullOrWhiteSpace(data))
            {
                try
                {
                    List<ReleaseInfo> releasesList = JsonConvert.DeserializeObject<List<ReleaseInfo>>(data);

                    releasesList.Sort((b, a) => a.published_at.CompareTo(b.published_at));
                    currentVersion = "v" + currentVersion;
                    int curIndex = releasesList.FindIndex(a => currentVersion.StartsWith(a.tag_name));
                    if (curIndex <= 0)
                        return;

                    List<ReleaseInfo> nVersions = releasesList.Take(curIndex).ToList();
                    string url = nVersions[0].html_url;
                    StringBuilder releaseNote = new StringBuilder();
                    foreach (var item in nVersions)
                    {
                        string text = item.body;
                        string[] listItems = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        releaseNote.AppendLine(item.tag_name + ":");
                        for (int i = 0; i < listItems.Length; i++)
                        {
                            releaseNote.AppendLine(listItems[i]);
                        }
                    }
                    if (MessageBox.Show($"Обнаружена новая версия программы ({nVersions[0].tag_name.Substring(1)}).\nНовое в версии:\n{releaseNote}\nХотите перейти на страницу загрузки?",
                        "Найдено обновление программы", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
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
