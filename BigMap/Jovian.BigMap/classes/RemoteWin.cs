using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    /// <summary>
    /// LPY 2016-3-11 添加
    /// 远程窗口类（大屏窗口）
    /// </summary>
    public class RemoteWin
    {
        private double width;
        private double height;
        private double x;
        private double y;
        private int zindex;
        private string name;
        private int fontsize;
        private string img;

        public double Width { set { width = value; } get { return width; } }
        public double Height { set { height = value; } get { return height; } }
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public int Zindex { get { return zindex; } set { zindex = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int FontSize { get { return fontsize; } set { fontsize = value; } }
        public string Img { get { return img; } set { img = value; } }

        public RemoteWin()
        {
            width = 192; height = 108; x = 0; y = 0; zindex = 0; name = "未命名窗口";
        }

        public RemoteWin(double width, double height, double x, double y, int zindex, string name, int fontsize, string img)
        {
            this.width = width; this.height = height; this.x = x; this.y = y; this.zindex = zindex; this.name = name; this.fontsize = fontsize; this.img = img;
        }
    }
}
