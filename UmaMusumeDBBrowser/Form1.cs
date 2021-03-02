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
            Text += " ver. " + Application.ProductVersion;
            Program.TableDisplaySettings = new SettingsLoader().LoadSettings();

            foreach (var item in Program.TableDisplaySettings)
            {
                listBox1.Items.Add(item);
            }

            toolStripComboBox1.SelectedIndex = 0;

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

        private void SetDataTable(TableSettings settings)
        {
            
            SQLiteTableReader reader = new SQLiteTableReader(Application.StartupPath);
            reader.Connect();
            var table = reader.GetDataTable(settings);
            reader.Disconnect();

            currentDictonary = Program.TransDict.LoadDictonary(Path.Combine(Program.DictonariesDir, settings.TableName + ".txt"));
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
            dataGridView1.DataSource = table;
            //dataGridView1.RowTemplate.Height = 50;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                if (dataGridView1.Columns[i].ValueType == typeof(Image))
                {
                    ((DataGridViewImageColumn)dataGridView1.Columns[i]).ImageLayout = DataGridViewImageCellLayout.Zoom;
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
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
            if (table == null)
                return;
            if (MessageBox.Show("Найдены несохраненные изменения.\nСохранить изменения в словарь перевода?", "Найдены изменения", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                SaveCurrentDictonary();
        }

        private void SaveCurrentDictonary()
        {
            if (currentDictonary == null || dataGridView1.DataSource == null)
                return;
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
            Program.TransDict.SaveDictonary(currentDictonary, Path.Combine(Program.DictonariesDir, currentTableSettings.TableName + ".txt"));
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
                    if (dataGridView1.SelectedCells[i].Value != DBNull.Value && !string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                        dataGridView1[cName + "_trans", dataGridView1.SelectedCells[i].RowIndex].Value = Program.tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, toolStripComboBox1.Text, false);
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
    }
}
