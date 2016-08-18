using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Toolkit.DataSources;
    using System.Windows.Media;
    /// <summary>
    /// LPY 2015-9-11 添加
    /// 管理地图图层，GraphicsLayer、FetureLayer、图层刷新、添加删除等操作
    /// </summary>
    class MapLayers
    {
        public static string filter = " 1=1 ";
        public static void InitMapLayers()
        {
            AddGraphicsLayerByID(PublicParams.gLayerDrawing,true);
            AddGraphicsLayerByID(PublicParams.gLayerLengthOrArea, true);
            PublicParams.listFLayer.Add(AddFeatureLayer(PublicParams.fLayerCase, PublicParams.urlCaseLayer, null, filter, false));//案件图层

            PublicParams.listFLayer.Add(AddFeatureLayer(PublicParams.fLayerCameras, PublicParams.urlCamerasLayer, PublicParams.rendererCamera, filter, false));
            PublicParams.listFLayer.Add(AddFeatureLayer(PublicParams.fLayerNetBar, PublicParams.urlNetBarLayer, PublicParams.rendererNetBar, filter, false));
            PublicParams.listFLayer.Add(AddFeatureLayer(PublicParams.fLayerCompany, PublicParams.urlCompanyLayer, PublicParams.rendererCompany, filter, false));
            PublicParams.listFLayer.Add(AddFeatureLayer(PublicParams.fLayerBank, PublicParams.urlBankLayer, PublicParams.rendererBank, filter, false));
            PublicParams.listFLayer.Add(AddFeatureLayer(PublicParams.fLayerGasoline, PublicParams.urlGasolineLayer, PublicParams.rendererGasoline, filter, false));
            PublicParams.listFLayer.Add(AddFeatureLayer(PublicParams.fLayerHospital, PublicParams.urlHospitalLayer, PublicParams.rendererHospital, filter, false));
                       
            AddGraphicsLayerByID(PublicParams.gLayerCase,false);//案件图层
            AddGraphicsLayerByID(PublicParams.gLayerPoliceCarGPS,false);//警车图层
            AddGraphicsLayerByID(PublicParams.gLayerCrimePoint,false);//大点
            AddGraphicsLayerByID(PublicParams.gLayerSearchCamerasNearCrime, false);//搜寻监控点图层-突发案件周边

            AddHeatMapLayerByID(PublicParams.hLayerCase, PublicParams.urlCaseLayer, false);//热力图-案件

            AddFeatureLayer(PublicParams.fLayerRoad, PublicParams.urlRoadLayer, null, filter, false);//道路
            AddFeatureLayer(PublicParams.fLayerTrafficLight, PublicParams.urlTrafficLightLayer, null, filter, false);//红绿灯
            AddClusterLayerByID(PublicParams.cLayerCase, PublicParams.urlCaseLayer, PublicParams.rendererCluster, filter, false);//聚合图
        }

        /// <summary>
        /// LPY 2015-9-17 添加
        /// 根据layerID图层ID把图层添加到主地图上
        /// </summary>
        /// <param name="layerID"></param>
        public static void AddGraphicsLayerByID(string layerID,bool visiable)
        {
            try
            {
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                {
                    LogHelper.WriteLog( layerID + "图层已存在");
                    return;
                }
                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = layerID,
                    DisplayName = layerID,
                    Visible=visiable
                };
                PublicParams.pubMainMap.Layers.Add(graphicsLayer);
            }
            catch (Exception)
            {
                LogHelper.WriteLog( layerID + ":发生了一个错误，在AddGraphicsLayerByID");
            }
        }

        /// <summary>
        /// LPY 2015-9-17 添加
        /// 根据图层ID layerID把图层从主地图上删除
        /// </summary>
        /// <param name="layerID"></param>
        public static void DelGraphicsLayerByID(string layerID)
        {
            try
            {
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                    PublicParams.pubMainMap.Layers.Remove(PublicParams.pubMainMap.Layers[layerID]);
            }
            catch (Exception)
            {
                LogHelper.WriteLog( layerID + ":发生了一个错误，在DelGraphicsLayerByID");
            }
        }

        //向GraphicsLayer添加Graphic
        public static void AddGraphicToGLayerByLayerID(Graphic graphic, string layerID)
        {
            try
            {
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                    ((GraphicsLayer)PublicParams.pubMainMap.Layers[layerID]).Graphics.Add(graphic);
            }
            catch (Exception)
            {
                LogHelper.WriteLog( layerID + ":发生了一个错误，在AddGraphicToLayerByLayerID");
            }
        }

        //从GraphicsLayer中查找一个Graphic
        public static Graphic GetGraphicFromGLayerByID(string key,string value, string layerID)
        {
            try
            {
                Graphic result = null;
                if (PublicParams.pubMainMap.Layers[layerID]!=null)
                {
                    foreach (Graphic graphic in (GraphicsLayer)PublicParams.pubMainMap.Layers[layerID])
                    {
                        if (graphic.Attributes[key].ToString() == value)
                            result = graphic;
                    }
                }                
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static FeatureLayer GetFeatureLayerByID(string layerID)
        {
            try
            {
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                    return PublicParams.pubMainMap.Layers[layerID] as FeatureLayer;
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void ClearGLayerByID(string layerID)
        {
            try
            {
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                    ((GraphicsLayer)PublicParams.pubMainMap.Layers[layerID]).Graphics.Clear();
            }
            catch (Exception)
            {
                LogHelper.WriteLog( layerID + ":发生了一个错误，在ClearGLayerByID");
            }
        }

        public static void RefreshGLayerByID(string layerID)
        {
            try
            {
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                    ((GraphicsLayer)PublicParams.pubMainMap.Layers[layerID]).Refresh();
            }
            catch (Exception)
            {
                LogHelper.WriteLog( layerID + ":发生了一个错误，在RefreshGLayerByID");
            }
        }

        /// <summary>
        /// 根据url向地图添加一个FeatureLayer
        /// </summary>
        /// <param name="layerID"></param>
        /// <param name="url"></param>
        /// <param name="renderer"></param>
        /// <param name="filter"></param>
        /// <param name="visiable"></param>
        /// <returns></returns>
        public static FeatureLayer AddFeatureLayer(string layerID, string url, SimpleRenderer renderer, string filter, bool visiable)
        {
            try
            {
                if (!HttpHelper.CheckUrl(url))
                {
                    LogHelper.WriteLog("图层url：" + url + "-无法连接！");
                    return null;
                }
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                {
                    LogHelper.WriteLog( layerID + "图层已存在");
                    return null;
                }
                FeatureLayer featureLayer = new FeatureLayer() { ID = layerID, Url = url, Renderer = renderer, Where = filter, Visible = visiable };
                featureLayer.OutFields.Add("*");

                PublicParams.pubMainMap.Layers.Add(featureLayer);
                return featureLayer;
            }
            catch (Exception)
            {
                LogHelper.WriteLog( layerID + ":发生了一个错误，在AddFeatureLayer");
                return null;
            }
        }

        public static void ShowHideFeatureLayerByID(string layerID, bool visiable)
        {
            ShowHideLayerByID(layerID, visiable);
        }

        public static void ShowHideGraphicsLayerByID(string layerID, bool visiable)
        {
            ShowHideLayerByID(layerID, visiable);
        }

        public static void ShowHideHeatMapLayerByID(string layerID, bool visiable)
        {
            ShowHideLayerByID(layerID, visiable);
        }

        private static void ShowHideLayerByID(string layerID, bool visiable)
        {
            try
            {
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                    PublicParams.pubMainMap.Layers[layerID].Visible = visiable;
            }
            catch (Exception)
            {
            }
        }

        public static void SwitchLayerByID(string layerID, string _switch)
        {
            try
            {
                switch (_switch)
                {
                    case "0":
                        ShowHideFeatureLayerByID(layerID, false);
                        break;
                    case "1":
                        ShowHideFeatureLayerByID(layerID, true);
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
        /// 热力图
        /// </summary>
        /// <param name="layerID"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HeatMapLayer AddHeatMapLayerByID(string layerID, string url, bool visiable)
        {
            try
            {
                if (!HttpHelper.CheckUrl(url))
                {
                    LogHelper.WriteLog("图层url：" + url + "-无法连接！");
                    return null;
                }
                HeatMap heatMap = new HeatMap() { ID = layerID, setfilter = " 1=1 ", seturl = url, Opacity = 0.8, Visible = visiable };
                heatMap.setsource();
                PublicParams.pubMainMap.Layers.Add(heatMap);
                heatMap.refreshnow();
                return heatMap;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// LPY 2016-4-2 添加
        /// 聚合图层添加
        /// </summary>
        /// <param name="layerID"></param>
        /// <param name="url"></param>
        /// <param name="renderer"></param>
        /// <param name="filter"></param>
        /// <param name="visiable"></param>
        public static void AddClusterLayerByID(string layerID, string url, SimpleRenderer renderer, string filter, bool visiable)
        {
            try
            {
                if (!HttpHelper.CheckUrl(url))
                {
                    LogHelper.WriteLog("图层url：" + url + "-无法连接！");
                    return;
                }
                if (PublicParams.pubMainMap.Layers[layerID] != null)
                {
                    LogHelper.WriteLog( layerID + "图层已存在");
                    return;
                }
                FeatureLayer featureLayer = new FeatureLayer() { ID = layerID, Url = url, Renderer = renderer, Where = filter, Visible = visiable };

                //FlareClusterer fc = new FlareClusterer()//设置聚合条件
                //{
                //    FlareForeground = new SolidColorBrush(Colors.White),
                //    FlareBackground = new SolidColorBrush(Colors.Black),

                //    //MaximumFlareCount = 10,//都是默认值，不需要改
                //    //Radius = 20,
                //    Gradient = PublicParams.lgbCluster
                //};
                //featureLayer.Clusterer = fc;

                SumClusterer sumClusterer = new SumClusterer() { Radius = 80 };
                featureLayer.Clusterer = sumClusterer;

                PublicParams.pubMainMap.Layers.Add(featureLayer);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
