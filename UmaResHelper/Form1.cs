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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            ExtractTextFromScriptsAsync(openFolderDialog.Folder);
        }

        private void ExtractTextFromScripts(string dir)
        {
            string[] files = Directory.GetFiles(dir, "StoryTimelineTextClipData*", SearchOption.AllDirectories);
            List<ScriptItem> texts = new List<ScriptItem>();
            string name = "";
            string text = "";
            foreach (var item in files)
            {
                try
                {
                    JObject jsonFile = JObject.Parse(File.ReadAllText(item));
                    name = jsonFile["Name"].ToString();
                    text = jsonFile["Text"].ToString();
                    var listItem = new ScriptItem() { Name = name, Text = text };
                    if (!IsTextListContainsItem(texts, listItem))
                        texts.Add(listItem);
                }
                catch (Exception ex)
                {
                    File.AppendAllLines("errorList.txt", new string[] { item });
                }
            }
            File.WriteAllText("DialogueTexts.json", JsonConvert.SerializeObject(texts));
        }

        private async void ExtractTextFromScriptsAsync(string dir)
        {
            await Task.Run(() => ExtractTextFromScripts(dir));
            MessageBox.Show("Завершено!");
        }

        private bool IsTextListContainsItem(List<ScriptItem> list, ScriptItem item)
        {
            if (list.Exists(a => a.Name == item.Name && a.Text == item.Text))
                return true;
            return false;
        }


        private class ScriptItem
        {
            public string Name;
            public string Text;
        }
    }
}
