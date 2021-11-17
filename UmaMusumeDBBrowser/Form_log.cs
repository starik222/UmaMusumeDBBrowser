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
    public partial class Form_log : Form
    {
        public Form_log()
        {
            InitializeComponent();
            logData = new StringBuilder();
            locker = new object();
        }
        delegate void LogHandler(string text);
        private StringBuilder logData;
        private int strIndex = 1;
        private bool isAutoScroll = true;
        private bool isPaused = false;
        private object locker;
        private void Form_log_Load(object sender, EventArgs e)
        {

        }
        public void AddToLog(string text)
        {
            lock (locker)
            {
                if (!isPaused)
                {
                    logData.AppendLine(strIndex + ": " + text);
                    strIndex++;
                    SetText(logData.ToString());
                }
            }
        }

        public void SaveLog(string file)
        {
            lock (locker)
            {
                File.WriteAllText(file, logData.ToString());
            }
        }

        public void SetText(string text = "")
        {
            if (this.textBox1.InvokeRequired)
            {
                LogHandler d = new LogHandler(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (isAutoScroll)
            {
                if (textBox1.Text.Length > 0)
                {
                    textBox1.SelectionStart = textBox1.Text.Length - 1;
                    textBox1.SelectionLength = 0;
                    textBox1.ScrollToCaret();
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            isAutoScroll = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ClearLog();
        }

        public void ClearLog()
        {
            lock (locker)
            {
                logData.Clear();
                strIndex = 1;
                SetText();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            isPaused = checkBox2.Checked;
        }
    }
}
