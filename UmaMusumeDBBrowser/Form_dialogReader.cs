using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UmaMusumeDBBrowser
{
    public partial class Form_dialogReader : Form
    {
        private Point mouseOffset;
        private bool isMouseDown = false;

        public Form_dialogReader()
        {
            InitializeComponent();
        }

        private DialogsManages.DialogsItemDataV1 currentDialogData;
        private bool showOriginal = false;

        private void Form_dialogReader_Load(object sender, EventArgs e)
        {

        }

        public void SetDialogText(DialogsManages.DialogsItemDataV1 dialogData)
        {
            currentDialogData = dialogData;
            SetTextToControls();
        }

        private void SetTextToControls()
        {
            if (currentDialogData == null)
                return;
            if (!showOriginal)
            {
                Extensions.SetTextToControl(textBox3, currentDialogData.NameTrans);
                Extensions.SetTextToControl(textBox4, currentDialogData.TextTrans);
            }
            else
            {
                Extensions.SetTextToControl(textBox3, currentDialogData.Name);
                Extensions.SetTextToControl(textBox4, currentDialogData.Text);
            }

            if (currentDialogData.ChoiceDataList.Count > 0)
            {
                Extensions.SetControlVisible(listBox1, true);
                Extensions.ClearListBox(listBox1);
                foreach (var item in currentDialogData.ChoiceDataList)
                {
                    Extensions.AddTextToList(listBox1, showOriginal ? item.Text : item.TransText);
                }
                Extensions.SetControlHeight(listBox1, currentDialogData.ChoiceDataList.Count * 22);
            }
            else
            {
                if (listBox1.Visible)
                {
                    Extensions.SetControlVisible(listBox1, false);
                    Extensions.ClearListBox(listBox1);
                }
            }
        }

        private void Form_dialogReader_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;

            if (e.Button == MouseButtons.Left)
            {
                xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
                yOffset = -e.Y - SystemInformation.CaptionHeight -
                    SystemInformation.FrameBorderSize.Height;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }

        private void Form_dialogReader_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }

        private void Form_dialogReader_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = !toolStrip1.Visible;
            if (toolStrip1.Visible)
                button1.ImageIndex = 0;
            else
                button1.ImageIndex = 1;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            showOriginal = !showOriginal;
            SetTextToControls();
        }
    }
}
