using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using System.IO;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;
    /// <summary>
    /// LPY 2015-9-12 添加
    /// 数据转换类，XAML文档中的数据转换，包括数据计算、根据文件名转换成图片路径等
    /// </summary>
    class DataConverter
    {
        
    }
    //LPY 2015-9-12 添加 根据字符串转换成图片所在路径
    public class ImagePathConverter : IValueConverter
    {
        private string imageDirectory = Directory.GetCurrentDirectory() + "\\images\\";
        public string ImageDirectory
        {
            get { return imageDirectory; }
            set { imageDirectory = value; }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string imagePath = Path.Combine(ImageDirectory, value.ToString());
            return new BitmapImage(new Uri(imagePath));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;//throw new NotSupportedException();
        }
    }
}
