using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UmaMusumeDBBrowser
{
    public class EventControlManager
    {
        public TextBox EventName { get; set; }
        public List<TextBox> Options { get; set; }
        public List<CustomRichTextBox> Effects { get; set; }

        private List<int> richTextBoxesHeight;

        private Color[] optionsColor;
        private bool isNeedChangeBackColor = true;

        public EventControlManager(TextBox eventNameTextBox)
        {
            EventName = eventNameTextBox;
            Options = new List<TextBox>();
            Effects = new List<CustomRichTextBox>();
            richTextBoxesHeight = new List<int>();

        }

        public void AddOption(TextBox textBox)
        {
            textBox.BackColorChanged += TextBox_BackColorChanged;
            Options.Add(textBox);
        }

        private void TextBox_BackColorChanged(object sender, EventArgs e)
        {
            if (isNeedChangeBackColor)
                SetOptionsColorInternal();
        }

        public void SetOptionsColor(Color[] colors)
        {
            optionsColor = colors;
            SetOptionsColorInternal();
        }

        public void SetOptionsColorInternal()
        {
            isNeedChangeBackColor = false;
            for (int i = 0; i < Options.Count && i < optionsColor.Length; i++)
            {
                Options[i].BackColor = optionsColor[i];
                Options[i].ForeColor = SystemColors.ControlText;
            }
            isNeedChangeBackColor = true;
        }

        public void AddEffect(CustomRichTextBox richTextBox)
        {
            Effects.Add(richTextBox);
            richTextBoxesHeight.Add(richTextBox.Height);
        }

        public void SetText(int OptionIndex, string optionText, string effectText)
        {
            Extensions.SetTextToControl(Options[OptionIndex], optionText);
            Extensions.SetTextToControl(Effects[OptionIndex], effectText);
        }

        public void SetEventName(string text)
        {
            Extensions.SetTextToControl(EventName, text);
        }

        public void SetVisibleFirst(int count)
        {
            bool v = false;
            int h = 0;
            for (int i = 0; i < Options.Count; i++)
            {
                if (i < count)
                {
                    v = true;
                    h = richTextBoxesHeight[i];
                }
                else
                {
                    v = false;
                    h = 0;
                }
                Extensions.SetControlVisible(Options[i], v);
                Extensions.SetControlVisible(Effects[i], v);
                //SetControlHeight(Effects[i], h);
            }

        }





    }
}
