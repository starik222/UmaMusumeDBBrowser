using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Translator;

namespace UmaResHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string outDir = "";
        private ResManager manager;
       
        private void Form1_Load(object sender, EventArgs e)
        {
            manager = new ResManager();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            outDir = openFolderDialog.Folder;
            label2.Text = outDir;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var table = manager.GetFilesOnFilter(textBox1.Text, !checkBox1.Checked);
            dataGridView1.DataSource = table;
            ApplyStyle();
        }

        private void ApplyStyle()
        {
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            manager.CopyFileFromTo(Program.ResPath, outDir);
        }
    }
}
