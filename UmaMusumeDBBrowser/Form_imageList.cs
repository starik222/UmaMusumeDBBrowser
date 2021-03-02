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
    public partial class Form_imageList : Form
    {
        private ImageList imageList;
        public Form_imageList(string appDir, List<string> dirs)
        {
            InitializeComponent();
            imageList = new ImageList();
            List<FileInfo> fi = new List<FileInfo>();
            foreach (var item in dirs)
            {
                fi.AddRange(new DirectoryInfo(Path.Combine(appDir, item)).GetFiles("*.png").ToList());
            }
            foreach (var item in fi)
            {
                imageList.Images.Add(Path.GetFileNameWithoutExtension(item.FullName), Image.FromFile(item.FullName));
            }
            listView1.View = View.LargeIcon;
            imageList.ImageSize = new Size(100, 100);
            listView1.LargeImageList = imageList;
            for (int i = 0; i < imageList.Images.Count; i++)
            {
                ListViewItem item = new ListViewItem();
                item.ImageKey = imageList.Images.Keys[i];
                this.listView1.Items.Add(item);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            SelectOK();
        }

        private void Form_imageList_Load(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            SelectOK();
        }

        private void SelectOK()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного изображения.");
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
