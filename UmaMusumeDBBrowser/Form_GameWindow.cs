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
using System.Diagnostics;

namespace UmaMusumeDBBrowser
{
    public partial class Form_GameWindow : Form
    {
        public Form_GameWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //WindowManager screen = new ScreenCapture();
            IntPtr handle = WindowManager.FindWindow("UnityWndClass", "umamusume");
            //IntPtr handle = WindowManager.GetHandleByProcessName("BlueStacks");
            //IntPtr handle = WindowManager.GetHandleByProcessName("кони");
            if (handle == IntPtr.Zero)
            {
                MessageBox.Show("Окно не найдено!");
                return;
            }
            Image img = WindowManager.CaptureWindow(handle);
            var img2 = ImageManager.PrepareImage((Bitmap)img, new Size(588, 1045));
            img2.AsBitmap().Save("img.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            pictureBox1.Image = null;
            pictureBox1.Image = img;
        }




        private void button2_Click(object sender, EventArgs e)
        {
            var prepImg = ImageManager.PrepareImageGray(pictureBox1.Image as Bitmap, new Size(568, 1010));
            prepImg = prepImg.ThresholdBinary(new Gray(111), new Gray(255));
            richTextBox1.Text = Program.TessManager.GetText(prepImg.Mat);
            prepImg.Draw(new Rectangle(55, 431, 427, 286), new Gray(0), 3);
            pictureBox1.Image = prepImg.AsBitmap();
            richTextBox1.Text = richTextBox1.Text.Replace(" ", "");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //pictureBox1.Image = ImageManager.PrepareSkillList((Bitmap)pictureBox1.Image);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            float conf = (float)Convert.ToDecimal(textBox1.Text.Replace(".", ","));
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            Image<Bgr, byte> image = ImageManager.PrepareImage((Bitmap)pictureBox1.Image, new Size(588, 1045));
            Image<Gray, byte> imageGray = ImageManager.PrepareImageGray((Bitmap)pictureBox1.Image, new Size(588, 1045));
            Image<Gray, byte> template = (new Image<Bgr, byte>(openFileDialog.FileName)).Convert<Gray, byte>();
            Mat mat = new Mat();
            if (MessageBox.Show("Выбрать файл с маской?", "выбор маски", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                Mat searchMask = new Mat(openFileDialog.FileName, ImreadModes.Grayscale);
                //CvInvoke.BitwiseNot(searchMask, searchMask);
                CvInvoke.Threshold(searchMask, searchMask, 128, 255, ThresholdType.Binary);
                CvInvoke.MatchTemplate(imageGray, template, mat, TemplateMatchingType.CcoeffNormed, searchMask);
            }
            else
            {
                CvInvoke.MatchTemplate(imageGray, template, mat, TemplateMatchingType.CcoeffNormed);
            }
            
            //CvInvoke.MatchTemplate(imageGray, template, mat, TemplateMatchingType.CcoeffNormed);
            //CvInvoke.Normalize(mat, mat, 0, 1, NormType.MinMax, DepthType.Default);
            double minVal=0, maxVal=0;
            Point minLoc = new Point(), maxLoc = new Point();
            Mat general_mask = Mat.Ones(mat.Rows, mat.Cols, DepthType.Cv8U, 1);
            
            while (true)
            {
                CvInvoke.MinMaxLoc(mat, ref minVal, ref maxVal, ref minLoc, ref maxLoc, general_mask);

                if (maxVal > conf)
                {
                    Rectangle rectangle = new Rectangle(maxLoc, template.Size);
                    CvInvoke.Rectangle(image, rectangle, new MCvScalar(255, 0, 0), 3);
                    mat.SetValue(minLoc.X, minLoc.Y, 0.0f);
                    mat.SetValue(maxLoc.X, maxLoc.Y, 0.0f);

                    float k_overlapping = 1.7f;//little overlapping is good for my task

                    //create template size for masking objects, which have been found,
                    //to be excluded in the next loop run
                    int template_w = (int)Math.Round(k_overlapping * template.Cols);
                    int template_h = (int)Math.Round(k_overlapping * template.Rows);
                    int x = maxLoc.X - template_w / 2;
                    int y = maxLoc.Y - template_h / 2;
                    if (y < 0) y = 0;
                    if (x < 0) x = 0;
                    //will template come beyond the mask?:if yes-cut off margin; 
                    if (template_w + x > general_mask.Cols)
                        template_w = general_mask.Cols - x;
                    if (template_h + y > general_mask.Rows)
                        template_h = general_mask.Rows - y;

                    Mat template_mask = Mat.Zeros(template_h, template_w, DepthType.Cv8U, 1);
                    Mat roi = new Mat(general_mask, new Rectangle(x, y, template_w, template_h));
                    template_mask.CopyTo(roi);
                    roi.Dispose();
                }
                else
                    break;
            }
            
            pictureBox1.Image = image.AsBitmap();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> image = ImageManager.PrepareImage((Bitmap)pictureBox1.Image, new Size(588, 1045));
            Mat hsvImage = new Mat();
            CvInvoke.CvtColor(image, hsvImage, ColorConversion.Bgr2Hsv);
            Mat txtImg = new Mat();
            CvInvoke.InRange(hsvImage, new ScalarArray(new MCvScalar(12, 63, 111)), new ScalarArray(new MCvScalar(13, 211, 206)), txtImg);


            pictureBox1.Image = txtImg.ToBitmap();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> image = ImageManager.PrepareImage((Bitmap)pictureBox1.Image, new Size(588, 1045));
            Mat backTest = new Mat(image.Mat, new Rectangle(131, 195, 30, 6));
            CvInvoke.CvtColor(backTest, backTest, ColorConversion.Bgr2Gray);
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            CvInvoke.MinMaxLoc(backTest, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            minVal -= 4;
            maxVal += 4;
            Mat txtImg = new Mat();
            CvInvoke.InRange(backTest, new ScalarArray(new MCvScalar(minVal)), new ScalarArray(new MCvScalar(maxVal)), txtImg);
            var imgRatio = ImageWhiteRatio(txtImg);
            pictureBox1.Image = txtImg.ToBitmap();

            Mat backIcon = new Mat(image.Mat, new Rectangle(96, 195, 30, 6));
            CvInvoke.CvtColor(backIcon, backIcon, ColorConversion.Bgr2Gray);
            Mat test2 = new Mat();
            CvInvoke.InRange(backIcon, new ScalarArray(new MCvScalar(minVal)), new ScalarArray(new MCvScalar(maxVal)), test2);
            var imgRatio2 = ImageWhiteRatio(test2);
            pictureBox1.Image = test2.ToBitmap();

        }

        private float ImageWhiteRatio(Mat thresImage)
        {
            if (thresImage.Depth != DepthType.Cv8U)
                throw new Exception("Изображение должно быть черно-белым");
            int whiteCount = CvInvoke.CountNonZero(thresImage);
            int imagePixelCount = thresImage.Rows * thresImage.Cols;
            float whiteRatio = (float)(whiteCount) / (float)imagePixelCount;
            return whiteRatio;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> image = ImageManager.PrepareImage((Bitmap)pictureBox1.Image, new Size(588, 1045));

            Mat hsvImage = new Mat();
            CvInvoke.CvtColor(image, hsvImage, ColorConversion.Bgr2Hsv);
            Mat txtImg = new Mat();
            CvInvoke.InRange(hsvImage, new ScalarArray(new MCvScalar(12, 63, 111)), new ScalarArray(new MCvScalar(13, 211, 206)), txtImg);
            VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint();
            Mat hierarhy = new Mat();
            CvInvoke.FindContours(txtImg, vvp, hierarhy, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            int x = int.MaxValue, y = int.MaxValue;
            for (int i = 0; i < vvp.Size; i++)
            {
                using (VectorOfPoint vp = vvp[i])
                {
                    Rectangle rect = CvInvoke.BoundingRectangle(vp);
                    if (rect.Height < 5)
                        continue;
                    if (rect.X < x)
                        x = rect.X;
                    if (rect.Y < y)
                        y = rect.Y;
                }
            }
            CvInvoke.Rectangle(image, new Rectangle(x-4, y-4, 350, 26), new MCvScalar(255, 50, 100));

            //CvInvoke.DrawContours(image, vvp, -1, new MCvScalar(255, 0, 255), 1);
            pictureBox1.Image = image.ToBitmap();
        }
    }
}
