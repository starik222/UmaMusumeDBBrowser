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
        private Emgu.CV.OCR.Tesseract engineJpn;
        private Emgu.CV.OCR.Tesseract engineUma;
        //private TesseractEngine engine;
        public TesseractManager()
        {
            engineJpn = new Emgu.CV.OCR.Tesseract(@"./tessdata", "jpn", OcrEngineMode.LstmOnly);
            engineUma = new Emgu.CV.OCR.Tesseract(@"./tessdata", "uma", OcrEngineMode.LstmOnly);
            //engine = new TesseractEngine(@"./tessdata", "jpn", EngineMode.Default);
        }

        public string GetText(IInputArray image, TessDict dict)
        {
            GetEngine(dict).SetImage(image);
            if (GetEngine(dict).Recognize() != 0)
                throw new Exception("Ошибка распознавания изображения!");
            return GetEngine(dict).GetUTF8Text();
        }

        public string GetTextMultiLine(IInputArray image, TessDict dict)
        {
            GetEngine(dict).PageSegMode = Emgu.CV.OCR.PageSegMode.SingleBlock;
            string text = GetText(image, dict).Replace(" ", "").Replace("\r", "");
            if (!string.IsNullOrWhiteSpace(text) && text.EndsWith("\n"))
            {
                text = text.Substring(0, text.Length - 1);
            }
            return text.Replace("\n", "\\n");
        }

        public string GetTextSingleLine(IInputArray image, TessDict dict)
        {
            GetEngine(dict).PageSegMode = Emgu.CV.OCR.PageSegMode.SingleLine;
            return GetText(image, dict).Replace(" ", "").Replace("\r", "").Replace("\n", "");
        }
        //public string GetText(Bitmap image, Rect region)
        //{
        //    Page page = engine.Process(image, region);
        //    string text = page.GetText();
        //    page.Dispose();
        //    text = text.Replace(" ", "");
        //    return text;
        //}
        private Emgu.CV.OCR.Tesseract GetEngine(TessDict dict)
        {
            switch (dict)
            {
                case TessDict.jpn:
                    return engineJpn;
                case TessDict.uma:
                    return engineUma;
                default:
                    throw new Exception("Должен быть указан словарь!");
            }
        }

        public enum TessDict
        {
            jpn,
            uma
        }
    }
}
