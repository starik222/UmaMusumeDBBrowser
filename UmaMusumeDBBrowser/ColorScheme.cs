using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

namespace UmaMusumeDBBrowser
{
    public class ColorSchemeManager
    {
        public List<ColorScheme> Items { get; set; }

        private int selectedSchemeIndex = -1;


        public delegate void CSData(object sender, EventArgs e);
        public event CSData SchemeChanded;



        public ColorScheme SelectedScheme
        {
            get
            {
                if (selectedSchemeIndex != -1)
                    return Items[selectedSchemeIndex];
                else
                    return null;
            }
        }

        public ColorSchemeManager()
        {
            Items = new List<ColorScheme>();
        }

        public void SelectScheme(string name)
        {
            int index = Items.FindIndex(a => a.SchemeName.Equals(name));
            if (index != selectedSchemeIndex)
            {
                selectedSchemeIndex = index;
                SchemeChanded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Load(string path)
        {
            var res = JsonConvert.DeserializeObject<ColorSchemeManager>(File.ReadAllText(path));
            Items = new List<ColorScheme>(res.Items);
            res.Items.Clear();
        }

        public void Save(string savePath)
        {
            File.WriteAllText(savePath, JsonConvert.SerializeObject(this));
        }

        public void ChangeColorScheme(Control ctrl, ColorScheme scheme)
        {
            if (ctrl is Button)
            {
                ((Button)ctrl).BackColor = scheme.ButtonStyle.BackColor;
                ((Button)ctrl).ForeColor = scheme.ButtonStyle.ForeColor;
            }
            else if (ctrl is TextBox || ctrl is RichTextBox)
            {
                ((TextBoxBase)ctrl).BackColor = scheme.TextBoxStyle.BackColor;
                ((TextBoxBase)ctrl).ForeColor = scheme.TextBoxStyle.ForeColor;
            }
            else if (ctrl is Form)
            {
                ((Form)ctrl).BackColor = scheme.FormStyle.BackColor;
                ((Form)ctrl).ForeColor = scheme.FormStyle.ForeColor;
            }
            else if (ctrl is DataGridView)
            {
                ((DataGridView)ctrl).DefaultCellStyle.BackColor = scheme.FormStyle.BackColor;
                ((DataGridView)ctrl).DefaultCellStyle.ForeColor = scheme.FormStyle.ForeColor;

                ((DataGridView)ctrl).BackgroundColor = scheme.GrigStyle.GridBackColor;
                ((DataGridView)ctrl).BackColor = scheme.FormStyle.BackColor;
                ((DataGridView)ctrl).ForeColor = scheme.FormStyle.ForeColor;

                ((DataGridView)ctrl).RowHeadersDefaultCellStyle.BackColor = scheme.FormStyle.BackColor;
                ((DataGridView)ctrl).RowHeadersDefaultCellStyle.ForeColor = scheme.FormStyle.ForeColor;

                ((DataGridView)ctrl).ColumnHeadersDefaultCellStyle.BackColor = scheme.FormStyle.BackColor;
                ((DataGridView)ctrl).ColumnHeadersDefaultCellStyle.ForeColor = scheme.FormStyle.ForeColor;
                ((DataGridView)ctrl).EnableHeadersVisualStyles = false;
            }
            else if (ctrl is TabPage)
            {
                ((TabPage)ctrl).BackColor = scheme.TabControlStyle.BackColor;
                ((TabPage)ctrl).ForeColor = scheme.TabControlStyle.ForeColor;

                if (((TabPage)ctrl).BackColor == Color.Transparent || ((TabPage)ctrl).BackColor == SystemColors.Window)
                {
                    ((TabControl)((TabPage)ctrl).Parent).DrawMode = TabDrawMode.Normal;
                }
                else
                    ((TabControl)((TabPage)ctrl).Parent).DrawMode = TabDrawMode.OwnerDrawFixed;
            }
            else if (ctrl is ToolStrip)
            {
                ((ToolStrip)ctrl).BackColor = scheme.ToolStripStyle.BackColor;
                ((ToolStrip)ctrl).ForeColor = scheme.ToolStripStyle.ForeColor;
            }
            else if (ctrl is Label)
            {
                ((Label)ctrl).BackColor = scheme.LabelStyle.BackColor;
                ((Label)ctrl).ForeColor = scheme.LabelStyle.ForeColor;
            }
            else
            {
                ctrl.BackColor = scheme.OtherStyle.BackColor;
                ctrl.ForeColor = scheme.OtherStyle.ForeColor;
            }
        }

        public void ChangeColorSchemeInConteiner(Control.ControlCollection collection, ColorScheme scheme)
        {
            foreach (Control item in collection)
            {
                if (item.Controls.Count > 0)
                {
                    ChangeColorSchemeInConteiner(item.Controls, scheme);
                }
                ChangeColorScheme(item, scheme);
            }
        }


        public class ColorScheme
        {
            public string SchemeName { get; set; }
            public GenericColorData ButtonStyle { get; set; }
            public GenericColorData TextBoxStyle { get; set; }
            public GenericColorData FormStyle { get; set; }
            public GridColorData GrigStyle { get; set; }
            public GenericColorData TabControlStyle { get; set; }
            public GenericColorData ToolStripStyle { get; set; }
            public GenericColorData LabelStyle { get; set; }
            public GenericColorData OtherStyle { get; set; }

            public override string ToString()
            {
                return SchemeName;
            }


            public static ColorScheme GetDefaultColorScheme()
            {
                ColorScheme scheme = new ColorScheme();
                scheme.SchemeName = "Classic";
                scheme.ButtonStyle = new GenericColorData() { BackColor = Color.Transparent, ForeColor = SystemColors.ControlText };
                scheme.TextBoxStyle = new GenericColorData() { BackColor = SystemColors.Window, ForeColor = SystemColors.ControlText };
                scheme.FormStyle = new GenericColorData() { BackColor = SystemColors.Window, ForeColor = SystemColors.ControlText };
                scheme.GrigStyle = new GridColorData() { BackColor = SystemColors.Window, ForeColor = SystemColors.ControlText, GridBackColor = SystemColors.ControlDark };
                scheme.TabControlStyle = new GenericColorData() { BackColor = SystemColors.Window, ForeColor = SystemColors.ControlText };
                scheme.ToolStripStyle = new GenericColorData() { BackColor = SystemColors.Control, ForeColor = SystemColors.ControlText };
                scheme.LabelStyle = new GenericColorData() { BackColor = Color.Transparent, ForeColor = SystemColors.ControlText };
                scheme.OtherStyle = new GenericColorData() { BackColor = SystemColors.Window, ForeColor = SystemColors.ControlText };
                return scheme;
            }

            public static ColorScheme GetDarkDefaultColorScheme()
            {
                ColorScheme scheme = new ColorScheme();
                scheme.SchemeName = "Dark";
                scheme.ButtonStyle = new GenericColorData() { BackColor = Color.Black, ForeColor = Color.White };
                scheme.TextBoxStyle = new GenericColorData() { BackColor = Color.Black, ForeColor = Color.White };
                scheme.FormStyle = new GenericColorData() { BackColor = Color.Black, ForeColor = Color.White };
                scheme.GrigStyle = new GridColorData() { BackColor = Color.Black, ForeColor = Color.White, GridBackColor = Color.Black };
                scheme.TabControlStyle = new GenericColorData() { BackColor = Color.Black, ForeColor = Color.White };
                scheme.ToolStripStyle = new GenericColorData() { BackColor = Color.Black, ForeColor = Color.White };
                scheme.LabelStyle = new GenericColorData() { BackColor = Color.Black, ForeColor = Color.White };
                scheme.OtherStyle = new GenericColorData() { BackColor = Color.Black, ForeColor = Color.White };
                
                return scheme;
            }
        }

        public class GenericColorData
        {
            public Color BackColor { get; set; }
            public Color ForeColor { get; set; }
        }

        public class GridColorData : GenericColorData
        {
            public Color GridBackColor { get; set; }
        }
    }
}
