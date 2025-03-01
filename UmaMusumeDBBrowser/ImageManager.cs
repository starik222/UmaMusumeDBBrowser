using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV.CvEnum;

namespace UmaMusumeDBBrowser
{
    public class ImageManager
    {
        public ImageManager()
        {

        }

        public static Image<Gray, byte> PrepareImageGray(Bitmap origImg, Size normalSize)
        {
            Image<Bgr, byte> img = NormalizeImage(origImg.ToImage<Bgr, byte>(), normalSize);
            //Image<Gray, byte> grayImage = img.Convert<Gray, byte>().ThresholdAdaptive(new Gray(255), Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.ToZero, 201, new Gray(111));
            Image<Gray, byte> grayImage = img.Convert<Gray, byte>();

            return grayImage;
        }

        public static Image<Bgr, byte> PrepareImage(Bitmap origImg, Size normalSize)
        {
            return NormalizeImage(origImg.ToImage<Bgr, byte>(), normalSize);
        }

        public static Image<Bgr, byte> ResizeImage(Image<Bgr, byte> img, double scale)
        {
            return img.Resize(scale, Inter.Lanczos4);
        }

        public static Image<Bgr, byte> NormalizeImage(Image<Bgr, byte> img, Size normalSize)
        {
            double scale = (double)normalSize.Height / (double)img.Height;

            return ResizeImage(img, scale);
        }
    }
}
