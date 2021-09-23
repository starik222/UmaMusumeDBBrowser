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
using Translator.IO;

namespace UmaMusumeDBBrowser
{
    public partial class Form_GameRecognizer : Form
    {

        private List<NAMES> skillTransDictonary;
        private Color[] optionColors;
        private delegate void TabDelegate(TabPage tabPage);
        private AllLibraryManager libManager;
        private GameSettings settings;
        private GameReader gameReader;
        public Form1 parentForm = null;
        private EventControlManager controlManager;
        private SkillControlManager skillControlManager;
        private string skillIconPath = null;
        private List<string> itemIconPathes = null;

        public Form_GameRecognizer(string lang)
        {
            InitializeComponent();
            libManager = new AllLibraryManager();
            libManager.FactorLibrary.FillTransDict(Path.Combine(Program.DictonariesDir, "succession_factor" + "_" + lang + ".txt"));
            libManager.MissionLibrary.FillTransDict(Path.Combine(Program.DictonariesDir, "mission_data" + "_" + lang + ".txt"));
            libManager.FillData();
            settings = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText(Path.Combine(Program.DictonariesDir, "GameParams.json")));
            LoadImagesToSettings();
            var repDict = Program.TransDict.LoadDictonary(Path.Combine(Program.DictonariesDir, "ReplaceChars.txt"));
            gameReader = new GameReader(GameReader.GameType.DMM, libManager, settings, repDict);
            gameReader.DataChanged += GameReader_DataChanged;
            SetColorScheme();
            //tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            Program.ColorManager.SchemeChanded += ColorManager_SchemeChanded;
           
        }

        private void SetColorScheme()
        {
            if (Program.ColorManager.SelectedScheme != null)
            {
                BackColor = Program.ColorManager.SelectedScheme.FormStyle.BackColor;
                ForeColor = Program.ColorManager.SelectedScheme.FormStyle.ForeColor;
                Program.ColorManager.ChangeColorSchemeInConteiner(Controls, Program.ColorManager.SelectedScheme);
            }
        }

        private void ColorManager_SchemeChanded(object sender, EventArgs e)
        {
            SetColorScheme();
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
                            Extensions.SetTextToControl(label1, "Event found: " + ((EventManager.EventData)gameDataArgs.DataClass).EventName);
                            ShowEventData((EventManager.EventData)gameDataArgs.DataClass);
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
                case GameReader.GameDataType.GenWindow:
                    {
                        SelectTab(tabPage6);
                        SetFactors((List<FactorManager.FactorData>)gameDataArgs.DataClass);
                        Extensions.SetTextToControl(label1, "Щелкните дважды на ячейку для открытия описания умения (если оно есть в тексте).");
                        break;
                    }
                case GameReader.GameDataType.MissionBtn:
                    {
                        SelectTab(tabPage7);
                        SetMissions((List<MissionManager.MissionData>)gameDataArgs.DataClass);
                        Extensions.SetTextToControl(label1, "Mission list");
                        break;
                    }
                case GameReader.GameDataType.TazunaAfterHelp:
                    {
                        SelectTab(tabPage5);
                        var data = (GameReader.TazunaHelpRelult)gameDataArgs.DataClass;
                        if (!string.IsNullOrWhiteSpace(data.Warning))
                            Extensions.SetTextToControl(textBox19, data.Warning);
                        else
                            Extensions.SetTextToControl(textBox19, "");
                        if (!string.IsNullOrWhiteSpace(data.Desc))
                            Extensions.SetTextToControl(textBox20, data.Desc);
                        else
                            Extensions.SetTextToControl(textBox20, "HELP TEXT NOT FOUND IN LIBRARY");
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


            var itemSettings = Program.TableDisplaySettings.Find(a => a.TableName.Equals("item_data"));
            itemIconPathes = new List<string>();
            if (itemSettings != null)
            {

                if (itemSettings.IconSettings.Count > 0)
                {
                    itemIconPathes = new List<string>(itemSettings.IconSettings[0].Value);
                }
            }

            pictureBox6.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Images\\Tazuna.bmp"));

        }

        private void SetFactors(List<FactorManager.FactorData> datas)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("FactorType", typeof(FactorManager.FactorType));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("NameTrans", typeof(string));
            dt.Columns.Add("Desc", typeof(string));
            dt.Columns.Add("DescTrans", typeof(string));

            foreach (var item in datas)
            {
                dt.Rows.Add(item.FactorType, item.Name, item.NameTrans, item.Desc, item.DescTrans);
            }

            Extensions.SetGridDataSource(dataGridView2, dt);
            Extensions.SetGridColumnVisible(dataGridView2, "FactorType", false);
            //Extensions.SetGridColumnVisible(dataGridView2, "NameToCheck", false);
            //Extensions.SetGridColumnVisible(dataGridView2, "FactorType", false);
            Extensions.SetGridColumnSizeMode(dataGridView2, "Name", DataGridViewAutoSizeColumnMode.NotSet);
            Extensions.SetGridColumnSizeMode(dataGridView2, "NameTrans", DataGridViewAutoSizeColumnMode.NotSet);
            Extensions.SetGridColumnSizeMode(dataGridView2, "Desc", DataGridViewAutoSizeColumnMode.Fill);
            Extensions.SetGridColumnSizeMode(dataGridView2, "DescTrans", DataGridViewAutoSizeColumnMode.Fill);

            for (int i = 0; i < dataGridView2.RowCount; i++)
            {
                if ((FactorManager.FactorType)dataGridView2["FactorType", i].Value == FactorManager.FactorType.Characteristics)
                {
                    Extensions.SetGridRowBackColor(dataGridView2, i, Color.FromArgb(0x34, 0xb6, 0xf4));
                    Extensions.SetGridRowForeColor(dataGridView2, i, SystemColors.ControlText);
                }
                else if ((FactorManager.FactorType)dataGridView2["FactorType", i].Value == FactorManager.FactorType.Suitability)
                {
                    Extensions.SetGridRowBackColor(dataGridView2, i, Color.FromArgb(0xff, 0x75, 0xb0));
                    Extensions.SetGridRowForeColor(dataGridView2, i, SystemColors.ControlText);
                }
                else if ((FactorManager.FactorType)dataGridView2["FactorType", i].Value == FactorManager.FactorType.ParentSkill)
                {
                    Extensions.SetGridRowBackColor(dataGridView2, i, Color.FromArgb(0x91, 0xcf, 0x2e));
                    Extensions.SetGridRowForeColor(dataGridView2, i, SystemColors.ControlText);
                }
                else
                {
                    if (Program.ColorManager.SelectedScheme != null) {
                        Extensions.SetGridRowBackColor(dataGridView2, i, Program.ColorManager.SelectedScheme.GrigStyle.BackColor);
                        Extensions.SetGridRowForeColor(dataGridView2, i, Program.ColorManager.SelectedScheme.GrigStyle.ForeColor);
                    }
                }
            }
        }

        private void SetMissions(List<MissionManager.MissionData> datas)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id", typeof(long));
            dt.Columns.Add("Item", typeof(Image));
            dt.Columns.Add("Count", typeof(long));
            dt.Columns.Add("Mission", typeof(string));
            dt.Columns.Add("MissionTrans", typeof(string));

            foreach (var item in datas)
            {
                Image img = null;
                foreach (var iconPath in itemIconPathes)
                {
                    img = Program.IconDB.GetImageByKey(iconPath, item.ItemId.ToString());
                    if (img != null)
                        break;
                }

                dt.Rows.Add(item.Id, img, item.ItemCount, item.MissionText, item.TransMissionText);
            }

            Extensions.SetGridDataSource(dataGridView3, dt);
            Extensions.SetGridColumnVisible(dataGridView3, "Id", false);
            Extensions.SetGridColumnSizeMode(dataGridView3, "Item", DataGridViewAutoSizeColumnMode.AllCells);
            Extensions.SetGridColumnSizeMode(dataGridView3, "Count", DataGridViewAutoSizeColumnMode.NotSet);
            Extensions.SetGridColumnSizeMode(dataGridView3, "Mission", DataGridViewAutoSizeColumnMode.Fill);
            Extensions.SetGridColumnSizeMode(dataGridView3, "MissionTrans", DataGridViewAutoSizeColumnMode.Fill);

            for (int i = 0; i < dataGridView3.RowCount; i++)
            {
                if (Program.ColorManager.SelectedScheme != null)
                {
                    Extensions.SetGridRowBackColor(dataGridView3, i, Program.ColorManager.SelectedScheme.GrigStyle.BackColor);
                    Extensions.SetGridRowForeColor(dataGridView3, i, Program.ColorManager.SelectedScheme.GrigStyle.ForeColor);
                }
            }
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
            //textBox1.BackColor = Color.White;
            controlManager.AddOption(textBox2);
            controlManager.AddOption(textBox3);
            controlManager.AddOption(textBox4);
            controlManager.AddOption(textBox5);
            controlManager.AddOption(textBox6);
            Font rcFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 1);
            controlManager.SetOptionsColor(optionColors);
            for (int i = 0; i < 5; i++)
            {
                //controlManager.Options[i].BackColor = optionColors[i];
                CustomRichTextBox customRichTextBox = new CustomRichTextBox();
                customRichTextBox.Font = rcFont;
                customRichTextBox.LinkClicked += CustomRichTextBox_LinkClicked;
                customRichTextBox.Size = new Size(1, 100);
                customRichTextBox.SetSelectionLineSpacing(4, 256);
                customRichTextBox.Dock = DockStyle.Fill;
                customRichTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
                customRichTextBox.ReadOnly = true;
                if (Program.ColorManager.SelectedScheme != null)
                {
                    customRichTextBox.BackColor = Program.ColorManager.SelectedScheme.TextBoxStyle.BackColor;
                    customRichTextBox.ForeColor = Program.ColorManager.SelectedScheme.TextBoxStyle.ForeColor;
                }
                tableLayoutPanel1.Controls.Add(customRichTextBox, 1, i);
                controlManager.AddEffect(customRichTextBox);
            }

        }

        private void ShowEventData(EventManager.EventData eventData)
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
                gameReader.BsTopPanelVisible = !checkBox2.Checked;
                gameReader.BsRightPanelVisible = !checkBox3.Checked;
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
            if (ids.Count == 0)
                MessageBox.Show("Skill list is empty.");
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
            checkBox2.Visible = v;
            checkBox3.Visible = v;
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

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            
            TabPage page = tabControl1.TabPages[e.Index];
            Color backColor = Color.Transparent;
            Color foreColor = SystemColors.ControlText;
            if (Program.ColorManager.SelectedScheme != null)
            {
                backColor = Program.ColorManager.SelectedScheme.OtherStyle.BackColor;
                foreColor = Program.ColorManager.SelectedScheme.OtherStyle.ForeColor;
            }
            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, Font, paddedBounds, foreColor);
            e.DrawFocusRectangle();
        }

        private void tabPage1_BackColorChanged(object sender, EventArgs e)
        {
            if (tabPage1.BackColor == Color.Transparent || tabPage1.BackColor == SystemColors.Window)
            {
                panelTempFix.Visible = false;
            }
            else
                panelTempFix.Visible = true;
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == -1 || e.RowIndex == -1)
                return;
            if (dataGridView2[e.ColumnIndex, e.RowIndex].Value == DBNull.Value)
                return;
            string text = (string)dataGridView2[e.ColumnIndex, e.RowIndex].Value;

            if (text.Contains('【'))
            {
                text = text.GetBetween("【", "】");
            }
            else if (text.Contains('「'))
            {
                text = text.GetBetween("「", "」");
            }
            else if (text.Contains('『'))
            {
                text = text.GetBetween("『", "』");
            }
            else
                return;
            parentForm.LoadTableByText(text);
            if (parentForm.WindowState == FormWindowState.Minimized)
                parentForm.WindowState = FormWindowState.Normal;
            parentForm.Activate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("DebugImg.bmp", ImageFormat.Bmp);
        }

        private void dataGridView3_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            if (e.Column.ValueType == typeof(Image))
            {
                ((DataGridViewImageColumn)e.Column).ImageLayout = DataGridViewImageCellLayout.Zoom;
                e.Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            List<Int64> ids = new List<Int64>();
            for (int i = 0; i < dataGridView3.RowCount; i++)
            {
                ids.Add((long)dataGridView3["Id", i].Value);
            }
            if (ids.Count == 0)
                MessageBox.Show("mission list is empty.");
            parentForm.LoadTableByIds("mission_data", ids);
            if (parentForm.WindowState == FormWindowState.Minimized)
                parentForm.WindowState = FormWindowState.Normal;
            parentForm.Activate();
        }
    }
}
