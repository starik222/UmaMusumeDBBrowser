using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Translator;
using ExcelPrint;
using System.Text.RegularExpressions;

namespace UmaMusumeDBBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Program.IsDebug)
            {
                toolStripButton7.Visible = false;
                toolStripButton5.Visible = false;
            }
            Text += " ver. " + Application.ProductVersion;
            Program.TableDisplaySettings = new SettingsLoader().LoadSettings();
            if (File.Exists(Path.Combine(Application.StartupPath, "Dictonaries\\Languages.txt")))
            {
                toolStripComboBox1.Items.AddRange(File.ReadAllLines(Path.Combine(Application.StartupPath, "Dictonaries\\Languages.txt")));
                string selectedLang = (string)Properties.Settings.Default["SelectedLang"];
                if (!string.IsNullOrWhiteSpace(selectedLang))
                {
                    if (toolStripComboBox1.Items.Contains(selectedLang))
                        toolStripComboBox1.SelectedItem = selectedLang;
                }
                else if (toolStripComboBox1.Items.Count > 0)
                    toolStripComboBox1.SelectedIndex = 0;
            }


            foreach (var item in Program.TableDisplaySettings)
            {
                listBox1.Items.Add(item);
            }


        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                CheckBeforeReload();
                SetDataTable((TableSettings)listBox1.SelectedItem);
            }
        }

        private List<NAMES> currentDictonary = null;
        private TableSettings currentTableSettings = null;
        private DataTable currentTable = null;
        private string selectedLanguages = null;

        public void LoadTableByText(string text)
        {
            SQLiteTableReader reader = new SQLiteTableReader(Application.StartupPath, Program.DbPath);
            reader.Connect();
            var data = reader.GetDataTableByText(text, Program.TableDisplaySettings);
            reader.Disconnect();
            listBox1.ClearSelected();
            if (data.table == null)
            {
                dataGridView1.DataSource = null;
                currentTableSettings = null;
                currentTable = null;
            }
            else
            {
                CheckBeforeReload();
                SetDataTable(data.settings, data.table);
            }
        }

        public void LoadTableByIds(string tableName, List<Int64> ids)
        {
            var tableSetting = Program.TableDisplaySettings.Find(a => a.TableName.Equals(tableName));

            SQLiteTableReader reader = new SQLiteTableReader(Application.StartupPath, Program.DbPath);
            reader.Connect();
            var table = reader.GetDataTable(tableSetting, ids);
            reader.Disconnect();
            listBox1.ClearSelected();
            if (table == null)
            {
                dataGridView1.DataSource = null;
                currentTableSettings = null;
                currentTable = null;
            }
            else
            {
                CheckBeforeReload();
                SetDataTable(tableSetting, table);
            }
        }

        private void SetDataTable(TableSettings settings, DataTable preloadedTable = null)
        {
            DataTable table = null;
            if (preloadedTable == null)
            {
                SQLiteTableReader reader = new SQLiteTableReader(Application.StartupPath, Program.DbPath);
                reader.Connect();
                table = reader.GetDataTable(settings);
                reader.Disconnect();
            }
            else
                table = preloadedTable;

            currentDictonary = Program.TransDict.LoadDictonary(Path.Combine(Program.DictonariesDir, settings.TableName + "_" + toolStripComboBox1.SelectedItem + ".txt"));
            if (currentDictonary.Count > 0)
            {
                foreach (var item in settings.TextTypeAndName)
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        string original = (string)table.Rows[i][item.Value];
                        if (!string.IsNullOrWhiteSpace(original))
                        {
                            string trans = Program.TransDict.GetTranslation(currentDictonary, original);
                            if (!string.IsNullOrWhiteSpace(trans))
                                table.Rows[i][item.Value + "_trans"] = trans;
                        }
                    }
                }
            }
            table.AcceptChanges();
            dataGridView1.RowTemplate.Height = settings.RowHeight;
            dataGridView1.DataSource = table;
            //dataGridView1.RowTemplate.Height = 50;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                if (dataGridView1.Columns[i].ValueType == typeof(Image))
                {
                    ((DataGridViewImageColumn)dataGridView1.Columns[i]).ImageLayout = DataGridViewImageCellLayout.Zoom;
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                else if (dataGridView1.Columns[i].Name.EndsWith("_imagePath"))
                {
                    dataGridView1.Columns[i].Visible = false;
                }
                else
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[i].ReadOnly = true;
                }
            }
            foreach (var item in settings.TextTypeAndName)
            {
                dataGridView1.Columns[item.Value].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns[item.Value].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridView1.Columns[item.Value].ReadOnly = false;

                dataGridView1.Columns[item.Value + "_trans"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns[item.Value + "_trans"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridView1.Columns[item.Value + "_trans"].ReadOnly = false;
            }
            foreach (var item in settings.ColumnWidth)
            {
                dataGridView1.Columns[item.Key].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns[item.Key].Width = item.Value;
                dataGridView1.Columns[item.Key].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            
            currentTableSettings = settings;
            currentTable = table;
            if (filter != null)
            {
                filter.Close();
                filter = null;
            }
            
            // var test = table.DefaultView.RowFilter = "LEN(SkillName)=5";
        }

        private void CheckBeforeReload()
        {
            if (currentDictonary == null || dataGridView1.DataSource == null)
                return;

            DataTable table = ((DataTable)dataGridView1.DataSource).GetChanges();
            if (table != null)
            {
                if (MessageBox.Show("Найдены несохраненные изменения.\nСохранить изменения в словарь перевода?", "Найдены изменения", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    SaveCurrentDictonary();
            }
            //DataTable temp = (DataTable)dataGridView1.DataSource;
            dataGridView1.DataSource = null;
            //temp.Dispose();
            GC.Collect();
        }

        private void SaveCurrentDictonary()
        {
            if (currentDictonary == null || dataGridView1.DataSource == null)
                return;
            if (selectedLanguages == null)
            {
                MessageBox.Show("Не выбран язык перевода!");
                return;
            }
            DataTable table = ((DataTable)dataGridView1.DataSource).GetChanges();
            if (table == null)
                return;
            foreach (var item in currentTableSettings.TextTypeAndName)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    Program.TransDict.SetTranslation(ref currentDictonary, table.Rows[i][item.Value] != DBNull.Value ? (string)table.Rows[i][item.Value] : null, table.Rows[i][item.Value + "_trans"] != DBNull.Value ? (string)table.Rows[i][item.Value + "_trans"] : null);
                }
            }
            Program.TransDict.SaveDictonary(currentDictonary, Path.Combine(Program.DictonariesDir, currentTableSettings.TableName + "_" + selectedLanguages + ".txt"));
            ((DataTable)dataGridView1.DataSource).AcceptChanges();
            MessageBox.Show("Сохранено");
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveCurrentDictonary();
        }

        private void перевестиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                string cName = dataGridView1.Columns[dataGridView1.SelectedCells[i].ColumnIndex].Name;
                if (currentTableSettings.TextTypeAndName.FindIndex(a => a.Value.Equals(cName)) != -1)
                {
                    if (selectedLanguages != null && dataGridView1.SelectedCells[i].Value != DBNull.Value && !string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                        dataGridView1[cName + "_trans", dataGridView1.SelectedCells[i].RowIndex].Value = Program.tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, selectedLanguages, true);
                }
            }
        }

        private Form_filter filter = null;
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (currentTable == null || currentTableSettings == null)
                return;
            if (filter == null)
            {
                filter = new Form_filter(currentTableSettings);
                for (int i = 0; i < currentTable.Columns.Count; i++)
                {
                    if (!currentTable.Columns[i].ColumnName.EndsWith("_image"))
                        filter.listBox1.Items.Add(currentTable.Columns[i].ColumnName);
                }
            }
            if (filter.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = filter.GetFilter();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка установки фильтра", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (currentTable == null || currentTableSettings == null)
                return;
            try
            {
                ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = string.Empty;
                if (filter != null)
                    filter.listBox3.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка установки фильтра", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (currentTableSettings == null)
                return;
            int hiddenColumnCount = 0;
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (!dataGridView1.Columns[i].Visible)
                    hiddenColumnCount++;
            }
            PrintToExcel excel = new PrintToExcel();
            excel.CreateWorkBook();
            excel.SetCurrentSheet(1);
            var copyMode = dataGridView1.ClipboardCopyMode;
            dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dataGridView1.SelectAll();
            excel.SelectedCellsToBuffer(dataGridView1);
            excel.PasteFromBuffer("Table: " + currentTableSettings.TableName, dataGridView1.Rows.Count, dataGridView1.RowTemplate.Height, dataGridView1.Columns.Count + 1 - hiddenColumnCount, 70, true, false);
            int curHidden = 0;
            var dpi = GetDPI();
            for (int j = 0; j < dataGridView1.Columns.Count; j++)
            {
                if (!dataGridView1.Columns[j].Visible)
                    curHidden++;
                if (dataGridView1.Columns[j].ValueType != typeof(Image))
                    continue;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    excel.SetCurrentCell(i + 3, j + 2 - curHidden);
                    string imagePath = string.Empty;
                    if (dataGridView1[dataGridView1.Columns[j].Name + "Path", i].Value != DBNull.Value)
                    {
                        Size imgSize = ((Image)(dataGridView1[dataGridView1.Columns[j].Name, i].Value)).Size;
                        double scale = imgSize.Width / imgSize.Height;
                        if (excel.CurrentCells.Width < excel.CurrentCells.Height * scale)
                        {
                            excel.CurrentCells.EntireColumn.ColumnWidth = excel.CurrentCells.EntireRow.RowHeight * scale / 5;
                        }
                        excel.Worksheet.Shapes.AddPicture((string)dataGridView1[dataGridView1.Columns[j].Name + "Path", i].Value, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, excel.CurrentCells.Left, excel.CurrentCells.Top, excel.CurrentCells.Height * scale, excel.CurrentCells.Height);
                    }
                    
                }
            }
            excel.Finally();

            dataGridView1.ClipboardCopyMode = copyMode;
        }

        private PointF GetDPI()
        {
            PointF pf = new PointF();
            Graphics g = this.CreateGraphics();
            try
            {
                pf.X = g.DpiX;
                pf.Y = g.DpiY;
            }
            finally
            {
                g.Dispose();
            }
            return pf;
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1_SelectedIndexChanged(sender, e);
            selectedLanguages = (string)toolStripComboBox1.SelectedItem;
            Properties.Settings.Default["SelectedLang"] = (string)toolStripComboBox1.SelectedItem;
            Properties.Settings.Default.Save();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Form_GameWindow gameWindow = new Form_GameWindow();
            gameWindow.Show();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Form_GameRecognizer form_GameRecognizer = new Form_GameRecognizer();
            //form_GameRecognizer.Owner = this;
            form_GameRecognizer.parentForm = this;
            toolStripComboBox1.Enabled = false;
            form_GameRecognizer.Show();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            Regex regex = new Regex("\"(.*?)\",\\s?\"(.*?)\"", RegexOptions.Singleline);
            List<NAMES> dict = new List<NAMES>();
            foreach (var file in openFileDialog.FileNames)
            {
                string[] lines = File.ReadAllLines(file);
                foreach (var item in lines)
                {
                    Match m = regex.Match(item);
                    if (m.Success)
                    {
                        string orig = m.Groups[1].Value;
                        string trans = m.Groups[2].Value;
                        if (trans.Contains("<size"))
                        {
                            trans = Regex.Replace(trans, "<size=\\d*>", "");
                            trans = trans.Replace("</size>", "");
                        }
                        orig = orig.Replace("\\n", "");
                        trans = trans.Replace("\\n", " ");
                        dict.Add(new NAMES() { orig_name = orig, translit_name = trans });
                    }
                }
            }

            foreach (var item in currentTableSettings.TextTypeAndName)
            {
                for (int i = 0; i < currentTable.Rows.Count; i++)
                {
                    string original = (string)currentTable.Rows[i][item.Value];
                    if (!string.IsNullOrWhiteSpace(original))
                    {
                        string trans = Program.TransDict.GetTranslation(dict, original);
                        if (!string.IsNullOrWhiteSpace(trans))
                            currentTable.Rows[i][item.Value + "_trans"] = trans;
                    }
                }
            }

            if (MessageBox.Show("Произвести поиск совпадений в тексте?", "Поиск подстрок в тексте", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (var item in currentTableSettings.TextTypeAndName)
                {
                    for (int i = 0; i < currentTable.Rows.Count; i++)
                    {
                        string original = (string)currentTable.Rows[i][item.Value];
                        if (!string.IsNullOrWhiteSpace(original))
                        {
                            if (currentTable.Rows[i][item.Value + "_trans"] == DBNull.Value || string.IsNullOrWhiteSpace((string)currentTable.Rows[i][item.Value + "_trans"]))
                            {
                                string trans = Program.tools.CompareAndReplace(original, dict);
                                if (!string.IsNullOrWhiteSpace(trans))
                                    currentTable.Rows[i][item.Value + "_trans"] = trans;
                            }
                        }
                    }
                }
            }

        }
    }
}
