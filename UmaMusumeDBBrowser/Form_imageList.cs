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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace UmaMusumeDBBrowser
{
    public partial class Form_imageList : Form
    {
        ImageList imageList;
        public Form_imageList(string appDir, List<string> dirs)
        {
            InitializeComponent();
            imageList = new ImageList();
            List<FileInfo> fi = new List<FileInfo>();
            foreach (var item in dirs)
            {
                fi.AddRange(new DirectoryInfo(Path.Combine(appDir, item)).GetFiles("*.png").ToList());
            }
            bool needCalcSize = true;
            imageList.ColorDepth = ColorDepth.Depth24Bit;
            foreach (var item in fi)
            {
                var img = Image.FromFile(item.FullName);
                if (needCalcSize)
                {
                    int w = img.Width;
                    int h = img.Height;
                    double scale = w / h;
                    int n_w = 100;
                    int n_h = 100;
                    if (scale > 1)
                        n_w = (int)(n_h * scale);
                    else if (scale < 1)
                        n_h = (int)(n_w * (double)(h / w));
                    if (n_w > 256)
                        n_w = 256;
                    if (n_h > 256)
                        n_h = 256;
                    imageList.ImageSize = new Size(n_w, n_h);
                    needCalcSize = false;
                }
                imageList.Images.Add(Path.GetFileNameWithoutExtension(item.FullName), img);
            }
            listView1.View = View.LargeIcon;
            listView1.LargeImageList = imageList;
            //imageList.ColorDepth = ColorDepth.Depth32Bit;
            for (int i = 0; i < imageList.Images.Count; i++)
            {
                ListViewItem item = new ListViewItem();
                item.ImageKey = imageList.Images.Keys[i];
                this.listView1.Items.Add(item);
            }

            SetColorScheme();
            Program.ColorManager.SchemeChanded += ColorManager_SchemeChanded;
        }


        private void ColorManager_SchemeChanded(object sender, EventArgs e)
        {
            SetColorScheme();
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


        //private Image ResizeImage(Image image) 
        //{
        //    Rectangle imgRect = new Rectangle(Point.Empty, imageList.ImageSize);
        //    double scl = image.Width / image.Height;
        //    if (scl > 1.0)
        //    {
        //        imgRect.Height = (int)(imgRect.Width / scl);
        //        imgRect.Y = (int)((imageList.ImageSize.Width - imgRect.Height) / 2);
        //    }
        //    else if(scl< 1.0)
        //    {
        //        imgRect.Width = (int)(imgRect.Height * scl);
        //        imgRect.X = (int)((imageList.ImageSize.Width - imgRect.Width) / 2);
        //    }
        //    Bitmap img = new Bitmap(imageList.ImageSize.Width, imageList.ImageSize.Height, image.PixelFormat);
        //    using (Graphics graphics = Graphics.FromImage(img))
        //    {
        //        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        graphics.DrawImage(image, imgRect);
        //    }
        //    return img;
        //}


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

        private void Form_imageList_FormClosing(object sender, FormClosingEventArgs e)
        {
            imageList.Dispose();
        }
    }
}
