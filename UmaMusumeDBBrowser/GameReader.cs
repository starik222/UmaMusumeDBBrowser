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
        private List<FactorManager.FactorData> lastGenResult;
        private List<MissionManager.MissionData> lastMissionResult;
        private AllLibraryManager libraryManager;
        private GameSettings settings;
        private CancellationTokenSource tokenSource;
        private string cardName = null;
        private List<NAMES> charReplaceDictonary;
        private GameType gameType;
        private IntPtr gameHandle;

        public bool BsTopPanelVisible { get; set; } = true;
        public bool BsRightPanelVisible { get; set; } = true;

        private const float optionsConfidence = 0.6f;


        public GameReader(GameType gameWindowType, AllLibraryManager libManager, GameSettings gameSettings, List<NAMES> replaceChars = null)
        {
            libraryManager = libManager;
            gameType = gameWindowType;
            settings = gameSettings;
            charReplaceDictonary = replaceChars;
            lastSkillResult = new List<SkillManager.SkillData>();
            lastGenResult = new List<FactorManager.FactorData>();
            lastMissionResult = new List<MissionManager.MissionData>();
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
            bool isVertical = true;
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
                if (origImg == null || origImg.Width <= 1 || origImg.Height <= 1)
                    goto Exit;
                if (gameType == GameType.BluestacksV4)
                {
                    Rectangle rectangle = new Rectangle(settings.BlueStacksPanel.Ver4.X, settings.BlueStacksPanel.Ver4.Height, origImg.Width - settings.BlueStacksPanel.Ver4.X - (BsRightPanelVisible ? settings.BlueStacksPanel.Ver4.Width : settings.BlueStacksPanel.Ver4.X),
                        origImg.Height - settings.BlueStacksPanel.Ver4.Y - (BsTopPanelVisible ? settings.BlueStacksPanel.Ver4.Height : settings.BlueStacksPanel.Ver4.Y));

                    if (rectangle.Width <= 0 || rectangle.Height <= 0 || rectangle.X <= 0 || rectangle.Y <= 0)
                    {
                        goto Exit;
                    }
                    origImg = origImg.CropAtRect(rectangle);
                }
                else if (gameType == GameType.BluestacksV5)
                {
                    Rectangle rectangle = new Rectangle(settings.BlueStacksPanel.Ver5.X, settings.BlueStacksPanel.Ver5.Height, origImg.Width - settings.BlueStacksPanel.Ver5.X - (BsRightPanelVisible ? settings.BlueStacksPanel.Ver5.Width : settings.BlueStacksPanel.Ver5.X),
                        origImg.Height - settings.BlueStacksPanel.Ver5.Y - (BsTopPanelVisible ? settings.BlueStacksPanel.Ver5.Height : settings.BlueStacksPanel.Ver5.Y));
                    if (rectangle.Width <= 0 || rectangle.Height <= 0 || rectangle.X <= 0 || rectangle.Y <= 0)
                    {
                        goto Exit;
                    }
                    origImg = origImg.CropAtRect(rectangle);
                }
                Size normalSize = new Size();
                if (origImg.Width < origImg.Height)
                {
                    isVertical = true;
                    normalSize.Width = settings.GameNormalSize.Vertical.Width;
                    normalSize.Height = settings.GameNormalSize.Vertical.Height;
                }
                else
                {
                    isVertical = false;
                    normalSize.Width = settings.GameNormalSize.Horizontal.Width;
                    normalSize.Height = settings.GameNormalSize.Horizontal.Height;
                }
                currentImage = ImageManager.PrepareImage((Bitmap)origImg, normalSize);
                var dataType = DetectDataType(currentImage, isVertical);
                if (dataType.type == GameDataType.NotFound)
                {
                    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.NotFound, DataClass = "Any game parts not found!" });
                    lastDataType = GameDataType.NotFound;
                    goto Exit;
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
                                goto Exit;
                            var eventData = GetEventData(currentImage, dataType.PartInfo);
                            if (eventData != null)
                            {
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = eventData });
                                lastDataType = GameDataType.TrainingEvent;
                            }
                            else
                            {
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = null });
                                lastDataType = GameDataType.NotFound;
                            }

                            //lastImage = currentImage;
                            
                            break;
                        }
                    case GameDataType.TazunaAfterHelp:
                        {
                            if (lastDataType == GameDataType.TazunaAfterHelp)
                                goto Exit;
                            if (dataType.PartInfo.Y > (settings.GameTypeElements[gameType].TazunaAfterHelpWindow.Y + settings.GameTypeElements[gameType].TazunaAfterHelpWindow.Height))
                                goto Exit;
                            var result = GetTazunaAfterHelp(currentImage, dataType.PartInfo);

                            DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = new TazunaHelpRelult() { Desc = result.desc, Warning = result.warning } });
                            //var eventData = GetEventData(currentImage, dataType.PartInfo);
                            //if (eventData != null)
                            //{
                            //    DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = eventData });
                            //}
                            //else
                            //    DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = null });
                            ////lastImage = currentImage;
                            lastDataType = GameDataType.TazunaAfterHelp;
                            break;
                        }
                    case GameDataType.SkillDetailView:
                    case GameDataType.UmaSkillList:
                        {
                            var result = GetSkillList(currentImage, dataType.PartInfo, dataType.type);
                            if (result.Count > 0 && !result.Equals(lastSkillResult))
                            {
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.UmaSkillList, DataClass = result });
                                lastSkillResult = result;
                            }
                            break;
                        }
                    case GameDataType.FactorDetailView:
                    case GameDataType.GenWindow:
                        {
                            var result = GetSFactorList(currentImage, dataType.type);
                            if (result.Count > 0 && !EqualFactorList(result, lastGenResult))
                            {
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.GenWindow, DataClass = result });
                                lastGenResult = result;
                            }
                            break;
                        }
                    case GameDataType.MissionBtn:
                        {
                            var result = GetMissionList(currentImage, dataType.type);
                            if (result.Count > 0 && !EqualMissionList(result, lastMissionResult))
                            {
                                DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.MissionBtn, DataClass = result });
                                lastMissionResult = result;
                            }
                            break;
                        }
                    default:
                        {
                            DataChanged?.Invoke(this, new GameDataArgs() { DataType = dataType.type, DataClass = "Other part" });
                            break;

                        }
                }
            Exit:
                if (currentImage != null)
                    currentImage.Dispose();
                if (origImg != null)
                    origImg.Dispose();
            }
        }

        private bool EqualFactorList(List<FactorManager.FactorData> list1, List<FactorManager.FactorData> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].Id != list2[i].Id)
                    return false;
            }
            return true;
        }

        private bool EqualMissionList(List<MissionManager.MissionData> list1, List<MissionManager.MissionData> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].Id != list2[i].Id)
                    return false;
            }
            return true;
        }


        private (string desc, string warning) GetTazunaAfterHelp(Image<Bgr, byte> img, Rectangle partInfo)
        {
            string warningText = null;
            string descText = null;
            string tempText = null;
            Rectangle tazunaHelpWindowRect = settings.GameTypeElements[gameType].TazunaAfterHelpWindow.GetRectangle();
            if (IsTazunaHelpWarning(img.Mat))
            {
                tazunaHelpWindowRect.Y += 4;
                Mat warningWindow = new Mat(img.Mat, settings.GameTypeElements[gameType].TazunaWarningWindow.GetRectangle());
                CvInvoke.CvtColor(warningWindow, warningWindow, ColorConversion.Bgr2Gray);
                if (Program.IsDebug)
                {
                    DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = warningWindow.ToBitmap() });
                }
                tempText = Program.TessManager.GetTextSingleLine(warningWindow);
                warningWindow.Dispose();
                var warnRes = libraryManager.TazunaLibrary.FindHelpItemByDescDice(tempText, TazunaManager.HelpType.AfterRaceWarning);
                if (warnRes != null)
                    warningText = warnRes.Translations;
            }

            Mat helpWindow = new Mat(img.Mat, tazunaHelpWindowRect);
            CvInvoke.CvtColor(helpWindow, helpWindow, ColorConversion.Bgr2Gray);
            if (Program.IsDebug)
            {
                DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = helpWindow.ToBitmap() });
            }
            tempText = Program.TessManager.GetTextMultiLine(helpWindow);
            tempText = CorrectText(tempText);
            helpWindow.Dispose();

            var res = libraryManager.TazunaLibrary.FindHelpItemByDescDice(tempText, TazunaManager.HelpType.AfterRaceDesc);
            if (res != null)
            {
                descText =  res.Translations;
            }
            return (descText, warningText);
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

        private List<Rectangle> GetGamePartsRects(Image<Gray, byte> grayImg, List<GameSettings.GamePart> parts, double confid = 0.58)
        {
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            List<Rectangle> partsLocInfo = new List<Rectangle>();


            foreach (var item in parts)
            {
                Mat imgOut = new Mat();
                CvInvoke.MatchTemplate(grayImg, item.Image, imgOut, TemplateMatchingType.CcoeffNormed);
                Mat general_mask = Mat.Ones(imgOut.Rows, imgOut.Cols, DepthType.Cv8U, 1);

                while (true)
                {
                    CvInvoke.MinMaxLoc(imgOut, ref minVal, ref maxVal, ref minLoc, ref maxLoc, general_mask);

                    if (maxVal > confid)
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
            partsLocInfo.Sort((a, b) => a.Y.CompareTo(b.Y));
            return partsLocInfo;

        }


        private Rectangle GetGamePartRect(Image<Gray, byte> grayImg, GameSettings.GamePart part, double confid = 0.58)
        {
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            List<Rectangle> partsLocInfo = new List<Rectangle>();

            Mat imgOut = new Mat();
            CvInvoke.MatchTemplate(grayImg, part.Image, imgOut, TemplateMatchingType.CcoeffNormed);
            CvInvoke.MinMaxLoc(imgOut, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            Rectangle rectangle = new Rectangle(maxLoc, part.Image.Size);
            imgOut.Dispose();

            return rectangle;

        }


        private List<FactorManager.FactorData> GetSFactorList(Image<Bgr, byte> img, GameDataType dataType)
        {
            List<FactorManager.FactorData> factorDatas = new List<FactorManager.FactorData>();
            Image<Gray, byte> grayImg = img.Convert<Gray, byte>();
            if (dataType == GameDataType.GenWindow)
            {
                var subParts = settings.GameParts.Find(a => a.PartName.Equals("SFactor"))?.SubGameParts;
                if (subParts == null || subParts.Count == 0)
                    return factorDatas;
                List<Rectangle> partsLocInfo = GetGamePartsRects(grayImg, subParts, 0.65);
                for (int i = 0; i < partsLocInfo.Count; i++)
                {
                    //исправление координат
                    Rectangle partRect = partsLocInfo[i];
                    partRect.X -= 468;
                    partRect.Width = 335;
                    partRect.Height += 2;
                    //---
                    var result = GetFactorFromPart(img, partRect, i);
                    if (result != null)
                        factorDatas.Add(result);
                }
            }
            else if (dataType == GameDataType.FactorDetailView)
            {
                var subParts = settings.GameParts.Find(a => a.PartName.Equals(dataType.ToString()))?.SubGameParts;
                if (subParts == null || subParts.Count == 0)
                    return factorDatas;
                var partRect = GetGamePartsRects(grayImg, subParts).FirstOrDefault();
                partRect.X -= 410;
                partRect.Y -= 2;
                partRect.Width = 315;
                partRect.Height = 26;

                int factorIndex = GetFactorIndex(img, partRect);

                var result = GetFactorFromPart(img, partRect, factorIndex);
                if (result != null)
                    factorDatas.Add(result);
            }
            grayImg.Dispose();
            return factorDatas;
        }


        private List<MissionManager.MissionData> GetMissionList(Image<Bgr, byte> img, GameDataType dataType)
        {
            List<MissionManager.MissionData> missionDatas = new List<MissionManager.MissionData>();
            Image<Gray, byte> grayImg = img.Convert<Gray, byte>();
            List<GameSettings.GamePart> parts = new List<GameSettings.GamePart>();

            var part = settings.GameParts.Find(a => a.PartName.Equals("MissionBtn"));
            if (part != null)
                parts.Add(part);
            //part = settings.GameParts.Find(a => a.PartName.Equals("MissionBtn2"));
            //if (part != null)
            //    parts.Add(part);
            if (parts.Count == 0)
                return missionDatas;
            List<Rectangle> partsLocInfo = GetGamePartsRects(grayImg, parts, 0.75);
            for (int i = 0; i < partsLocInfo.Count; i++)
            {
                //исправление координат
                Rectangle partRect = partsLocInfo[i];
                partRect.X -= 335;
                partRect.Y -= 22;
                partRect.Width = 320;
                partRect.Height = 46;
                if (partRect.Y < 400)
                    continue;
                //partRect.Height += 2;
                //---
                var result = GetMissionFromPart(img, partRect);
                if (result != null)
                    missionDatas.Add(result);
            }
            grayImg.Dispose();
            return missionDatas;
        }

        private MissionManager.MissionData GetMissionFromPart(Image<Bgr, byte> img, Rectangle partRect)
        {
            MissionManager.MissionData missionData = null;
            Mat textMat = new Mat(img.Mat, partRect);
            //нужно ли?
            //CvInvoke.Resize(textMat, textMat, new Size(), 2, 2);

            CvInvoke.CvtColor(textMat, textMat, ColorConversion.Bgr2Gray);
            if (Program.IsDebug)
            {
                DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = textMat.ToBitmap() });
            }
            string tempText = Program.TessManager.GetTextMultiLine(textMat).Replace("\\n", "");
            tempText = CorrectText(tempText);
            textMat.Dispose();

            missionData = libraryManager.MissionLibrary.FindMissionItemByTextDice(tempText);
            return missionData;
        }

        private int GetFactorIndex(Image<Bgr, byte> img, Rectangle bounds)
        {
            int index = 3;
            Mat namePart = new Mat(img.Mat, bounds);
            Mat hsvImg = new Mat();
            CvInvoke.CvtColor(namePart, hsvImg, ColorConversion.Bgr2Hsv);
            //index 0
            Mat mask = new Mat();
            CvInvoke.InRange(hsvImg, new ScalarArray(new MCvScalar(98, 0, 0)), new ScalarArray(new MCvScalar(103, 255, 255)), mask);
            var ratio = ImageWhiteRatio(mask);
            if (ratio > 0.7)
            {
                index = 0;
                goto exit;
            }
            //index 1
            CvInvoke.InRange(hsvImg, new ScalarArray(new MCvScalar(165, 0, 0)), new ScalarArray(new MCvScalar(169, 255, 255)), mask);
            ratio = ImageWhiteRatio(mask);
            if (ratio > 0.7)
            {
                index = 1;
                goto exit;
            }
            //index 2
            CvInvoke.InRange(hsvImg, new ScalarArray(new MCvScalar(38, 0, 0)), new ScalarArray(new MCvScalar(50, 255, 255)), mask);
            ratio = ImageWhiteRatio(mask);
            if (ratio > 0.7)
            {
                index = 2;
                goto exit;
            }

        exit:
            mask.Dispose();
            hsvImg.Dispose();
            namePart.Dispose();
            return index;
        }


        private FactorManager.FactorData GetFactorFromPart(Image<Bgr, byte> img, Rectangle partRect, int partPosition=0)
        {
            FactorManager.FactorData factorData = null;
            Mat textMat = new Mat(img.Mat, partRect);
            Mat mask = new Mat();
            Mat hsvMat = new Mat();
            Mat resMat = new Mat();
            CvInvoke.Resize(textMat, textMat, new Size(), 2, 2);

            bool needCorrect = false;
            bool isNext = false;
        nextTry:
            if (!needCorrect)
            {

                resMat = textMat.Clone();
                if (partPosition < 3)
                    CvInvoke.BitwiseNot(resMat, resMat);
            }
            else
            {
                MCvScalar minS = new MCvScalar();
                MCvScalar maxS = new MCvScalar();
                if (partPosition < 3)
                {
                    //CvInvoke.BitwiseNot(textMat, textMat);
                    if (!isNext)
                    {
                        minS = new MCvScalar(0, 0, 233);
                        maxS = new MCvScalar(185, 65, 255);
                    }
                    else
                    {
                        minS = new MCvScalar(0, 0, 239);
                        maxS = new MCvScalar(170, 24, 255);
                    }
                }
                else
                {
                    if (!isNext)
                    {
                        minS = new MCvScalar(12, 45, 70);
                        maxS = new MCvScalar(13, 218, 204);
                    }
                    else
                    {
                        minS = new MCvScalar(12, 107, 120);
                        maxS = new MCvScalar(13, 210, 154);
                    }
                }

                CvInvoke.CvtColor(textMat, hsvMat, ColorConversion.Bgr2Hsv);

                CvInvoke.InRange(hsvMat, new ScalarArray(minS), new ScalarArray(maxS), mask);
                CvInvoke.BitwiseNot(mask, mask);
                resMat = textMat.Clone();
                if (partPosition < 3)
                {
                    CvInvoke.BitwiseNot(resMat, resMat);
                }
                resMat.SetTo(new MCvScalar(255, 255, 255), mask);
            }


            if (Program.IsDebug)
            {
                DataChanged?.Invoke(this, new GameDataArgs() { DataType = GameDataType.DebugImage, DataClass = resMat.ToBitmap() });
            }

            string text = Program.TessManager.GetTextSingleLine(resMat);
            if (!string.IsNullOrWhiteSpace(text))
            {
                var res = libraryManager.FactorLibrary.FindFactorByNameDiceAlg(text, 0.3f);
                if (res.Count == 1)
                    factorData = res[0].Value;
                else if (res.Count > 1)
                {
                    var s = GetItemWithMaxConfidence<FactorManager.FactorData>(res);
                    if (s != null)
                        factorData = s;
                }
                else
                {
                    if (!needCorrect)
                    {
                        needCorrect = true;
                        goto nextTry;
                    }
                    else if (!isNext)
                    {
                        isNext = true;
                        goto nextTry;
                    }
                }
            }
            else
            {
                if (!needCorrect)
                {
                    needCorrect = true;
                    goto nextTry;
                }
                else if (!isNext)
                {
                    isNext = true;
                    goto nextTry;
                }
            }
            textMat.Dispose();
            resMat.Dispose();
            hsvMat.Dispose();
            mask.Dispose();

            return factorData;
        }

        private List<SkillManager.SkillData> GetSkillList(Image<Bgr, byte> img, Rectangle gamePart, GameDataType dataType)
        {
            List<SkillManager.SkillData> skillDatas = new List<SkillManager.SkillData>();
            if (dataType == GameDataType.UmaSkillList)
            {
                Image<Gray, byte> grayImg = img.Convert<Gray, byte>();
                //Поиск контуров всех кнопок понижения.

                var subParts = settings.GameParts.Find(a => a.PartName.Equals("SkillList"))?.SubGameParts;

                if (subParts == null || subParts.Count == 0)
                {
                    grayImg.Dispose();
                    return skillDatas;
                }
                List<Rectangle> partsLocInfo = GetGamePartsRects(grayImg, subParts);
                //Создание рабочих контуров
                foreach (var item in partsLocInfo)
                {
                    if (item.Y < (settings.GameTypeElements[gameType].SkillListWindow.Y + settings.GameTypeElements[gameType].SkillNameCorrectBounds.Y))//исключение первого умения, если его название обрезано.
                        continue;
                    Rectangle textCountor = new Rectangle(item.X - settings.GameTypeElements[gameType].SkillListWindow.Width,
                        item.Y - settings.GameTypeElements[gameType].SkillNameCorrectBounds.Y, settings.GameTypeElements[gameType].SkillNameCorrectBounds.Width,
                        settings.GameTypeElements[gameType].SkillNameCorrectBounds.Height);
                    var result = GetSkillDataFromCountor(img, textCountor);
                    if (result != null)
                        skillDatas.Add(result);
                }
                grayImg.Dispose();
            }
            else if (dataType == GameDataType.SkillDetailView)
            {
                Mat hsvImage = new Mat();
                CvInvoke.CvtColor(img, hsvImage, ColorConversion.Bgr2Hsv);
                Mat txtImg = new Mat();
                CvInvoke.InRange(hsvImage, new ScalarArray(new MCvScalar(12, 63, 111)), new ScalarArray(new MCvScalar(13, 211, 206)), txtImg);
                VectorOfVectorOfPoint countors = new VectorOfVectorOfPoint();
                Mat hierarhy = new Mat();
                CvInvoke.FindContours(txtImg, countors, hierarhy, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int x = int.MaxValue, y = int.MaxValue;
                for (int i = 0; i < countors.Size; i++)
                {
                    using (VectorOfPoint vp = countors[i])
                    {
                        Rectangle rect = CvInvoke.BoundingRectangle(vp);
                        if (rect.Y < gamePart.Y)
                            continue;
                        if (rect.Height < 5)
                            continue;
                        if (rect.X < 50)
                            continue;
                        if (rect.X < x)
                            x = rect.X;
                        if (rect.Y < y)
                            y = rect.Y;
                    }
                }
                hierarhy.Dispose();
                countors.Dispose();
                txtImg.Dispose();
                hsvImage.Dispose();

                Rectangle textRect = new Rectangle(x - 4, y - 4, 270, 26);
                if (textRect.X < 0)
                    textRect.X = 0;
                if (textRect.Y < 0)
                    textRect.Y = 0;
                var result = GetSkillDataFromCountor(img, textRect);
                if (result != null)
                    skillDatas.Add(result);
            }
            return skillDatas;
        }


        private SkillManager.SkillData GetSkillDataFromCountor(Image<Bgr, byte> img, Rectangle textCountor)
        {
            SkillManager.SkillData skillData = null;
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

            var res = libraryManager.SkillLibrary.FindSkillByName(text, true, 0.8f);
            if (res.Count == 1)
                skillData = res[0].Value;
            else if (res.Count > 1)
            {
                var s = GetItemWithMaxConfidence<SkillManager.SkillData>(res);
                if (s != null)
                    skillData = s;
            }
            else if (res.Count == 0 && !string.IsNullOrWhiteSpace(text))
            {
                res = libraryManager.SkillLibrary.FindEventByNameDiceAlg(text);
                if (res.Count == 1)
                    skillData = res[0].Value;
                else if (res.Count > 1)
                {
                    var s = GetItemWithMaxConfidence<SkillManager.SkillData>(res);
                    if (s != null)
                        skillData = s;
                }
            }



            if (res.Count == 0 && !isNext)
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
            return skillData;
        }

        private T GetItemWithMaxConfidence<T>(List<KeyValuePair<float, T>> datas)
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
                return default(T);
            return datas[index].Value;
        }

        private string DeleteNumbers(string text)
        {
            string pattern = "[1234567890]";
            return Regex.Replace(text, pattern, "");
        }

        private EventManager.EventData GetEventData(Image<Bgr, byte> img, Rectangle partInfo)
        {
            int offset = 0;
            if (IsEventNameIcon(img.Mat))
            {
                offset = settings.GameTypeElements[gameType].EventNameIconBounds.Width + 7;
            }
            Mat mat = new Mat(img.Mat, new Rectangle(settings.GameTypeElements[gameType].EventNameBounds.X + offset, settings.GameTypeElements[gameType].EventNameBounds.Y,
                settings.GameTypeElements[gameType].EventNameBounds.Width, settings.GameTypeElements[gameType].EventNameBounds.Height));
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
            var eventData = libraryManager.EventLibrary.FindEventByName(text, true);
            if (eventData.Count == 0)
            {
                string text2 = Program.TessManager.GetTextSingleLine(threshMat);
                text2 = CorrectText(text2);
                eventData = libraryManager.EventLibrary.FindEventByName(text2, true);
            }
            if (eventData.Count == 0)
            {
                eventData = libraryManager.EventLibrary.FindEventByNameDiceAlg(text);
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
                text = EventManager.PrepareText(text);
                if (eventData.Count > 1)
                {
                    var res = eventData.Find(a => a.ContainsOptionDice(text, optionsConfidence));
                    //var res = eventData.Find(a => a.ContainsOption(text, true));
                    //if (res == null)
                    //{
                    //    res = eventData.Find(a => a.ContainsOptionDice(text));
                    //}
                    if (res == null)
                    {
                        //Поиск второго варианта
                        textRect.Y += 90;
                        text = GetOptionText(img.Mat, textRect, isNext);
                        text = CorrectText(text);
                        text = EventManager.PrepareText(text);
                        var res2 = eventData.Find(a => a.ContainsOptionDice(text, optionsConfidence));
                        //var res2 = eventData.Find(a => a.ContainsOption(text, true));
                        //if (res2 == null)
                        //{
                        //    res2 = eventData.Find(a => a.ContainsOptionDice(text));
                        //}
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
                    var res = libraryManager.EventLibrary.FindEventByOptionsDiceAlg(text, optionsConfidence);
                    //var res = library.FindEventByOption(text, true);
                    //if (res.Count == 0)
                    //{
                    //    res = library.FindEventByOptionsDiceAlg(text);
                    //}
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
                        text = EventManager.PrepareText(text);
                        res = libraryManager.EventLibrary.FindEventByOptionsDiceAlg(text, optionsConfidence);
                        //res = library.FindEventByOption(text, true);
                        //if (res.Count == 0)
                        //{
                        //    res = library.FindEventByOptionsDiceAlg(text);
                        //}
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
                        text = EventManager.PrepareText(text);
                        var res2 = res.Find(a => a.ContainsOptionDice(text, optionsConfidence));
                        //var res2 = res.Find(a => a.ContainsOption(text, true));
                        //if (res2 == null)
                        //{
                        //    res2 = eventData.Find(a => a.ContainsOptionDice(text));
                        //}
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

        private (GameDataType type, Rectangle PartInfo) DetectDataType(Image<Bgr, byte> img, bool IsVertical)
        {
            Image<Gray, byte> grayImg = img.Convert<Gray, byte>();
            double[] minVal, maxVal;
            Point[] minLoc, maxLoc;
            foreach (var item in settings.GameParts)
            {
                if (item.VerticalState != IsVertical)
                    continue;
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
                    grayImg.Dispose();
                    return (item.DataType, new Rectangle(maxLoc[0], item.Image.Size));
                }
            }
            grayImg.Dispose();
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

        private (int minVal, int maxVal) GetEventNameBackMinMax(Mat img)
        {
            Rectangle testBackRect = settings.GameTypeElements[gameType].EventNameIconBounds.GetRectangle();
            testBackRect.Height = 6;
            testBackRect.X += 35;
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            Mat backForTest = new Mat(img, testBackRect);
            CvInvoke.MinMaxLoc(backForTest, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            backForTest.Dispose();
            minVal -= 3;
            maxVal += 3;
            return ((int)minVal, (int)maxVal);

        }


        private bool IsTazunaHelpWarning(Mat img)
        {
            Rectangle testBackRect = settings.GameTypeElements[gameType].TazunaWarningTestRect.GetRectangle();
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            Mat backForTest = new Mat(img, testBackRect);
            CvInvoke.CvtColor(backForTest, backForTest, ColorConversion.Bgr2Gray);
            CvInvoke.MinMaxLoc(backForTest, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            backForTest.Dispose();
            double res = maxVal - minVal;
            if (res > 4)
                return true;
            else
                return false;
        }


        private bool IsEventNameIcon(Mat img)
        {
            bool result = false;
            Rectangle iconPartRect = settings.GameTypeElements[gameType].EventNameIconBounds.GetRectangle();
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
            TazunaAfterHelp = 6,
            GenWindow = 7,
            SkillDetailView = 8,
            FactorDetailView = 9,
            MissionBtn = 10,
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

        public struct TazunaHelpRelult
        {
            public string Desc;
            public string Warning;
        }
    }


}
