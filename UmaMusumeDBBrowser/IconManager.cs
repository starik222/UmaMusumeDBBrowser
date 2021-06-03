using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace UmaMusumeDBBrowser
{
    public class IconManager
    {
        private Dictionary<string, Dictionary<string, Image>> imagesDictonary;
        private string appPath;
        public IconManager(string AppPath)
        {
            appPath = AppPath;
            imagesDictonary = new Dictionary<string, Dictionary<string, Image>>();
        }

        private void LoadImages(string path)
        {
            string fullPath = Path.Combine(appPath, path);
            FileInfo[] fi = new DirectoryInfo(fullPath).GetFiles("*.png", SearchOption.TopDirectoryOnly);
            Dictionary<string, Image> imageList = new Dictionary<string, Image>();

            if (imagesDictonary.ContainsKey(path))
            {
                imageList = imagesDictonary[path];
            }
            else
                imagesDictonary.Add(path, imageList);

            foreach (var item in fi)
            {
                imageList.Add(Path.GetFileNameWithoutExtension(item.Name), Image.FromFile(item.FullName));
            }
            
        }

        public Image GetImageByKey(string path, string key)
        {
            if (!imagesDictonary.ContainsKey(path))
                LoadImages(path);
            if (imagesDictonary[path].ContainsKey(key))
                return imagesDictonary[path][key];
            else
                return null;
        }




    }
}
