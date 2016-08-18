using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;


namespace Jovian.BigMap.classes
{
    /// <summary>
    /// LPY 2016-2-26 添加   
    /// 静态类
    /// 处理由客户端发来的参数合集，并处理（设置大屏上模块相关参数，并更新保存到本地）
    /// </summary>
    public static class ParamsHelper
    {
        private static int screenRaito = Convert.ToInt32(XmlHelper.GetValueByXPath(PublicParams.xmlFilePath, "/Root/ScreenRatio"));
        public static void ExecParamsFromJson(JObject json)
        {
            ExecActionByName(json["Name"].ToString(), Convert.ToDouble(json["Width"].ToString()), Convert.ToDouble(json["Height"].ToString()), Convert.ToDouble(json["X"].ToString()), Convert.ToDouble(json["Y"].ToString()), Convert.ToInt32(json["Zindex"].ToString()), Convert.ToInt32(json["FontSize"].ToString()));
            
        }

        public static void ExecSaveParamsFromJson(JObject json)
        {
            try
            {
                RemoteWin[] rws = (RemoteWin[])JsonHelper.FromJson(typeof(RemoteWin[]), Encoding.Default.GetString(Convert.FromBase64String(json["Modulars"].ToString())));
                XmlHelper.UpdateModularsByName(PublicParams.xmlFilePath, "/Root/Windows", rws);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog( ex.Message);
            }
        }

        

        //配置文件不存在时，新建配置文件：position.xml
        private static void CreateXMLFile()
        {
            XmlDocument dom = new XmlDocument();
            XmlElement xe = dom.CreateElement("Root");
            dom.AppendChild(xe);
            dom.Save("params.xml");
        }


        public static void ReloadModularsFromXML()
        {
            RemoteWin[] Modulars = XmlHelper.GetModularsByXPath(PublicParams.xmlFilePath, "/Root/Windows");
            
            foreach (RemoteWin rw in Modulars)
            {
                ExecActionByName(rw.Name, rw.Width * screenRaito, rw.Height * screenRaito, rw.X * screenRaito, rw.Y * screenRaito,rw.Zindex, rw.FontSize);
            }
        }


        public static void SetPadPositionAndSize(UserControl uc, double width, double height, double x, double y, int fontsize)
        {
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() => {
                uc.Width = width; uc.Height = height;
                Thickness thickness = new Thickness(x, y, 0, 0);
                uc.Margin = thickness;
            }));
        }

        public static void ExecActionByName(string name,double width,double height,double x,double y,int zindex,int fontsize)
        {
            UserControl uc = null;
            switch (name)
            {
                case "警情动态":
                    uc = PublicParams.padCaseInfo;
                    break;
                case "动态视频":
                    uc = PublicParams.padVideos;
                    PublicParams.videoPadHeight = height;
                    PublicParams.videoPadWidth = width;
                    break;
                default:
                    break;
            }
            if (uc!=null)
            {
                uc.BeginAnimation(UserControl.WidthProperty, null);
                uc.BeginAnimation(UserControl.HeightProperty, null);
                SetPadPositionAndSize(uc, width,height,x,y,fontsize);
                Canvas.SetZIndex(uc, zindex);
            }
        }


        internal static void ExecParamsAndNameFromJson(JObject json)
        {
            //RemoteWin rw = new RemoteWin(Convert.ToDouble(json["Width"].ToString()), Convert.ToDouble(json["Height"].ToString()), Convert.ToDouble(json["X"].ToString()), Convert.ToDouble(json["Y"].ToString()), Convert.ToInt32(json["Zindex"].ToString()),json["NewName"].ToString(), Convert.ToInt32(json["FontSize"].ToString()),"");
            //XmlHelper.UpdateModularByName(PublicParams.xmlFilePath, "/Root/Windows", json["OldName"].ToString(), rw);
        }
    }
}
