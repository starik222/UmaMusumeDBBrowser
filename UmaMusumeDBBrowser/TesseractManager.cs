using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV.OCR;
using Emgu.CV;

namespace UmaMusumeDBBrowser
{
    public class TesseractManager
    {
        private Emgu.CV.OCR.Tesseract engine;
        //private TesseractEngine engine;
        public TesseractManager()
        {
            engine = new Emgu.CV.OCR.Tesseract(@"./tessdata", "jpn", OcrEngineMode.LstmOnly);
            //engine = new TesseractEngine(@"./tessdata", "jpn", EngineMode.Default);
        }

        public string GetText(IInputArray image)
        {
            engine.SetImage(image);
            if (engine.Recognize() != 0)
                throw new Exception("Ошибка распознавания изображения!");
            return engine.GetUTF8Text();
        }

        public string GetTextSingleLine(IInputArray image)
        {
            engine.PageSegMode = Emgu.CV.OCR.PageSegMode.SingleLine;
            return GetText(image).Replace(" ", "").Replace("\r", "").Replace("\n", "");
        }
        //public string GetText(Bitmap image, Rect region)
        //{
        //    Page page = engine.Process(image, region);
        //    string text = page.GetText();
        //    page.Dispose();
        //    text = text.Replace(" ", "");
        //    return text;
        //}
    }
}
