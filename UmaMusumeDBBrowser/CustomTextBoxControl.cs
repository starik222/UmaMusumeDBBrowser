using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;

namespace UmaMusumeDBBrowser
{
    public class CustomRichTextBox : RichTextBox
    {
        private List<KeyValuePair<Range, string>> links;
        private bool isProcessed = false;
        public CustomRichTextBox() : base()
        {
            links = new List<KeyValuePair<Range, string>>();
            //base.LinkClicked += CustomRichTextBox_LinkClicked;
            base.Click += CustomRichTextBox_Click;
            base.ImeMode = ImeMode.Off;
            base.ImeModeBase = ImeMode.Off;
            //base.DefaultImeMode = ImeMode.Off;
        }

        private void CustomRichTextBox_Click(object sender, EventArgs e)
        {
            //if (base.SelectionColor == Color.Blue && base.SelectionFont.Style == FontStyle.Underline)
            //{
            //    base.OnLinkClicked(new LinkClickedEventArgs("a"));
            //}

            var res = links.FindIndex(a => a.Key.Start <= base.SelectionStart && a.Key.End >= base.SelectionStart);
            if (res != -1)
            {
                base.OnLinkClicked(new LinkClickedEventArgs(links[res].Value));
            }
        }

        private void CustomRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                if (!isProcessed)
                {
                    links.Clear();
                    string notFormattedText = base.Text;
                    if (notFormattedText.Contains('【'))
                    {
                        int startPos = 0;
                        while (startPos != -1)
                        {
                            startPos = SelectLink('【', '】', startPos);
                        }
                    }

                    if (notFormattedText.Contains('「'))
                    {
                        int startPos = 0;
                        while (startPos != -1)
                        {
                            startPos = SelectLink('「', '」', startPos);
                        }
                    }
                    if (notFormattedText.Contains('『'))
                    {
                        int startPos = 0;
                        while (startPos != -1)
                        {
                            startPos = SelectLink('『', '』', startPos);
                        }
                    }
                }


            }
        }

        private int SelectLink(char startChar, char endChar, int startPos = 0)
        {
            isProcessed = true;
            int startCharPos = base.Text.IndexOf(startChar, startPos);
            if (startCharPos == -1)
            {
                isProcessed = false;
                return -1;
            }
            int endCharPos = base.Text.IndexOf(endChar, startCharPos);
            if (endCharPos == -1)
            {
                isProcessed = false;
                return -1;
            }
            base.Select(startCharPos + 1, endCharPos - startCharPos-1);
            base.SelectionColor = Color.Blue;
            if (base.SelectionFont != null)
                base.SelectionFont = new Font(base.SelectionFont, FontStyle.Underline);
            else
                base.SelectionFont = new Font(base.Font, FontStyle.Underline);
            var link = new KeyValuePair<Range, string>(new Range(startCharPos + 1, endCharPos), base.SelectedText);
            if (!links.Contains(link))
                links.Add(new KeyValuePair<Range, string>(new Range(startCharPos + 1, endCharPos), base.SelectedText));
            isProcessed = false;
            return endCharPos;

        }

        public void SetSelectionLineSpacing(byte bLineSpacingRule, int dyLineSpacing)
        {
            PARAFORMAT2 format = new PARAFORMAT2();
            format.cbSize = Marshal.SizeOf(format);
            format.dwMask = PFM_LINESPACING;
            format.dyLineSpacing = dyLineSpacing;
            format.bLineSpacingRule = bLineSpacingRule;
            SendMessage(this.Handle, EM_SETPARAFORMAT, SCF_SELECTION, ref format);
        }


        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, ref CHARFORMAT2 lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, ref PARAFORMAT2 lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PARAFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public Int16 wNumbering;
            public Int16 wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public Int16 wAlignment;
            public Int16 cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public Int16 sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public Int16 wShadingWeight;
            public Int16 wShadingStyle;
            public Int16 wNumberingStart;
            public Int16 wNumberingStyle;
            public Int16 wNumberingTab;
            public Int16 wBorderSpace;
            public Int16 wBorderWidth;
            public Int16 wBorders;
        }
        //public void ClearAllFormatting(Font font)
        //{
        //    CHARFORMAT2 fmt = new CHARFORMAT2();

        //    fmt.cbSize = Marshal.SizeOf(fmt);
        //    fmt.dwMask = CFM_ALL2;
        //    fmt.dwEffects = CFE_AUTOCOLOR | CFE_AUTOBACKCOLOR | CFE_FONTBOUND;
        //    fmt.szFaceName = font.FontFamily.Name;

        //    double size = font.Size;
        //    size /= 72;//logical dpi (pixels per inch)
        //    size *= 1440.0;//twips per inch

        //    fmt.yHeight = (int)size;//165
        //    fmt.yOffset = 0;
        //    fmt.crTextColor = 0;
        //    fmt.bCharSet = 128;// DEFAULT_CHARSET;
        //    fmt.bPitchAndFamily = 0;// DEFAULT_PITCH;
        //    fmt.wWeight = 400;// FW_NORMAL;
        //    fmt.sSpacing = 0;
        //    fmt.crBackColor = 0;
        //    //fmt.lcid = ???
        //    fmt.dwMask &= ~CFM_LCID;//don't know how to get this...
        //    fmt.dwReserved = 0;
        //    fmt.sStyle = 0;
        //    fmt.wKerning = 0;
        //    fmt.bUnderlineType = 0;
        //    fmt.bAnimation = 0;
        //    fmt.bRevAuthor = 0;
        //    fmt.bReserved1 = 0;

        //    SendMessage(this.Handle, EM_SETCHARFORMAT, SCF_ALL, ref fmt);
        //}
        private const int PFM_LINESPACING = 256;
        private const int EM_SETPARAFORMAT = 1095;

        private const UInt32 WM_USER = 0x0400;
        private const UInt32 EM_GETCHARFORMAT = (WM_USER + 58);
        private const UInt32 EM_SETCHARFORMAT = (WM_USER + 68);
        private const UInt32 SCF_ALL = 0x0004;
        private const UInt32 SCF_SELECTION = 0x0001;



        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        private struct CHARFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public uint dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szFaceName;
            public short wWeight;
            public short sSpacing;
            public int crBackColor;
            public int lcid;
            public int dwReserved;
            public short sStyle;
            public short wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        #region CFE_
        // CHARFORMAT effects 
        private const UInt32 CFE_BOLD = 0x0001;
        private const UInt32 CFE_ITALIC = 0x0002;
        private const UInt32 CFE_UNDERLINE = 0x0004;
        private const UInt32 CFE_STRIKEOUT = 0x0008;
        private const UInt32 CFE_PROTECTED = 0x0010;
        private const UInt32 CFE_LINK = 0x0020;
        private const UInt32 CFE_FONTBOUND = 0x00100000;
        private const UInt32 CFE_AUTOCOLOR = 0x40000000;            // NOTE: this corresponds to 
                                                                    // CFM_COLOR, which controls it 
                                                                    // Masks and effects defined for CHARFORMAT2 -- an (*) indicates
                                                                    // that the data is stored by RichEdit 2.0/3.0, but not displayed
        private const UInt32 CFE_SMALLCAPS = CFM_SMALLCAPS;
        private const UInt32 CFE_ALLCAPS = CFM_ALLCAPS;
        private const UInt32 CFE_HIDDEN = CFM_HIDDEN;
        private const UInt32 CFE_OUTLINE = CFM_OUTLINE;
        private const UInt32 CFE_SHADOW = CFM_SHADOW;
        private const UInt32 CFE_EMBOSS = CFM_EMBOSS;
        private const UInt32 CFE_IMPRINT = CFM_IMPRINT;
        private const UInt32 CFE_DISABLED = CFM_DISABLED;
        private const UInt32 CFE_REVISED = CFM_REVISED;

        // CFE_AUTOCOLOR and CFE_AUTOBACKCOLOR correspond to CFM_COLOR and
        // CFM_BACKCOLOR, respectively, which control them
        private const UInt32 CFE_AUTOBACKCOLOR = CFM_BACKCOLOR;
        #endregion
        #region CFM_
        // CHARFORMAT masks 
        private const UInt32 CFM_BOLD = 0x00000001;
        private const UInt32 CFM_ITALIC = 0x00000002;
        private const UInt32 CFM_UNDERLINE = 0x00000004;
        private const UInt32 CFM_STRIKEOUT = 0x00000008;
        private const UInt32 CFM_PROTECTED = 0x00000010;
        private const UInt32 CFM_LINK = 0x00000020;         // Exchange hyperlink extension 
        private const UInt32 CFM_SIZE = 0x80000000;
        private const UInt32 CFM_COLOR = 0x40000000;
        private const UInt32 CFM_FACE = 0x20000000;
        private const UInt32 CFM_OFFSET = 0x10000000;
        private const UInt32 CFM_CHARSET = 0x08000000;

        private const UInt32 CFM_SMALLCAPS = 0x0040;            // (*)  
        private const UInt32 CFM_ALLCAPS = 0x0080;          // Displayed by 3.0 
        private const UInt32 CFM_HIDDEN = 0x0100;           // Hidden by 3.0 
        private const UInt32 CFM_OUTLINE = 0x0200;          // (*)  
        private const UInt32 CFM_SHADOW = 0x0400;           // (*)  
        private const UInt32 CFM_EMBOSS = 0x0800;           // (*)  
        private const UInt32 CFM_IMPRINT = 0x1000;          // (*)  
        private const UInt32 CFM_DISABLED = 0x2000;
        private const UInt32 CFM_REVISED = 0x4000;

        private const UInt32 CFM_BACKCOLOR = 0x04000000;
        private const UInt32 CFM_LCID = 0x02000000;
        private const UInt32 CFM_UNDERLINETYPE = 0x00800000;        // Many displayed by 3.0 
        private const UInt32 CFM_WEIGHT = 0x00400000;
        private const UInt32 CFM_SPACING = 0x00200000;      // Displayed by 3.0 
        private const UInt32 CFM_KERNING = 0x00100000;      // (*)  
        private const UInt32 CFM_STYLE = 0x00080000;        // (*)  
        private const UInt32 CFM_ANIMATION = 0x00040000;        // (*)  
        private const UInt32 CFM_REVAUTHOR = 0x00008000;

        private const UInt32 CFE_SUBSCRIPT = 0x00010000;        // Superscript and subscript are 
        private const UInt32 CFE_SUPERSCRIPT = 0x00020000;      //  mutually exclusive           

        private const UInt32 CFM_SUBSCRIPT = (CFE_SUBSCRIPT | CFE_SUPERSCRIPT);
        private const UInt32 CFM_SUPERSCRIPT = CFM_SUBSCRIPT;

        // CHARFORMAT "ALL" masks
        private const UInt32 CFM_EFFECTS = (CFM_BOLD | CFM_ITALIC | CFM_UNDERLINE | CFM_COLOR |
                             CFM_STRIKEOUT | CFE_PROTECTED | CFM_LINK);
        private const UInt32 CFM_ALL = (CFM_EFFECTS | CFM_SIZE | CFM_FACE | CFM_OFFSET | CFM_CHARSET);

        private const UInt32 CFM_EFFECTS2 = (CFM_EFFECTS | CFM_DISABLED | CFM_SMALLCAPS | CFM_ALLCAPS
                            | CFM_HIDDEN | CFM_OUTLINE | CFM_SHADOW | CFM_EMBOSS
                            | CFM_IMPRINT | CFM_DISABLED | CFM_REVISED
                            | CFM_SUBSCRIPT | CFM_SUPERSCRIPT | CFM_BACKCOLOR);

        private const UInt32 CFM_ALL2 = (CFM_ALL | CFM_EFFECTS2 | CFM_BACKCOLOR | CFM_LCID
                            | CFM_UNDERLINETYPE | CFM_WEIGHT | CFM_REVAUTHOR
                            | CFM_SPACING | CFM_KERNING | CFM_STYLE | CFM_ANIMATION);
        #endregion


    }
}
