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
using System.Text.RegularExpressions;

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
                    if (jsonFile["ChoiceDataList"] != null && ((JArray)jsonFile["ChoiceDataList"]).Count>0)
                    {
                        foreach (var choice in jsonFile["ChoiceDataList"])
                        {
                            listItem.ChoiceDataList.Add(new ScriptSubItem() { Text = choice["Text"].Value<string>() });
                        }
                    }
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


        private void ExtractAndAppendTextFromScripts(string destJson, string dir)
        {
            var scriptItems = JsonConvert.DeserializeObject<List<ScriptItem>>(File.ReadAllText(destJson));

            string[] files = Directory.GetFiles(dir, "StoryTimelineTextClipData*", SearchOption.AllDirectories);
           // List<ScriptItem> texts = new List<ScriptItem>();
            string name = "";
            string text = "";
            foreach (var item in files)
            {
                try
                {
                    JObject jsonFile = JObject.Parse(File.ReadAllText(item));
                    name = jsonFile["Name"].ToString();
                    text = jsonFile["Text"].ToString();

                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(text))
                        continue;

                    var listItem = new ScriptItem() { Name = name, Text = text };
                    if (jsonFile["ChoiceDataList"] != null && ((JArray)jsonFile["ChoiceDataList"]).Count > 0)
                    {
                        foreach (var choice in jsonFile["ChoiceDataList"])
                        {
                            listItem.ChoiceDataList.Add(new ScriptSubItem() { Text = choice["Text"].Value<string>() });
                        }
                    }
                    int index = scriptItems.FindIndex(a => a.Name == listItem.Name && a.Text == listItem.Text);
                    if (index == -1)
                    {
                        scriptItems.Add(listItem);
                    }
                    else
                    {
                        foreach (var subItem in listItem.ChoiceDataList)
                        {
                            if (!scriptItems[index].ChoiceDataList.Exists(a => a.Text == subItem.Text))
                                scriptItems[index].ChoiceDataList.Add(subItem);
                        }
                    }
                    //if (!IsTextListContainsItem(texts, listItem))
                    //    texts.Add(listItem);
                }
                catch (Exception ex)
                {
                    File.AppendAllLines("errorList.txt", new string[] { item });
                }
            }
            File.WriteAllText("DialogueTexts.json", JsonConvert.SerializeObject(scriptItems, Formatting.Indented));
        }

        private async void ExtractTextFromScriptsAsync(string dir)
        {
            await Task.Run(() => ExtractTextFromScripts(dir));
            MessageBox.Show("Завершено!");
        }

        private async void ExtractAndAppendTextFromScriptsAsync(string destJson, string dir)
        {
            await Task.Run(() => ExtractAndAppendTextFromScripts(destJson, dir));
            MessageBox.Show("Завершено!");
        }

        private bool IsTextListContainsItem(List<ScriptItem> list, ScriptItem item)
        {
            if (list.Exists(a => a.Name == item.Name && a.Text == item.Text && a.ChoiceDataList.Count == item.ChoiceDataList.Count))
                return true;
            return false;
        }


        private class ScriptItem
        {
            public string Name;
            public string Text;
            public string TextTrans;
            public List<ScriptSubItem> ChoiceDataList;

            public ScriptItem()
            {
                Name = "";
                Text = "";
                TextTrans = "";
                ChoiceDataList = new List<ScriptSubItem>();
            }
        }

        private class ScriptItem2
        {
            public string Name;
            public string NameTrans;

            public List<ScriptSubItem> TextList;

            public ScriptItem2()
            {
                TextList = new List<ScriptSubItem>();
            }

        }

        private class ScriptItemV2
        {
            public string Name;
            public string NameTrans;

            public SortedDictionary<int, List<ScriptSubItem>> TextList;

            public ScriptItemV2()
            {
                Name = "";
                NameTrans = "";
                TextList = new SortedDictionary<int, List<ScriptSubItem>>();
            }

        }

        private class ScriptSubItem
        {
            public string Text;
            public string TransText;
            public List<ScriptSubItem> ChoiceDataList;

            public ScriptSubItem()
            {
                Text = "";
                TransText = "";
                ChoiceDataList = new List<ScriptSubItem>();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            ConvertAndTranslateAsync(openFileDialog.FileName);
        }

        private async void ConvertAndTranslateAsync(string path)
        {
            await Task.Run(() => ConvertAndTranslate(path));
            MessageBox.Show("Завершено!");
        }

        private void ConvertAndTranslate(string path)
        {
            var scriptItems = JsonConvert.DeserializeObject<List<ScriptItem>>(File.ReadAllText(path));
            List<ScriptItemV2> siVer2 = new List<ScriptItemV2>();
            foreach (var item in scriptItems)
            {
                if (string.IsNullOrEmpty(item.Name) && string.IsNullOrEmpty(item.Text))
                    continue;
                int index = siVer2.FindIndex(a => a.Name == item.Name);
                if (index == -1)
                {
                    ScriptItemV2 item2 = new ScriptItemV2();
                    if (!string.IsNullOrWhiteSpace(item.Name))
                    {
                        item2.Name = item.Name;
                        item2.NameTrans = Program.tools.CompareAndReplace(item.Name, Program.tools.names);
                    }
                    ScriptSubItem subItem = new ScriptSubItem();
                    subItem.Text = item.Text;
                    subItem.TransText = item.TextTrans;
                    subItem.ChoiceDataList = new List<ScriptSubItem>(item.ChoiceDataList);
                    int key = subItem.Text.Length;
                    if (item2.TextList.ContainsKey(key))
                    {
                        item2.TextList[key].Add(subItem);
                    }
                    else
                    {
                        item2.TextList.Add(key, new List<ScriptSubItem>() { subItem });
                    }
                    siVer2.Add(item2);
                }
                else
                {
                    ScriptSubItem subItem = new ScriptSubItem();
                    subItem.Text = item.Text;
                    subItem.TransText = item.TextTrans;
                    subItem.ChoiceDataList = new List<ScriptSubItem>(item.ChoiceDataList);
                    int key = subItem.Text.Length;

                    if (siVer2[index].TextList.ContainsKey(key))
                    {
                        siVer2[index].TextList[key].Add(subItem);
                    }
                    else
                    {
                        siVer2[index].TextList.Add(key, new List<ScriptSubItem>() { subItem });
                    }
                }
            }
            File.WriteAllText("DialogueTextsV2.json", JsonConvert.SerializeObject(siVer2, Formatting.Indented));

        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            var scriptItems = JsonConvert.DeserializeObject<List<ScriptItem>>(File.ReadAllText(openFileDialog.FileName));
            StringBuilder indexes = new StringBuilder();
            StringBuilder textData = new StringBuilder();
            StringBuilder choiceIndexes = new StringBuilder();
            StringBuilder choiceData = new StringBuilder();
            int fileIndex = 0;
            int choiceFileIndex = 0;
            for (int i = 0; i < scriptItems.Count; i++)
            {
                if (string.IsNullOrEmpty(scriptItems[i].Text))
                    continue;
                if (string.IsNullOrEmpty(scriptItems[i].TextTrans))
                {
                    indexes.AppendLine(i.ToString());
                    textData.AppendLine(scriptItems[i].Text.Replace("\r", "").Replace("\n", ""));
                    textData.AppendLine();
                    textData.AppendLine();
                }

                if (scriptItems[i].ChoiceDataList.Count > 0)
                {
                    for (int j = 0; j < scriptItems[i].ChoiceDataList.Count; j++)
                    {
                        if (string.IsNullOrWhiteSpace(scriptItems[i].ChoiceDataList[j].TransText))
                        {
                            choiceIndexes.AppendLine($"{i}:{j}");

                            choiceData.AppendLine(scriptItems[i].ChoiceDataList[j].Text.Replace("\r", "").Replace("\n", ""));
                            choiceData.AppendLine();
                            choiceData.AppendLine();
                        }
                    }
                }

                if (textData.Length > 95000)
                {
                    File.WriteAllText("indexFile_" + fileIndex + ".txt", indexes.ToString());
                    //File.WriteAllText("TextFile_" + fileIndex + ".txt", textData.ToString());
                    SaveDataToDoc(textData.ToString(), "TextFile_" + fileIndex + ".docx");
                    fileIndex++;
                    indexes.Clear();
                    textData.Clear();
                }

                if (choiceData.Length > 95000)
                {
                    File.WriteAllText("choiceIndexFile_" + choiceFileIndex + ".txt", choiceIndexes.ToString());
                    //File.WriteAllText("TextFile_" + fileIndex + ".txt", textData.ToString());
                    SaveDataToDoc(choiceData.ToString(), "choiceTextFile_" + choiceFileIndex + ".docx");
                    choiceFileIndex++;
                    choiceIndexes.Clear();
                    choiceData.Clear();
                }

            }
            File.WriteAllText("indexFile_" + fileIndex + ".txt", indexes.ToString());
            SaveDataToDoc(textData.ToString(), "TextFile_" + fileIndex + ".docx");

            File.WriteAllText("choiceIndexFile_" + choiceFileIndex + ".txt", choiceIndexes.ToString());
            SaveDataToDoc(choiceData.ToString(), "choiceTextFile_" + choiceFileIndex + ".docx");
        }

        private void SaveDataToDoc(string data, string outPath)
        {
            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            //app.ShowAnimation = false;
            app.Visible = false;
            object missing = System.Reflection.Missing.Value;

            string source = Path.Combine(Application.StartupPath, outPath);
            Microsoft.Office.Interop.Word.Document document = app.Documents.Add();
            //doc.Activate();
            Microsoft.Office.Interop.Word.Paragraph paragraph = document.Content.Paragraphs.Add();
            paragraph.Range.Text = data;
            paragraph.Range.InsertParagraphAfter();
            document.SaveAs2(source);
            document.Close();
            releaseObject(document);
            app.Quit();
            releaseObject(app);
        }

        private string GetTextFromDoc(string docFilePath)
        {
            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            //app.ShowAnimation = false;
            app.Visible = false;
            object missing = System.Reflection.Missing.Value;

            string source = Path.Combine(Application.StartupPath, docFilePath);
            Microsoft.Office.Interop.Word.Document document = app.Documents.Open(source);

            string text = document.Content.Text;

            document.Close();
            releaseObject(document);
            app.Quit();
            releaseObject(app);

            return text;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                GC.Collect();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            var scriptItems = JsonConvert.DeserializeObject<List<ScriptItem>>(File.ReadAllText(openFileDialog.FileName));
            var indexFiles = Directory.GetFiles(openFolderDialog.Folder, "indexFile_*", SearchOption.TopDirectoryOnly);
            var choiceIndexFiles = Directory.GetFiles(openFolderDialog.Folder, "choiceIndexFile_*", SearchOption.TopDirectoryOnly);
            string indexFilePattern = ".*ndexFile_(\\d+)\\.txt";

            foreach (var indexFile in indexFiles)
            {
                var m = Regex.Match(indexFile, indexFilePattern);
                if (!m.Success)
                    throw new Exception("Ошибка чтения имени файла!");
                string docFile = Path.Combine(Path.GetDirectoryName(indexFile), $"TextFile_{m.Groups[1].Value} ru.docx");

                string textData = GetTextFromDoc(docFile);
                var indexes = GetIndexesFromFile(indexFile);
                var clearTextData = GetClearTextData(textData);
                if (clearTextData.Count != indexes.Count)
                {
                    throw new Exception("Рассинхронизация индексов и текста! Файл: " + Path.GetFileNameWithoutExtension(indexFile));
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    scriptItems[indexes[i]].TextTrans = clearTextData[i];
                }
            }

            foreach (var indexFile in choiceIndexFiles)
            {
                var m = Regex.Match(indexFile, indexFilePattern);
                if (!m.Success)
                    throw new Exception("Ошибка чтения имени файла!");
                string docFile = Path.Combine(Path.GetDirectoryName(indexFile), $"choiceTextFile_{m.Groups[1].Value} ru.docx");

                string textData = GetTextFromDoc(docFile);
                var indexes = GetIndexesFromChoiceFile(indexFile);
                var clearTextData = GetClearTextData(textData);
                if (clearTextData.Count != indexes.Count)
                {
                    throw new Exception("Рассинхронизация индексов и текста! Файл: " + Path.GetFileNameWithoutExtension(indexFile));
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    scriptItems[indexes[i].Key].ChoiceDataList[indexes[i].Value].TransText = clearTextData[i];
                }
            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_filled.json"),
                JsonConvert.SerializeObject(scriptItems, Formatting.Indented));
        }

        private List<string> GetClearTextData(string text)
        {
            char[] lineSplitters = { '\r', '\n' };

            string[] textList = text.Split(lineSplitters, StringSplitOptions.None);
            List<string> clearTextData = new List<string>();

            for (int i = 1; i < textList.Length; i = i + 3)
            {
                clearTextData.Add(textList[i]);
            }
            if (string.IsNullOrEmpty(clearTextData[clearTextData.Count - 1]))
                clearTextData.RemoveAt(clearTextData.Count - 1);
            return clearTextData;
        }

        private List<int> GetIndexesFromFile(string filePath)
        {
            string[] textIndexes = File.ReadAllLines(filePath);
            List<int> indexes = new List<int>();
            foreach (var item in textIndexes)
            {
                if (!string.IsNullOrEmpty(item))
                    indexes.Add(Convert.ToInt32(item));
            }
            return indexes;
        }

        private List<KeyValuePair<int,int>> GetIndexesFromChoiceFile(string filePath)
        {
            char[] splitChar = { ':' };
            string[] textIndexes = File.ReadAllLines(filePath);
            List<KeyValuePair<int, int>> indexes = new List<KeyValuePair<int, int>>();
            foreach (var item in textIndexes)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string[] data = item.Split(splitChar);
                    indexes.Add(new KeyValuePair<int, int>(Convert.ToInt32(data[0]), Convert.ToInt32(data[1])));
                }
            }
            return indexes;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            var scriptItems = JsonConvert.DeserializeObject<List<ScriptItem>>(File.ReadAllText(openFileDialog.FileName));

            List<string> names = new List<string>();
            List<string> clearNames = new List<string>();
            foreach (var item in scriptItems)
            {
                if (!clearNames.Contains(item.Name))
                {
                    clearNames.Add(item.Name);
                    string transName = Program.tools.CompareAndReplace(item.Name, Program.tools.RepText);
                    if (transName == item.Name)
                    {
                        names.Add(item.Name + "=" + Program.tools.TranslateText(item.Name, "ru"));
                    }
                    else
                    {
                        names.Add(item.Name + "=" + transName);
                    }

                }
            }
            File.WriteAllLines("names.txt", names);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var scriptItems = JsonConvert.DeserializeObject<List<ScriptItem2>>(File.ReadAllText(openFileDialog.FileName));

            List<ScriptItemV2> nItems = new List<ScriptItemV2>();

            foreach (var item in scriptItems)
            {
                ScriptItemV2 sItem = new ScriptItemV2();

                sItem.Name = item.Name;
                sItem.NameTrans = item.NameTrans;
                foreach (var textItem in item.TextList)
                {
                    int key = textItem.Text.Length;
                    if (sItem.TextList.ContainsKey(key))
                    {
                        sItem.TextList[key].Add(textItem);
                    }
                    else
                    {
                        sItem.TextList.Add(key, new List<ScriptSubItem>() { textItem });
                    }
                }
                nItems.Add(sItem);

            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_V2.json"),
    JsonConvert.SerializeObject(nItems, Formatting.Indented));
        }

        private void button10_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            ExtractAndAppendTextFromScriptsAsync(openFileDialog.FileName, openFolderDialog.Folder);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var scriptItems = JsonConvert.DeserializeObject<List<ScriptItem>>(File.ReadAllText(openFileDialog.FileName));
            int maxCount = 400000;
            StringBuilder sb = new StringBuilder();
            int lineLen = 0;
            for (int i = 0; i < scriptItems.Count && i < maxCount; i++)
            {
                string text = scriptItems[i].Text.Replace("\r", "").Replace("\n", "");
                sb.Append(text);
                lineLen += text.Length;
                if (lineLen > 380)
                {
                    sb.Append("\n");
                    lineLen = 0;
                }
            }
            File.WriteAllText("train.txt", sb.ToString());


        }

        private void button12_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            string p = "chr_icon_(\\d+).png";
            CopyFilesWithPattern(openFolderDialog.Folder, openFolderDialog.Folder, p, NotConvertName);

        }

        private void CopyFilesWithPattern(string srcDir, string dstDir, string pattern, Func<string, string> convNameFunc)
        {
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            string[] files = Directory.GetFiles(srcDir, "*.png", SearchOption.AllDirectories);
            foreach (var item in files)
            {
                string fName = Path.GetFileName(item);
                Match m = r.Match(fName);
                if (m.Success)
                {
                    string nFilePath = Path.Combine(dstDir, convNameFunc(m.Groups[1].Value) + ".png");
                    if (!File.Exists(nFilePath))
                        File.Copy(item, nFilePath);
                }
            }
        }

        private string NotConvertName(string name)
        {
            return name;
        }

        private string ConvToInt(string name)
        {
            int res = -1;
            if (int.TryParse(name, out res))
                return res.ToString();
            else
                return name;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            string p = "item_icon_(.*).png";
            CopyFilesWithPattern(openFolderDialog.Folder, openFolderDialog.Folder, p, ConvToInt);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            string p = "thum_race_rt_\\d+_(\\d+)_\\d+.png";
            CopyFilesWithPattern(openFolderDialog.Folder, openFolderDialog.Folder, p, NotConvertName);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            string p = "support_card_s_(\\d+).png";
            CopyFilesWithPattern(openFolderDialog.Folder, openFolderDialog.Folder, p, NotConvertName);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            string p = "honor_(\\d+).png";
            CopyFilesWithPattern(openFolderDialog.Folder, openFolderDialog.Folder, p, NotConvertName);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            string p = "piece_icon_(\\d+).png";
            CopyFilesWithPattern(openFolderDialog.Folder, openFolderDialog.Folder, p, NotConvertName);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
                return;
            string p = "utx_ico_skill_(\\d+).png";
            CopyFilesWithPattern(openFolderDialog.Folder, openFolderDialog.Folder, p, NotConvertName);
        }
    }
}
