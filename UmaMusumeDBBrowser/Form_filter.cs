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

namespace UmaMusumeDBBrowser
{
    public partial class Form_filter : Form
    {
        private TableSettings settings;
        public Form_filter(TableSettings tableSettings)
        {
            InitializeComponent();
            settings = tableSettings;
            if (settings.IconSettings.Count == 0)
                button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int index = settings.IconSettings.FindIndex(a => a.Key.Equals((string)listBox1.SelectedItem));
            Form_imageList imagelist = new Form_imageList(Application.StartupPath, settings.IconSettings[index].Value);
            if (imagelist.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = imagelist.listView1.SelectedItems[0].ImageKey;
            }
            //imagelist.BackgroundImage
            imagelist.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox3.Items.Add(new FilterData() { Column = (string)listBox1.SelectedItem, Op = (string)listBox2.SelectedItem, Value = textBox1.Text });
        }

        public string GetFilter()
        {
            if (!checkBox1.Checked)
            {
                List<string> lines = new List<string>();
                foreach (var item in listBox3.Items)
                {
                    lines.Add(((FilterData)item).ToString());
                }
                return string.Join(" AND ", lines);
            }
            else
            {
                return textBox2.Text;
            }
        }

        private void Form_filter_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex == -1)
                return;
            listBox3.Items.RemoveAt(listBox3.SelectedIndex);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex != -1)
            {
                FilterData data = (FilterData)listBox3.SelectedItem;
                listBox1.SelectedItem = data.Column;
                listBox2.SelectedItem = data.Op;
                textBox1.Text = data.Value;
                button6.Enabled = true;
            }
            else
                button6.Enabled = false;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex == -1)
                return;
            listBox3.Items.RemoveAt(listBox3.SelectedIndex);
            listBox3.Items.Add(new FilterData() { Column = (string)listBox1.SelectedItem, Op = (string)listBox2.SelectedItem, Value = textBox1.Text });
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (settings.IconSettings.FindIndex(a => a.Key.Equals((string)listBox1.SelectedItem)) != -1)
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = checkBox1.Checked;
            StandartFilterEnable(!checkBox1.Checked);
        }

        private void StandartFilterEnable(bool enable)
        {
            button2.Enabled = enable;
            button6.Enabled = enable;
            button5.Enabled = enable;
            listBox1.Enabled = enable;
            listBox2.Enabled = enable;
            listBox3.Enabled = enable;
            textBox1.Enabled = enable;
            button1.Enabled = enable;
        }
    }
}
