using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace UmaMusumeDBBrowser
{
    public static class Extensions
    {

        private delegate void TextDelegate(Control textBox, string text);
        private delegate void VisibleDelegate(Control control, bool visible);
        private delegate void HeightDelegate(Control control, int Height);
        private delegate void ImageDelegate(PictureBox control, Image img);


        public static void SetTextToControl(Control textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                var d = new TextDelegate(SetTextToControl);
                textBox.Invoke(d, new object[] { textBox, text });
            }
            else
                textBox.Text = text;
        }


        public static void SetImageToPicBox(PictureBox picBox, Image img)
        {
            if (picBox.InvokeRequired)
            {
                var d = new ImageDelegate(SetImageToPicBox);
                picBox.Invoke(d, new object[] { picBox, img });
            }
            else
                picBox.Image = img;
        }

        public static void SetControlVisible(Control control, bool visible)
        {
            if (control.InvokeRequired)
            {
                var d = new VisibleDelegate(SetControlVisible);
                control.Invoke(d, new object[] { control, visible });
            }
            else
                control.Visible = visible;
        }

        public static void SetControlEnable(Control control, bool enable)
        {
            if (control.InvokeRequired)
            {
                var d = new VisibleDelegate(SetControlVisible);
                control.Invoke(d, new object[] { control, enable });
            }
            else
                control.Enabled = enable;
        }

        public static void SetControlHeight(Control control, int Height)
        {
            if (control.InvokeRequired)
            {
                var d = new HeightDelegate(SetControlHeight);
                control.Invoke(d, new object[] { control, Height });
            }
            else
                control.Height = Height;
        }


        public static dynamic GetValue(this Mat mat, int row, int col)
        {
            var value = CreateElement(mat.Depth);
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }

        public static void SetValue(this Mat mat, int row, int col, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }
        private static dynamic CreateElement(DepthType depthType, dynamic value)
        {
            var element = CreateElement(depthType);
            element[0] = value;
            return element;
        }

        private static dynamic CreateElement(DepthType depthType)
        {
            if (depthType == DepthType.Cv8S)
            {
                return new sbyte[1];
            }
            if (depthType == DepthType.Cv8U)
            {
                return new byte[1];
            }
            if (depthType == DepthType.Cv16S)
            {
                return new short[1];
            }
            if (depthType == DepthType.Cv16U)
            {
                return new ushort[1];
            }
            if (depthType == DepthType.Cv32S)
            {
                return new int[1];
            }
            if (depthType == DepthType.Cv32F)
            {
                return new float[1];
            }
            if (depthType == DepthType.Cv64F)
            {
                return new double[1];
            }
            return new float[1];
        }

        public static Image CropAtRect(this Image b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, -r.X, -r.Y);
                return nb;
            }
        }
        public static float PercentageComparison(this String orig, String text)
        {
            if (orig.Length != text.Length)
                return 0;
            byte[] origBytes = Encoding.GetEncoding(932).GetBytes(orig);
            byte[] textBytes = Encoding.GetEncoding(932).GetBytes(text);
            if (origBytes.Length != textBytes.Length)
                return 0;
            int badBytesCount = 0;
            for (int i = 0; i < origBytes.Length; i++)
            {
                if (origBytes[i] != textBytes[i])
                    badBytesCount++;
            }
            return 1 - (badBytesCount / origBytes.Length);
        }

        public static float PercentageComparison(this String orig, Encoding encodingTextBytes, byte[] textBytes)
        {
            byte[] origBytes = encodingTextBytes.GetBytes(orig);
            if (origBytes.Length != textBytes.Length)
                return 0;
            int badBytesCount = 0;
            for (int i = 0; i < origBytes.Length; i++)
            {
                if (origBytes[i] != textBytes[i])
                    badBytesCount++;
            }
            return 1.0f - ((float)badBytesCount / (float)origBytes.Length);
        }
    }




    public class FilterData
    {
        public string Column;
        public string Op;
        public string Value;

        public FilterData() { }

        public override string ToString()
        {
            switch (Op)
            {
                case "Equals":
                    {
                        return $"{Column}='{Value}'";
                    }
                case "Contains":
                    {
                        return string.Format((string)Column + " LIKE '%{0}%'", Value);
                    }
                case "Length":
                    {
                        return $"LEN({Column})={Value}";
                    }
                default:
                    return "";
            }
        }
    }
}
