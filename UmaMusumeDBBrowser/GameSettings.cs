using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Schema;
using System.Drawing;

namespace UmaMusumeDBBrowser
{
    public class GameSettings
    {
        public GameSize GameNormalSize { get; set; }
        public BSPanel BlueStacksPanel { get; set; }
        public GameElements GameTypeElements { get; set; }
        public List<GamePart> GameParts { get; set; }


        public class GameSize
        {
            public Size Vertical { get; set; }
            public Size Horizontal { get; set; }
        }

        public class BSPanel
        {
            public Rect Ver4 { get; set; }
            public Rect Ver5 { get; set; }
        }

        public class GameElements
        {
            public Elements this[GameReader.GameType type]
            {
                get
                {
                    if (type == GameReader.GameType.DMM)
                        return DmmElements;
                    else
                        return MobileElements;
                }
            }
            public Elements DmmElements { get; set; }
            public Elements MobileElements { get; set; }
        }

        public class Elements
        {
            
            public Rect UmaMusumeSubNameBounds { get; set; }
            public Rect UmaMusumeNameBounds { get; set; }
            public Rect CurrentTurnBounds { get; set; }
            public Rect EventCategoryBounds { get; set; }
            public Rect EventNameBounds { get; set; }
            public Rect EventNameIconBounds { get; set; }
            public Rect EventBottomOptionBounds { get; set; }
            public Rect CurrentMenuBounds { get; set; }
            public Rect BackButtonBounds { get; set; }
            public Rect SkillListWindow { get; set; }
            public Rect LegendBuffListWindow { get; set; }
            public Rect SkillNameCorrectBounds { get; set; }
            public Rect LegendBuffNameCorrectBounds { get; set; }
            public Rect TazunaAfterHelpWindow { get; set; }
            public Rect TazunaWarningTestRect { get; set; }
            public Rect TazunaWarningWindow { get; set; }

        }

        public class GamePart
        {
            public string PartName { get; set; }
            public bool VerticalState { get; set; }
            public GameReader.GameDataType DataType { get; set; }
            public string ImageName { get; set; }
            public Emgu.CV.Image<Emgu.CV.Structure.Gray, byte> Image { get; set; } = null;
            public List<GamePart> SubGameParts { get; set; }
        }

        public class Size
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public override string ToString()
            {
                return $"Width: {Width}, Height: {Height}";
            }
        }

        public class Rect
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public override string ToString()
            {
                return $"X: {X}, Y: {Y}, Width: {Width}, Height: {Height}";
            }

            public Rectangle GetRectangle()
            {
                return new Rectangle(X, Y, Width, Height);
            }
        }
    }
}
