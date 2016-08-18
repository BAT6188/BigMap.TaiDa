using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jovian.BigMap.classes
{
    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Symbols;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// LPY 2016-4-12 添加
    /// 为定制Clusterer的外观
    /// </summary>
    public class SumClusterer : GraphicsClusterer
    {
        public string AggregateColumn { get; set; }
        public double SymbolScale { get; set; }

        public SumClusterer()
        {
            SymbolScale = 1;
            //base.Radius = 50;
        }

        protected override Graphic OnCreateGraphic(GraphicCollection cluster, MapPoint center, int maxClusterCount)
        {
            if (cluster.Count == 1)
                return cluster[0];

            Graphic graphic = null;
            double sum = cluster.Count;
            double size = 100;//(Math.Log(sum * SymbolScale / 10) * 10 + 20);
            Brush color = new SolidColorBrush(Colors.Transparent);

            graphic = new Graphic() { Symbol = new ClusterSymbol() { Size = size }, Geometry = center };

            GetSizeAndColorByCount(sum, ref size, ref color);

            graphic.Attributes.Add("Count", sum);
            graphic.Attributes.Add("Size", size);
            graphic.Attributes.Add("Color", color); //InterpolateColor(size - 12, 100));
            return graphic;
        }

        /// <summary>
        /// LPY 2016-4-13 添加
        /// 根据聚合数量，得到符号大小和颜色
        /// </summary>
        /// <param name="count">聚合数量</param>
        /// <param name="size">符号大小</param>
        /// <param name="color">颜色</param>
        private static void GetSizeAndColorByCount(double count, ref double size, ref Brush color)
        {
            if (count >= 0 && count <= 5)
            {
                size = 100; color = new SolidColorBrush(Color.FromArgb(200, 0x00, 0xa8, 0x84));
            }
            else if (count >= 6 && count <= 15)
            {
                size = 110; color = new SolidColorBrush(Color.FromArgb(200, 0x00, 0x90, 0x4a));
            }
            else if (count >= 16 && count <= 35)
            {
                size = 120; color = new SolidColorBrush(Color.FromArgb(200, 0x7b, 0xc9, 0x38));
            }
            else if (count >= 36 && count <= 55)
            {
                size = 130; color = new SolidColorBrush(Color.FromArgb(200, 0xfe, 0xf8, 0x02));
            }
            else if (count >= 56 && count <= 155)
            {
                size = 140; color = new SolidColorBrush(Color.FromArgb(200, 0xeb, 0xb7, 0x01));
            }
            else if (count >= 156 && count <= 555)
            {
                size = 150; color = new SolidColorBrush(Color.FromArgb(200, 0xf2, 0x96, 0x31));
            }
            else if (count >= 556 && count <= 1555)
            {
                size = 160; color = new SolidColorBrush(Color.FromArgb(200, 0xf3, 0x80, 0x01));
            }
            else
            {
                size = 180; color = new SolidColorBrush(Color.FromArgb(200, 0xeb, 0x3d, 0x00));
            }
        }
        //private static Brush InterpolateColor(double value, double max)
        //{
        //    value = (int)Math.Round(value * 255.0 / max);
        //    if (value > 255)
        //        value = 255;
        //    else if (value < 0)
        //        value = 0;

        //    return new SolidColorBrush(Color.FromArgb(255, (byte)(value + 240 > 255 ? value+240-255 : value + 240), (byte)(value + 100 > 255 ? value+100-255 : value + 100), (byte)value));
        //}
    }

    internal class ClusterSymbol : MarkerSymbol
    {
        public ClusterSymbol()
        {
            string template = "<ControlTemplate " +
            "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"" + ">" +
            @"<Grid IsHitTestVisible=""False"">
            <Ellipse
                Fill=""{Binding Attributes[Color]}"" 
                Width=""{Binding Attributes[Size]}""
                Height=""{Binding Attributes[Size]}"" />
            <Grid HorizontalAlignment=""Center"" VerticalAlignment=""Center"">
            <TextBlock 
                Text=""{Binding Attributes[Count]}"" 
                FontSize=""48"" Margin=""1,1,0,0"" FontWeight=""Bold""
                Foreground=""#99000000"" />
            <TextBlock
                Text=""{Binding Attributes[Count]}"" 
                FontSize=""48"" Margin=""0,0,1,1"" FontWeight=""Bold""
                Foreground=""White"" />
            </Grid>
            </Grid>
            </ControlTemplate>";

            System.IO.MemoryStream templateStream = new System.IO.MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(template));
            ControlTemplate = System.Windows.Markup.XamlReader.Load(templateStream) as ControlTemplate;
        }

        public double Size { get; set; }

        public override double OffsetX
        {
            get
            {
                return Size / 2;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public override double OffsetY
        {
            get
            {
                return Size / 2;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
