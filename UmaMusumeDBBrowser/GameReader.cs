using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV.CvEnum;
using Translator;
using System.Text.RegularExpressions;

namespace UmaMusumeDBBrowser
{
    public class GameReader
    {
        public bool IsStarted { get; set; }

        public delegate void GameData(object sender, GameDataArgs gameDataArgs);
        public event GameData DataChanged;

        //private Image<Bgr, byte> lastImage = null;
        private GameDataType lastDataType = GameDataType.NotFound;
        private List<SkillManager.SkillData> lastSkillResult;

        private UmaMusumeLibrary library;
        private SkillManager skManager;
        private GameSettings settings;
        private CancellationTokenSource tokenSource;
        private string cardName = null;

        private List<NAMES> charReplaceDictonary;
        private GameType gameType;
        private IntPtr gameHandle;


        public GameReader(GameType gameWindowType, UmaMusumeLibrary umaMusumeLibrary, SkillManager skillManager, GameSettings gameSettings, List<NAMES> replaceChars = null)
        {
            gameType = gameWindowType;
            library = umaMusumeLibrary;
            skManager = skillManager;
            settings = gameSettings;
            charReplaceDictonary = replaceChars;
            lastSkillResult = new List<SkillManager.SkillData>();
            gameHandle = IntPtr.Zero;

        }

        public void SetGameType(GameType gameWindowType)
        {
            gameType = gameWindowType;
        }


        public void Start(/*string InitialCardName,*/ int periodMillisec)
        {
            tokenSource = new CancellationTokenSource();
            //cardName = InitialCardName;
            IsStarted = true;
            Processed(periodMillisec, tokenSource.Token);
        }
        public async void StartAsync(/*string InitialCardName, */int periodMillisec)
        {
            await Task.Run(() => Start(/*InitialCardName, */periodMillisec));
        }

        public void Stop()
        {
            tokenSource.Cancel();
            IsStarted = false;
        }

        public bool SetWindowInfo(GameType gameWindowType, IntPtr handle)
        {
            gameType = gameWindowType;
            if (gameType == GameType.DMM)
            {
                gameHandle = WindowManager.FindWindow("UnityWndClass", "umamusume");
            }
            else
            {
                gameHandle = handle;
            }

            if (gameHandle == IntPtr.Zero)
            {
                return false;
            }
            return true;
        }



        private void Processed(int periodMillisec, CancellationToken token)
        {

            Image<Bgr, byte> currentImage = null;
            //Image<Bgr, byte> currentImageGray = null;
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(periodMillisec);
                if (!User32.IsWindow(gameHandle))
                {
                    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.GameNotFound, DataClass = "Game not found! Scan is stopped!" });
                    Stop();
                    break;
                }
                //handle = WindowManager.GetHandleByProcessName("BlueStacks");
                //if (handle == IntPtr.Zero)
                //{
                //    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.GameNotFound, DataClass = "Game not found!" });
                //    continue;
                //}
                Image origImg = WindowManager.CaptureWindow(gameHandle);
                if (origImg.Width <= 1 || origImg.Height <= 1)
                    continue;
                if (gameType == GameType.BluestacksV4)
                {
                    Rectangle rectangle = new Rectangle(settings.BlueStacksPanel.Ver4.X, settings.BlueStacksPanel.Ver4.Height, origImg.Width - settings.BlueStacksPanel.Ver4.X - settings.BlueStacksPanel.Ver4.Width,
                        origImg.Height - settings.BlueStacksPanel.Ver4.Y - settings.BlueStacksPanel.Ver4.Height);
                    if (rectangle.Width <= 0 || rectangle.Height <= 0 || rectangle.X <= 0 || rectangle.Y <= 0)
                    {
                        continue;
                    }
                    origImg = origImg.CropAtRect(rectangle);
                }
                else if (gameType == GameType.BluestacksV5)
                {
                    Rectangle rectangle = new Rectangle(settings.BlueStacksPanel.Ver5.X, settings.BlueStacksPanel.Ver5.Height, origImg.Width - settings.BlueStacksPanel.Ver5.X - settings.BlueStacksPanel.Ver5.Width,
                        origImg.Height - settings.BlueStacksPanel.Ver5.Y - settings.BlueStacksPanel.Ver5.Height);
                    if (rectangle.Width <= 0 || rectangle.Height <= 0 || rectangle.X <= 0 || rectangle.Y <= 0)
                    {
                        continue;
                    }
                    origImg = origImg.CropAtRect(rectangle);
                }
                Size normalSize = new Size();
                if (origImg.Width < origImg.Height)
                {
                    normalSize.Width = settings.GameNormalSize.Vertical.Width;
                    normalSize.Height = settings.GameNormalSize.Vertical.Height;
                }
                else
                {
                    normalSize.Width = settings.GameNormalSize.Horizontal.Width;
                    normalSize.Height = settings.GameNormalSize.Horizontal.Height;
                }
                currentImage = ImageManager.PrepareImage((Bitmap)origImg, normalSize);
                var dataType = DetectDataType(currentImage);
                if (dataType.type == GameDataType.NotFound)
                {
                    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.NotFound, DataClass = "Any game parts not found!" });
                    lastDataType = GameDataType.NotFound;
                    continue;
                }
                switch (dataType.type)
                {
                    case GameDataType.MainTraining:
                        {
                            DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = "main training...Not Implemented" });
                            break;
                        }
                    case GameDataType.TrainingEvent:
                        {
                            if (lastDataType == GameDataType.TrainingEvent)
                                continue;
                            var eventData = GetEventData(currentImage, dataType.PartInfo);
                            if (eventData != null)
                            {
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = eventData });
                            }
                            else
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = null });
                            //lastImage = currentImage;
                            lastDataType = GameDataType.TrainingEvent;
                            break;
                        }
                    case GameDataType.UmaSkillList:
                        {
                            var result = GetSkillList(currentImage);
                            if (result.Count > 0 && !result.Equals(lastSkillResult))
                            {
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = result });
                                lastSkillResult = result;
                            }
                            break;
                        }
                    default:
                        {
                            DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = "Other part" });
                            break;

                        }
                }
                currentImage.Dispose();
                origImg.Dispose();
            }
        }

        private string CorrectText(string text)
        {
            if (charReplaceDictonary == null)
                return text;
            foreach (var item in charReplaceDictonary)
            {
                text = text.Replace(item.orig_name, item.translit_name);
            }
            return text;
        }

        private List<SkillManager.SkillData> GetSkillList(Image<Bgr, byte> img)
        {
            Image<Gray, byte> grayImg = img.Convert<Gray, byte>();
            //Поиск контуров всех кнопок понижения.
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            List<Rectangle> partsLocInfo = new List<Rectangle>();
            var subParts = settings.GameParts.Find(a => a.PartName.Equals("SkillList"))?.SubGameParts;
            List<SkillManager.SkillData> skillDatas = new List<SkillManager.SkillData>();

            foreach (var item in subParts)
            {
                Mat imgOut = new Mat();
                CvInvoke.MatchTemplate(grayImg, item.Image, imgOut, TemplateMatchingType.CcoeffNormed);
                Mat general_mask = Mat.Ones(imgOut.Rows, imgOut.Cols, DepthType.Cv8U, 1);

                while (true)
                {
                    CvInvoke.MinMaxLoc(imgOut, ref minVal, ref maxVal, ref minLoc, ref maxLoc, general_mask);

                    if (maxVal > 0.58)
                    {
                        Rectangle rectangle = new Rectangle(maxLoc, item.Image.Size);
                        partsLocInfo.Add(rectangle);
                        imgOut.SetValue(minLoc.X, minLoc.Y, 0.0f);
                        imgOut.SetValue(maxLoc.X, maxLoc.Y, 0.0f);

                        float k_overlapping = 1.7f;
                        int template_w = (int)Math.Round(k_overlapping * item.Image.Cols);
                        int template_h = (int)Math.Round(k_overlapping * item.Image.Rows);
                        int x = maxLoc.X - template_w / 2;
                        int y = maxLoc.Y - template_h / 2;
                        if (y < 0) y = 0;
                        if (x < 0) x = 0;
                        if (template_w + x > general_mask.Cols)
                            template_w = general_mask.Cols - x;
                        if (template_h + y > general_mask.Rows)
                            template_h = general_mask.Rows - y;

                        Mat template_mask = Mat.Zeros(template_h, template_w, DepthType.Cv8U, 1);
                        Mat roi = new Mat(general_mask, new Rectangle(x, y, template_w, template_h));
                        template_mask.CopyTo(roi);
                        roi.Dispose();
                        template_mask.Dispose();
                    }
                    else
                        break;
                }
                general_mask.Dispose();
                imgOut.Dispose();
            }
            //Создание рабочих контуров
            partsLocInfo.Sort((a, b) => a.Y.CompareTo(b.Y));
            foreach (var item in partsLocInfo)
            {
                if (item.Y < (settings.GameElements.SkillListWindow.Y + 26))//исключение первого умения, если его название обрезано.
                    continue;
                Rectangle textCountor = new Rectangle(item.X - settings.GameElements.SkillListWindow.Width, item.Y - 26, settings.GameElements.SkillListWindow.Width - 10, 23);
                if (Program.IsDebug)
                {
                    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = (new Mat(img.Mat, textCountor)).ToBitmap() });
                }

                Mat textMat = new Mat(img.Mat, textCountor);
                CvInvoke.Resize(textMat, textMat, new Size(), 2, 2);
                Mat mask = new Mat();
                Mat hsvMat = new Mat();
                CvInvoke.CvtColor(textMat, hsvMat, ColorConversion.Bgr2Hsv);
                bool isNext = false;
                MCvScalar minS = new MCvScalar(10, 45, 70);
                MCvScalar maxS = new MCvScalar(13, 218, 204);
            nextTry:
                CvInvoke.InRange(hsvMat, new ScalarArray(minS), new ScalarArray(maxS), mask);
                CvInvoke.BitwiseNot(mask, mask);
                Mat resMat = textMat.Clone();
                resMat.SetTo(new MCvScalar(255, 255, 255), mask);

                if (Program.IsDebug)
                {
                    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = textMat.ToBitmap() });
                }

                string text = Program.TessManager.GetTextSingleLine(resMat);

                var res = skManager.FindSkillByName(text, true, 0.8f);
                if (res.Count == 1)
                    skillDatas.Add(res[0].Value);
                else if (res.Count > 1)
                {
                    var s = GetSkillWithMaxConfidence(res);
                    if (s != null)
                        skillDatas.Add(s);
                }
                else if (res.Count == 0 && !string.IsNullOrWhiteSpace(text))
                {
                    res = skManager.FindEventByNameDiceAlg(text);
                    if (res.Count == 1)
                        skillDatas.Add(res[0].Value);
                    else if (res.Count > 1)
                    {
                        var s = GetSkillWithMaxConfidence(res);
                        if (s != null)
                            skillDatas.Add(s);
                    }
                }



                if(res.Count == 0 && !isNext)
                {
                    resMat.Dispose();
                    minS = new MCvScalar(10, 67, 75);
                    maxS = new MCvScalar(14, 212, 162);
                    isNext = true;
                    goto nextTry;
                }
                resMat.Dispose();
                textMat.Dispose();
                hsvMat.Dispose();
                mask.Dispose();
            }
            return skillDatas;


        }

        private SkillManager.SkillData GetSkillWithMaxConfidence(List<KeyValuePair<float, SkillManager.SkillData>> datas)
        {
            float c = -1;
            int index = -1;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i].Key > c)
                {
                    index = i;
                    c = datas[i].Key;
                }
            }
            if (index == -1)
                return null;
            return datas[index].Value;
        }

        private string DeleteNumbers(string text)
        {
            string pattern = "[1234567890]";
            return Regex.Replace(text, pattern, "");
        }

        private UmaMusumeLibrary.EventData GetEventData(Image<Bgr, byte> img, Rectangle partInfo)
        {
            int offset = 0;
            if (IsEventNameIcon(img.Mat))
            {
                offset = settings.GameElements.EventNameIconBounds.Width + 5;
            }
            Mat mat = new Mat(img.Mat, new Rectangle(settings.GameElements.EventNameBounds.X + offset, settings.GameElements.EventNameBounds.Y,
                settings.GameElements.EventNameBounds.Width, settings.GameElements.EventNameBounds.Height));
            Mat invertedMat = new Mat();
            CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
            CvInvoke.BitwiseNot(mat, invertedMat);
            Mat threshMat = new Mat();
            CvInvoke.Threshold(invertedMat, threshMat, 0.0, 255.0, ThresholdType.Otsu);
            //img = img.ThresholdBinary(new Gray(111), new Gray(255));
            if (Program.IsDebug)
            {
                DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = invertedMat.ToBitmap() });
            }
            string text = Program.TessManager.GetTextSingleLine(invertedMat);
            text = CorrectText(text);
            //text = DeleteNumbers(text);
            //Пробуем искать по имени.
            var eventData = library.FindEventByName(text, true);
            if (eventData.Count == 0)
            {
                string text2 = Program.TessManager.GetTextSingleLine(threshMat);
                text2 = CorrectText(text2);
                eventData = library.FindEventByName(text2, true);
            }
            if (eventData.Count == 0)
            {
                eventData = library.FindEventByNameDiceAlg(text);
            }

            if (eventData.Count == 1)
            {
                return eventData[0];
            }
            else
            {
                bool isNext = false;
            nextTry:
                Size textSize = new Size(420, partInfo.Height + 2);
                Point textStartPoint = new Point(partInfo.X + 35, partInfo.Y - 2);
                //Поиск первого варианта
                Rectangle textRect = new Rectangle(textStartPoint, textSize);
                text = GetOptionText(img.Mat, textRect, isNext);
                text = CorrectText(text);
                text = UmaMusumeLibrary.PrepareText(text);
                if (eventData.Count > 1)
                {

                    var res = eventData.Find(a => a.ContainsOption(text, true));
                    if (res == null)
                    {
                        //Поиск второго варианта
                        textRect.Y += 90;
                        text = GetOptionText(img.Mat, textRect, isNext);
                        text = CorrectText(text);
                        text = UmaMusumeLibrary.PrepareText(text);
                        var res2 = eventData.Find(a => a.ContainsOption(text, true));
                        if (!isNext && res2 == null)
                        {
                            isNext = true;
                            goto nextTry;
                        }
                        return res2;
                    }
                    else
                        return res;
                }
                else
                {
                    var res = library.FindEventByOption(text, true);
                    if (res.Count == 1)
                        return res[0];
                    else if (res.Count == 0)
                    {
                        //Поиск второго варианта
                        //textStartPoint.Y += 90;
                        //textRect = new Rectangle(textStartPoint, textSize);
                        textRect.Y += 90;
                        text = GetOptionText(img.Mat, textRect, isNext);
                        text = CorrectText(text);
                        text = UmaMusumeLibrary.PrepareText(text);
                        res = library.FindEventByOption(text, true);
                        if (res.Count == 1)
                            return res[0];
                        else
                        {
                            if (!isNext)
                            {
                                isNext = true;
                                goto nextTry;
                            }
                            return null;
                        }
                    }
                    else
                    {
                        //Поиск второго варианта
                        //textStartPoint.Y += 90;
                        //textRect = new Rectangle(textStartPoint, textSize);
                        textRect.Y += 90;
                        text = GetOptionText(img.Mat, textRect, isNext);
                        text = CorrectText(text);
                        text = UmaMusumeLibrary.PrepareText(text);
                        var res2 = res.Find(a => a.ContainsOption(text, true));
                        if (!isNext && res2 == null)
                        {
                            isNext = true;
                            goto nextTry;
                        }
                        return res2;
                    }

                    //var res = eventData.Find(a => a.ContainsOption(text));

                }

                //Поиск по вариантам выбора???
            }
        }

        private string GetOptionText(Mat origImg, Rectangle rect, bool method2)
        {
            Mat cutImg = new Mat();
            cutImg = new Mat(origImg, rect);
            //Mat hsvImage = new Mat();
            //CvInvoke.CvtColor(cutImg, hsvImage, ColorConversion.Bgr2Hsv);
            //Mat txtImg = new Mat();
            //CvInvoke.InRange(hsvImage, new ScalarArray(new MCvScalar(12,75,100)), new ScalarArray(new MCvScalar(13, 255, 180)), txtImg);
            //if (Program.IsDebug)
            //{
            //    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = hsvImage.ToBitmap() });
            //}
            CvInvoke.CvtColor(cutImg, cutImg, ColorConversion.Bgr2Gray);
            if (method2)
            {
                CvInvoke.Threshold(cutImg, cutImg, 0.0, 255.0, ThresholdType.Otsu);
            }
            string text = Program.TessManager.GetTextSingleLine(cutImg);
            cutImg.Dispose();
            return text;
        }

        private (GameDataType type, Rectangle PartInfo) DetectDataType(Image<Bgr, byte> img)
        {
            Image<Gray, byte> grayImg = img.Convert<Gray, byte>();
            double[] minVal, maxVal;
            Point[] minLoc, maxLoc;
            foreach (var item in settings.GameParts)
            {
                Mat imgOut = new Mat();
                CvInvoke.MatchTemplate(grayImg, item.Image, imgOut, TemplateMatchingType.CcoeffNormed);
                imgOut.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);
                imgOut.Dispose();
                if (maxVal[0] > 0.86)
                {
                    if (Program.IsDebug)
                    {
                        var tempImg = grayImg.Clone();
                        CvInvoke.Rectangle(tempImg, new Rectangle(/*new Point(maxLoc[0].X + item.Image.Width, maxLoc[0].Y + item.Image.Height)*/maxLoc[0], item.Image.Size), new MCvScalar(0, 0, 0), 3);
                        DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = tempImg.AsBitmap() });
                        tempImg.Dispose();
                    }
                    return (item.DataType, new Rectangle(maxLoc[0], item.Image.Size));
                }
            }

            return (GameDataType.NotFound, new Rectangle());

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


        private bool IsEventNameIcon(Mat img)
        {
            bool result = false;
            Rectangle iconPartRect = settings.GameElements.EventNameIconBounds.GetRectangle();
            iconPartRect.Height = 6;
            Rectangle testBackRect = iconPartRect;
            testBackRect.X += 35;
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();

            Mat backForTest = new Mat(img, testBackRect);
            Mat iconMat = new Mat(img, iconPartRect);
            CvInvoke.CvtColor(backForTest, backForTest, ColorConversion.Bgr2Gray);
            CvInvoke.CvtColor(iconMat, iconMat, ColorConversion.Bgr2Gray);
            CvInvoke.MinMaxLoc(backForTest, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            minVal -= 5;
            maxVal += 5;
            Mat maskMat = new Mat();
            CvInvoke.InRange(backForTest, new ScalarArray(new MCvScalar(minVal)), new ScalarArray(new MCvScalar(maxVal)), maskMat);
            var imgRatio = ImageWhiteRatio(maskMat);
            if (imgRatio < 0.9f)
                throw new Exception("Ошибочная реализация?");
            CvInvoke.InRange(iconMat, new ScalarArray(new MCvScalar(minVal)), new ScalarArray(new MCvScalar(maxVal)), maskMat);
            imgRatio = ImageWhiteRatio(maskMat);

            if (imgRatio < 0.7)
                result = true;
            maskMat.Dispose();
            iconMat.Dispose();
            backForTest.Dispose();
            return result;
        }





        public class GameDataArgs
        {
            public GameDataType DataType { get; set; }
            public object DataClass { get; set; }
        }

        public enum GameDataType
        {
            MainTraining = 0,
            TrainingEvent = 1,
            SelectCurrentRace = 2,
            SelectPlanRace = 3,
            UmaInfo = 4,
            UmaSkillList = 5,
            GameNotFound = -1,
            NotFound = -2,
            DebugImage = -3
        }

        public enum GameType
        {
            DMM,
            BluestacksV4,
            BluestacksV5
        }
    }


}
