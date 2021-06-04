using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.IO;
using System.Drawing.Imaging;
using Emgu.CV.CvEnum;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Translator;

namespace UmaMusumeDBBrowser
{
    public partial class Form_GameRecognizer : Form
    {

        private List<NAMES> skillTransDictonary;
        private Color[] optionColors;
        private delegate void TabDelegate(TabPage tabPage);
        private UmaMusumeLibrary library;
        private GameSettings settings;
        private GameReader gameReader;
        public Form1 parentForm = null;
        private EventControlManager controlManager;
        private SkillControlManager skillControlManager;
        private SkillManager skillManager;
        private string skillIconPath = null;

        public Form_GameRecognizer()
        {
            InitializeComponent();
            library = new UmaMusumeLibrary();
            library.LoadLibrary(Path.Combine(Program.DictonariesDir, "EventLibrary.json"));
            skillManager = new SkillManager();
            skillManager.FillData();
            settings = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText(Path.Combine(Program.DictonariesDir, "GameParams.json")));
            LoadImagesToSettings();
            var repDict = Program.TransDict.LoadDictonary(Path.Combine(Program.DictonariesDir, "ReplaceChars.txt"));
            gameReader = new GameReader(GameReader.GameType.DMM, library, skillManager, settings, repDict);
            gameReader.DataChanged += GameReader_DataChanged;
            //comboBox1.Items.AddRange(library.GetCardNameList());
        }



        private void SelectTab(TabPage tabPage)
        {
            if (tabControl1.InvokeRequired)
            {
                var d = new TabDelegate(SelectTab);
                tabControl1.Invoke(d, new object[] { tabPage });
            }
            else
                tabControl1.SelectTab(tabPage);
        }

        private void GameReader_DataChanged(object sender, GameReader.GameDataArgs gameDataArgs)
        {
            switch (gameDataArgs.DataType)
            {
                case GameReader.GameDataType.GameNotFound:
                    {
                        Extensions.SetTextToControl(label1, (string)gameDataArgs.DataClass);
                        IsReaderStarted(false);
                        SelectTab(tabPage1);
                        break;
                    }
                case GameReader.GameDataType.NotFound:
                    {
                        if (Program.IsDebug)
                            Extensions.SetTextToControl(label1, (string)gameDataArgs.DataClass);
                        else
                            Extensions.SetTextToControl(label1, "");
                        break;
                    }
                case GameReader.GameDataType.MainTraining:
                    {
                        Extensions.SetTextToControl(label1, (string)gameDataArgs.DataClass);
                        break;
                    }
                case GameReader.GameDataType.TrainingEvent:
                    {
                        SelectTab(tabPage2);
                        if (gameDataArgs.DataClass != null)
                        {
                            Extensions.SetTextToControl(label1, "Event found: " + ((UmaMusumeLibrary.EventData)gameDataArgs.DataClass).EventName);
                            ShowEventData((UmaMusumeLibrary.EventData)gameDataArgs.DataClass);
                        }
                        else
                        {
                            Extensions.SetTextToControl(label1, "EVENT NOT FOUND!");
                        }
                        break;
                    }
                case GameReader.GameDataType.DebugImage:
                    {
                        pictureBox1.Image = (Bitmap)gameDataArgs.DataClass;
                        break;
                    }
                case GameReader.GameDataType.UmaSkillList:
                    {
                        SelectTab(tabPage4);
                        SetSkills((List<SkillManager.SkillData>)gameDataArgs.DataClass);
                        Extensions.SetTextToControl(label1, "Skill list");
                        break;
                    }
            }
            //throw new NotImplementedException();
        }


        private void Form_GameRecognizer_Load(object sender, EventArgs e)
        {
            if (!Program.IsDebug)
                tabControl1.TabPages.Remove(tabPage3);
            optionColors = new Color[5];
            optionColors[0] = Color.FromArgb(0xd5, 0xfb, 0xa4);
            optionColors[1] = Color.FromArgb(0xff, 0xf6, 0xb0);
            optionColors[2] = Color.FromArgb(0xff, 0xcd, 0xe5);
            optionColors[3] = Color.FromArgb(0xb5, 0xe7, 0xff);
            optionColors[4] = Color.FromArgb(0xce, 0xcb, 0xff);
            CreateRichTextBoxes();
            controlManager.SetVisibleFirst(0);
            CreateSkillControlManager();
            var skillSettings = Program.TableDisplaySettings.Find(a => a.TableName.Equals("skill_data"));
            if (skillSettings != null)
            {
                if (skillSettings.IconSettings.Count > 0)
                    skillIconPath = skillSettings.IconSettings[0].Value[0];
            }
            skillTransDictonary = Program.TransDict.LoadDictonary(Path.Combine(Program.DictonariesDir, "skill_data" + "_" + parentForm.toolStripComboBox1.SelectedItem + ".txt"));

        }

        private void SetSkills(List<SkillManager.SkillData> datas)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < datas.Count)
                {
                    Image img = Program.IconDB.GetImageByKey(skillIconPath, datas[i].IconId.ToString());
                    if (checkBox1.Checked)
                    {
                        string transName = Program.TransDict.GetTranslation(skillTransDictonary, datas[i].Name);
                        if (string.IsNullOrEmpty(transName))
                            transName = string.Empty;
                        string transDesc = Program.TransDict.GetTranslation(skillTransDictonary, datas[i].Desc);
                        if (string.IsNullOrEmpty(transDesc))
                            transDesc = datas[i].Desc;
                        skillControlManager.SetText(i, datas[i].Id, datas[i].Name, transName, transDesc, img);
                    }
                    else
                        skillControlManager.SetText(i, datas[i].Id, datas[i].Name, "", datas[i].Desc, img);
                }
                else
                    skillControlManager.SetText(i, -1, "", "", "", null);

            }
        }

        private void CreateSkillControlManager()
        {
            skillControlManager = new SkillControlManager();
            skillControlManager.AddItem(textBox7, textBox8, textBox9, pictureBox2);
            skillControlManager.AddItem(textBox10, textBox11, textBox12, pictureBox3);
            skillControlManager.AddItem(textBox13, textBox14, textBox15, pictureBox4);
            skillControlManager.AddItem(textBox16, textBox17, textBox18, pictureBox5);
        }


        private void CreateRichTextBoxes()
        {
            controlManager = new EventControlManager(textBox1);
            textBox1.BackColor = Color.White;
            controlManager.AddOption(textBox2);
            controlManager.AddOption(textBox3);
            controlManager.AddOption(textBox4);
            controlManager.AddOption(textBox5);
            controlManager.AddOption(textBox6);
            Font rcFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 1);
            for (int i = 0; i < 5; i++)
            {
                controlManager.Options[i].BackColor = optionColors[i];
                CustomRichTextBox customRichTextBox = new CustomRichTextBox();
                customRichTextBox.Font = rcFont;
                customRichTextBox.LinkClicked += CustomRichTextBox_LinkClicked;
                customRichTextBox.Size = new Size(1, 100);
                customRichTextBox.SetSelectionLineSpacing(4, 256);
                customRichTextBox.Dock = DockStyle.Fill;
                customRichTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
                customRichTextBox.ReadOnly = true;
                tableLayoutPanel1.Controls.Add(customRichTextBox, 1, i);
                controlManager.AddEffect(customRichTextBox);
            }

        }

        private void ShowEventData(UmaMusumeLibrary.EventData eventData)
        {
            controlManager.SetVisibleFirst(eventData.EventOptionsList.Count);
            controlManager.SetEventName(eventData.EventName);
            for (int i = 0; i < eventData.EventOptionsList.Count; i++)
            {
                controlManager.SetText(i, eventData.EventOptionsList[i].Option, eventData.EventOptionsList[i].Effect.Replace('◯', '○'));
            }
        }


        private void CustomRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            parentForm.LoadTableByText(e.LinkText);
            if (parentForm.WindowState == FormWindowState.Minimized)
                parentForm.WindowState = FormWindowState.Normal;
            parentForm.Activate();
            ((CustomRichTextBox)sender).Select(0, 0);
        }

        private void LoadImagesToSettings()
        {
            foreach (var item in settings.GameParts)
            {
                LoadImagesToGamePart(item);
            }
        }

        private void LoadImagesToGamePart(GameSettings.GamePart gamePart)
        {
            if (!string.IsNullOrWhiteSpace(gamePart.ImageName))
            {
                gamePart.Image = (new Image<Bgr, byte>(Path.Combine(Application.StartupPath, "Images//" + gamePart.ImageName))).Convert<Gray, byte>();
            }
            if (gamePart.SubGameParts != null && gamePart.SubGameParts.Count > 0)
            {
                foreach (var item in gamePart.SubGameParts)
                    LoadImagesToGamePart(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {

                if (!gameReader.SetWindowInfo(GameReader.GameType.DMM, IntPtr.Zero))
                {
                    goto gameNotFound;
                }
            }
            else
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Сначала необходимо выбрать процесс из списка.");
                    return;
                }
                IntPtr handle = (IntPtr)dataGridView1.SelectedRows[0].Cells["pHandle"].Value;
                if (radioButton2.Checked)
                {
                    if (!gameReader.SetWindowInfo(GameReader.GameType.BluestacksV4, handle))
                        goto gameNotFound;
                }
                else if (radioButton3.Checked)
                {
                    if(!gameReader.SetWindowInfo(GameReader.GameType.BluestacksV5, handle))
                        goto gameNotFound;
                }
            }
            gameReader.StartAsync(/*(string)comboBox1.SelectedItem,*/ (int)numericUpDown1.Value * 1000);
            IsReaderStarted(true);
            return;
        gameNotFound:
            MessageBox.Show("Игровое окно не найдено!");
        }
        private void IsReaderStarted(bool started)
        {
            Extensions.SetControlEnable(button1, !started);
            Extensions.SetControlEnable(button3, started);
            Extensions.SetControlEnable(numericUpDown1, !started);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (gameReader.IsStarted)
            {
                gameReader.Stop();
                IsReaderStarted(gameReader.IsStarted);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<Int64> ids = new List<Int64>();
            for (int i = 0; i < skillControlManager.SkillId.Count; i++)
            {
                if (skillControlManager.SkillId[i] != -1)
                    ids.Add(skillControlManager.SkillId[i]);
            }
            parentForm.LoadTableByIds("skill_data", ids);
            if (parentForm.WindowState == FormWindowState.Minimized)
                parentForm.WindowState = FormWindowState.Normal;
            parentForm.Activate();
        }

        private void Form_GameRecognizer_FormClosing(object sender, FormClosingEventArgs e)
        {
            parentForm.toolStripComboBox1.Enabled = true;
            if (gameReader.IsStarted)
                gameReader.Stop();
        }

        private void dataGridView1_VisibleChanged(object sender, EventArgs e)
        {
            if (dataGridView1.Visible)
            {
                UpdateProcessList();
            }
        }

        private void UpdateProcessList()
        {
            dataGridView1.DataSource = WindowManager.GetProcessesTable();
            dataGridView1.Columns["pHandle"].Visible = false;
            dataGridView1.Columns["pIcon"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            dataGridView1.Columns["ProcessName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["WindowTitle"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.ReadOnly = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                VisibleProcessControls(true);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                VisibleProcessControls(true);
        }

        private void VisibleProcessControls(bool v)
        {
            dataGridView1.Visible = v;
            label5.Visible = v;
            button2.Visible = v;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                VisibleProcessControls(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateProcessList();
        }
    }
}
