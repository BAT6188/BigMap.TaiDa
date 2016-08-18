using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jovian.BigMap.parts
{
    using Jovian.BigMap.classes;
    using System.Windows.Controls.Primitives;
    /// <summary>
    /// PadVideos.xaml 的交互逻辑
    /// </summary>
    public partial class PadVideos : UserControl
    {
        public PadVideos()
        {
            InitializeComponent();
        }

        public static void ShowHideTextByTitle(string title,int id, string _switch)
        {
            try
            {
                switch (_switch)
                {
                    case "1"://show title
                        (PublicParams.padVideos.LayoutRoot.FindName("tb" + id.ToString()) as TextBlock).Text = title;

                        break;
                    case "0":
                        (PublicParams.padVideos.LayoutRoot.FindName("tb" + id.ToString()) as TextBlock).Text = "";
                        break;
                    default:
                        break;
                }
                
            }
            catch (Exception)
            {
                
            }            
        }

        /// <summary>
        /// LPY 2016-3-30 添加
        /// 清空视频板上的标题
        /// </summary>
        public static void ClearPadTitles()
        {
            foreach (var control in (PublicParams.padVideos.LayoutRoot.Children[0] as UniformGrid).Children)
            {
                if (control.GetType() == typeof(TextBlock))
                {
                    (control as TextBlock).Text = "";
                }
            }
        }
    }
}
