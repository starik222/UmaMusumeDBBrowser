using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace UmaMusumeDBBrowser
{
    public class SkillControlManager
    {
        public List<Int64> SkillId;
        public List<TextBox> SkillNames;
        public List<TextBox> SkillTransNames;
        public List<TextBox> SkillDesc;
        public List<PictureBox> Icons;

        public SkillControlManager()
        {
            SkillId = new List<Int64>();
            SkillNames = new List<TextBox>();
            SkillDesc = new List<TextBox>();
            SkillTransNames = new List<TextBox>();
            Icons = new List<PictureBox>();
        }

        public int Count()
        {
            return SkillNames.Count;
        }

        public void AddItem(TextBox name, TextBox nameTrans,TextBox desc, PictureBox icon)
        {
            SkillId.Add(-1);
            SkillNames.Add(name);
            SkillTransNames.Add(nameTrans);
            SkillDesc.Add(desc);
            Icons.Add(icon);
        }


        public void SetText(int index, string name, string desc)
        {
            if (index < 0 || index >= SkillNames.Count)
                throw new Exception("id out of range!");
            Extensions.SetTextToControl(SkillNames[index], name);
            Extensions.SetTextToControl(SkillDesc[index], desc);
        }

        public void SetText(int index, Int64 skId, string name, string desc)
        {
            if (index < 0 || index >= SkillNames.Count)
                throw new Exception("id out of range!");
            Extensions.SetTextToControl(SkillNames[index], name);
            Extensions.SetTextToControl(SkillDesc[index], desc);
            SkillId[index] = skId;
        }

        public void SetText(int index, Int64 skId, string name, string nameTrans, string desc, Image icon)
        {
            if (index < 0 || index >= SkillNames.Count)
                throw new Exception("id out of range!");
            Extensions.SetTextToControl(SkillNames[index], name);
            Extensions.SetTextToControl(SkillTransNames[index], nameTrans);
            Extensions.SetTextToControl(SkillDesc[index], desc);
            Extensions.SetImageToPicBox(Icons[index], icon);
            SkillId[index] = skId;
        }


        public (Int64 skId, string Name, string Desc) GetValue(int index)
        {
            if (index < 0 || index >= SkillNames.Count)
                throw new Exception("id out of range!");
            return (SkillId[index], SkillNames[index].Text, SkillDesc[index].Text);
        }

        public void SetSkillId(int index, Int64 id)
        {
            SkillId[index] = id;
        }
    }
}
