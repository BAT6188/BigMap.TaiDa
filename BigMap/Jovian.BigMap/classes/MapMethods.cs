using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using System.Threading;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Graphics;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Symbols;
    using ESRI.ArcGIS.Client.Toolkit;
    using ESRI.ArcGIS.Client.Tasks.Utils.JSON;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Windows;

    /// <summary>
    /// LPY 2015-9-7 添加
    /// 提供对地图的各种静态操作方法：切换地图、放大缩小、平移、根据ID查找GraphicsLayer、FeatureLayer等
    /// </summary>
    public static class MapMethods
    {
        private static int currentLevel = 9;
        private static double nowX;
        private static double nowY;
        private static int nowLevel;
        /// <summary>
        /// LPY 2015-9-7 添加 地图平移缩放
        /// 
        /// </summary>
        /// <param name="json"></param>
        public static void MoveAndZoomMapByJson(JObject json)
        {
            MoveAndZoomMapByXYL(Convert.ToDouble(json["CENTX"].ToString()), Convert.ToDouble(json["CENTY"].ToString()), Convert.ToInt32(json["LEVEL"].ToString()));
        }

        /// <summary>
        /// LPY 2015-9-7 添加
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="level"></param>
        public static void MoveAndZoomMapByXYL(double x, double y, int level)
        {
            try
            {
                double ratio = 1.0;
                double resolution;
                level += PublicParams.Delta; 
                if (level > 20)
                    level = 20;

                MapPoint pointCenter = new MapPoint(x, y);
                resolution = GetResoulution(level);

                if (PublicParams.pubMainMap.Resolution != 0.0)
                    ratio = resolution / PublicParams.pubMainMap.Resolution;
                if (Math.Abs(1.0 - ratio) < 0.0001)
                {
                    PublicParams.pubMainMap.PanTo(pointCenter);
                }
                else
                {
                    MapPoint pointMapCenter = PublicParams.pubMainMap.Extent.GetCenter();
                    double X = (pointCenter.X - ratio * pointMapCenter.X) / (1 - ratio);
                    double Y = (pointCenter.Y - ratio * pointMapCenter.Y) / (1 - ratio);

                    bool flagSL = PublicParams.pubMainMap.Layers["SL"].Visible;
                    bool flagYX = PublicParams.pubMainMap.Layers["YX"].Visible;

                    if (flagSL)
                    {
                        PublicParams.pubMainMap.Layers["SL"].Visible = false;
                        PublicParams.pubMainMap.ZoomToResolution(resolution, new MapPoint(X, Y));
                        PublicParams.pubMainMap.Layers["SL"].Visible = true;
                    }
                    if (flagYX)
                    {
                        PublicParams.pubMainMap.Layers["YX"].Visible = false;
                        PublicParams.pubMainMap.ZoomToResolution(resolution, new MapPoint(X, Y));
                        PublicParams.pubMainMap.Layers["YX"].Visible = true;
                    }
                }

                currentLevel = level;

            }
            catch (Exception)
            {
                LogHelper.WriteLog("MapMethods.cs-MoveAndZoomMapByJson-地图缩放平移操作出错！");
            }
        }

        public static void RefreshMainMapThread()
        {
            while (true)
            {
                if (PublicParams.pubX!=nowX||PublicParams.pubY!=nowY||PublicParams.pubLevel!=nowLevel)
                {
                    nowX = PublicParams.pubX;
                    nowY = PublicParams.pubY;
                    nowLevel = PublicParams.pubLevel;
                    PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(delegate { 
                        MoveAndZoomMapByXYL(nowX, nowY, nowLevel);
                    }));                    
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static double GetResoulution(int level)
        {
            double ResolutionLV20 = 0.0000019073515436137569;
            return (ResolutionLV20 * Math.Pow(2, 20 - level));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static int GetLevel(double resolution)
        {
            double ResolutionLV20 = 0.0000019073515436137569;
            //return (ResolutionLV20 * Math.Pow(2, 20 - level));
            return Convert.ToInt32(20 - Math.Log((resolution / ResolutionLV20), 2));
        }

        /// <summary>
        /// LPY 2015-9-7 添加 地图切换底图，目前在矢量、影像之间切换
        /// </summary>
        /// <param name="json"></param>
        public static void ChangeMapByJson(JObject json)
        {
            try
            {
                string strMapName = json["WITCHMAP"].ToString();
                if (strMapName == "slmap")
                {
                    PublicParams.pubMainMap.Layers["SL"].Visible = true;
                    PublicParams.pubMainMap.Layers["YX"].Visible = false;
                }
                else
                {
                    PublicParams.pubMainMap.Layers["SL"].Visible = false;
                    PublicParams.pubMainMap.Layers["YX"].Visible = true;
                }
            }
            catch (Exception)
            {
                LogHelper.WriteLog("MapMethods.cs-ChangeMapByJson-地图底图切换操作出错！");
            }            
        }

        /// <summary>
        /// 根据客户端发来的Graphic，在地图上画出该Graphic
        /// Graphic以Json串形式发来
        /// </summary>
        /// <param name="json"></param>
        public static void DrawBufferByJSON(JObject json)
        {
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() => {
                try
                {

                    Geometry geometry = Geometry.FromJson(json["GRAPHIC"]["Geometry"].ToString());
                    Graphic graphic = new Graphic() { Geometry = geometry };
                    switch (json["TYPE"].ToString())
                    {
                        case "Polygon":
                            graphic.Symbol = PublicParams.fillSymbol;
                            
                            break;
                        case "Polyline":
                            graphic.Symbol = PublicParams.lineSymbol;
                            break;
                        default:
                            break;
                    }
                    MapLayers.AddGraphicToGLayerByLayerID(graphic, PublicParams.gLayerDrawing);
                }
                catch (Exception)
                {
                }
            }));
        }

        

        /// <summary>
        /// LPY 2015-9-18 添加
        /// 清空Buffer图层上的Graphics
        /// </summary>
        public static void ClearBufferLayer()
        {
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    MapLayers.ClearGLayerByID(PublicParams.gLayerDrawing);
                }
                catch (Exception)
                {
                }
            }));
        }

        /// <summary>
        /// LPY 2016-3-25 添加
        /// 根据JSON，在大屏上标出测量的距离或者面积结果
        /// </summary>
        /// <param name="json"></param>
        public static void ShowLengthOrAreaByJSON(JObject json)
        {
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    string result = json["RESULT"].ToString();
                    Geometry geometry = Geometry.FromJson(json["GRAPHIC"]["Geometry"].ToString());
                    Graphic graphic = new Graphic() { Geometry = geometry };
                    switch (json["TYPE"].ToString())
                    {
                        case "Polygon":
                            graphic.Symbol = PublicParams.areaSymbol;

                            break;
                        case "Polyline":
                            graphic.Symbol = PublicParams.lengthSymbol;
                            break;
                        default:
                            break;
                    }
                    MapLayers.AddGraphicToGLayerByLayerID(graphic, PublicParams.gLayerLengthOrArea);

                    double xCenter = (graphic.Geometry.Extent.XMax + graphic.Geometry.Extent.XMin) / 2;//查找中心点
                    double yCenter = (graphic.Geometry.Extent.YMax + graphic.Geometry.Extent.YMin) / 2;
                    MapPoint mpCenter = new MapPoint(xCenter, yCenter, graphic.Geometry.SpatialReference);
                    TextSymbol txtSymbol = PublicParams.textSymbol;

                    Graphic gResult = new Graphic() { Geometry = mpCenter, Symbol = txtSymbol };
                    gResult.Attributes["Result"] = result;
                    gResult.SetZIndex(2);
                    MapLayers.AddGraphicToGLayerByLayerID(gResult, PublicParams.gLayerLengthOrArea);

                }
                catch (Exception)
                {
                }
            }));
        }

        public static void ShowSearchCamerasByJson(JObject json)
        {
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() => {
                try
                {
                    Geometry geometry = Geometry.FromJson(json["GRAPHIC"]["Geometry"].ToString());
                    Graphic graphic = new Graphic() { Geometry = geometry, Symbol = PublicParams.symbolSearchCameras };
                    MapLayers.AddGraphicToGLayerByLayerID(graphic, PublicParams.gLayerSearchCamerasNearCrime);
                }
                catch (Exception)
                {
                }
            }));
        }

        /// <summary>
        /// LPY 2016-3-24 添加
        /// 清空测距离和面积的图层
        /// </summary>
        public static void ClearLengthOrAreaLayer()
        {
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    MapLayers.ClearGLayerByID(PublicParams.gLayerLengthOrArea);
                }
                catch (Exception)
                {
                }
            }));
        }

        public static void ShowInfoWindowByJSON(JObject json)
        {
            try
            {
                JObject jsonAttr = json["GRAPHIC"]["Attributes"] as JObject;
                if (jsonAttr!=null)
                {
                    PublicParams.pubInfoWin.IsOpen = false ;
                    PublicParams.pubInfoWin.Anchor = new MapPoint(Convert.ToDouble(jsonAttr["X"].ToString()), Convert.ToDouble(jsonAttr["Y"].ToString()));
                    PublicParams.pubInfoWin.ContentTemplate = PublicParams.pubLayoutRoot.FindResource("DT"+jsonAttr["Class"].ToString()) as DataTemplate;
                    PublicParams.pubInfoWin.Content = jsonAttr;
                    PublicParams.pubInfoWin.IsOpen = true;
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SendPoliceCarPositon(Graphic graphic,int screenX,int screenY)
        {
            string strToSend = string.Format("{{CMD:'PoliceCarPositon',TITLE:'{0}',X:'{1}',Y:'{2}',GRAPHIC:{3}}}", graphic.Attributes["TITLE"].ToString(), screenX, screenY, JsonConvert.SerializeObject(graphic));
            TCPClients.SendCMD(strToSend);
        }


    }
}
