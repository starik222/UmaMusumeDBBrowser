using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace UmaMusumeDBBrowser
{
    public class LegendBuffControlManager
    {
        public List<Int64> BuffId;
        public List<TextBox> BuffNames;
        public List<TextBox> BuffTransNames;
        public List<TextBox> BuffDesc;
        public List<PictureBox> IconsRank;
        public List<PictureBox> IconsBuff;

        public LegendBuffControlManager()
        {
            BuffId = new List<Int64>();
            BuffNames = new List<TextBox>();
            BuffDesc = new List<TextBox>();
            BuffTransNames = new List<TextBox>();
            IconsRank = new List<PictureBox>();
            IconsBuff = new List<PictureBox>();
        }

        public int Count()
        {
            return BuffNames.Count;
        }

        public void AddItem(TextBox name, TextBox nameTrans,TextBox desc, PictureBox iconRank, PictureBox iconBuff)
        {
            BuffId.Add(-1);
            BuffNames.Add(name);
            BuffTransNames.Add(nameTrans);
            BuffDesc.Add(desc);
            IconsRank.Add(iconRank);
            IconsBuff.Add(iconBuff);
        }


        public void SetText(int index, string name, string desc)
        {
            if (index < 0 || index >= BuffNames.Count)
                throw new Exception("id out of range!");
            Extensions.SetTextToControl(BuffNames[index], name);
            Extensions.SetTextToControl(BuffDesc[index], desc);
        }

        public void SetText(int index, Int64 skId, string name, string desc)
        {
            if (index < 0 || index >= BuffNames.Count)
                throw new Exception("id out of range!");
            Extensions.SetTextToControl(BuffNames[index], name);
            Extensions.SetTextToControl(BuffDesc[index], desc);
            BuffId[index] = skId;
        }

        public void SetText(int index, Int64 skId, string name, string nameTrans, string desc, Image iconRank, Image iconBuff)
        {
            if (index < 0 || index >= BuffNames.Count)
                throw new Exception("id out of range!");
            Extensions.SetTextToControl(BuffNames[index], name);
            Extensions.SetTextToControl(BuffTransNames[index], nameTrans);
            Extensions.SetTextToControl(BuffDesc[index], desc);
            Extensions.SetImageToPicBox(IconsRank[index], iconRank);
            Extensions.SetImageToPicBox(IconsBuff[index], iconBuff);
            BuffId[index] = skId;
        }


        public (Int64 skId, string Name, string Desc) GetValue(int index)
        {
            if (index < 0 || index >= BuffNames.Count)
                throw new Exception("id out of range!");
            return (BuffId[index], BuffNames[index].Text, BuffDesc[index].Text);
        }

        public void SetBuffId(int index, Int64 id)
        {
            BuffId[index] = id;
        }
    }
}
