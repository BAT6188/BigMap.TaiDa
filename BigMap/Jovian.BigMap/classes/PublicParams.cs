using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using System.Configuration;
    using System.Windows.Controls;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Sockets;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Toolkit;
    using ESRI.ArcGIS.Client.Symbols;

    using Jovian.BigMap.parts;
    using System.Collections.ObjectModel;

    /// <summary>
    /// LPY 2015-9-6 添加
    /// 公共参数类
    /// </summary>
    public class PublicParams
    {
        public static string urlVectorMap = GetAppConfigValueByString("SL");//矢量地图URL
        public static string urlImageMap = GetAppConfigValueByString("YX");//影像地图URL

        public static Socket pubSocketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static List<Socket> pubSocketClients = new List<Socket>();

        public static string strMQUrl = GetAppConfigValueByString("MQ");
        public static string topicCase = GetAppConfigValueByString("TopicCase");                            //案件推送
        public static string topicGPS = GetAppConfigValueByString("TopicGPS");                              //警车、警员GSP信息推送
        public static string topicReservoir = GetAppConfigValueByString("TopicReservoir");                  //水库信息
        public static string topicPoliceCarPosition = GetAppConfigValueByString("TopicPoliceCarPosition");  //警车所在社区
        public static string topicTraffic = GetAppConfigValueByString("TopicTraffic");                      //交通
        public static string topicLight = GetAppConfigValueByString("TopicLight");

        public static string strDBPath = GetAppConfigValueByString("DBPath");

        public static int LocalPort = Convert.ToInt32(GetAppConfigValueByString("LocalPort"));//本地等待客户端接入的监听接口
        public static string strDVCSIP = GetAppConfigValueByString("DVCSIP");                 //DVCS服务器IP
        public static int strDVCSPort = Convert.ToInt32(GetAppConfigValueByString("DVCSPort"));
        public static Type type = typeof(MainWindow);//

        public static Map pubMainMap = null;
        public static Grid pubLayoutRoot;
        public static InfoWindow pubInfoWin;
        public static Canvas pubCanvasChild1;        

        public static int Delta = Convert.ToInt32(GetAppConfigValueByString("Delta"));//大屏地图与客户端地图显示层级的差
        public static string splitChar = GetAppConfigValueByString("SplitChar");//分隔符

        //下面这三个参数用来存储当前地图中心坐标点X、Y、Level
        public static double pubX;
        public static double pubY;
        public static int pubLevel;

        public static Thread threadForMQ = null;//监听MQ线程，程序关闭时，要判断该线程是否为null，不为null时要释放掉

        public static PadCaseInfo padCaseInfo;
        //public static PadErrorsInfo padErrorsInfo;
        public static PadVideos padVideos;
        
        //系统内建GraphicsLayer ID
        public static string gLayerDrawing = "gLayerDrawing";//地图圈选功能
        public static string gLayerPoliceCarGPS = "gLayerPoliceCarGPS";//警车图层
        public static string gLayerCase = "gLayerCase";//案件图层
        public static string gLayerCrimePoint = "gLayerCrimePoint";//存放大的案件点
        public static string gLayerLengthOrArea = "gLayerLengthOrArea";//测量距离和面积图层
        public static string gLayerSearchCamerasNearCrime = "gLayerSearchCamerasNearCrime";//案件周边查询监控点范围 的图层

        public static string urlCamerasLayer = GetAppConfigValueByString("CamerasLayer");//
        public static string urlNetBarLayer = GetAppConfigValueByString("NetBarLayer");//
        public static string urlCompanyLayer = GetAppConfigValueByString("CompanyLayer");//
        public static string urlBankLayer = GetAppConfigValueByString("BankLayer");//
        public static string urlGasolineLayer = GetAppConfigValueByString("GasolineLayer");//
        public static string urlCaseLayer = GetAppConfigValueByString("CaseLayer");
        public static string urlHospitalLayer = GetAppConfigValueByString("HospitalLayer");//医院

        public static string urlTrafficLightLayer = GetAppConfigValueByString("TrafficLightLayer");
        public static string urlRoadLayer = GetAppConfigValueByString("RoadLayer");

        public static string fLayerCameras = "fLayerCameras";//摄像头图层名称
        public static string fLayerBank = "fLayerBank";//银行图层名称
        public static string fLayerCompany = "fLayerCompany";//
        public static string fLayerGasoline = "fLayerGasoline";
        public static string fLayerNetBar = "fLayerNetBar";//网吧
        public static string fLayerCase = "fLayerCase";
        public static string fLayerHospital = "fLayerHospital";

        public static string fLayerTrafficLight = "fLayerTrafficLight";
        public static string fLayerRoad = "fLayerRoad";

        public static string hLayerCase = "hLayerCase";//热力图-案件
        public static string cLayerCase = "cLayerCase";//聚合图

        public static string xmlFilePath = "params.xml";//默认xml配置文件名
        public static string IsInitLayers = GetAppConfigValueByString("InitLayers");//标记是否加载图层


        public static SimpleRenderer rendererCamera = new SimpleRenderer() { Symbol = (Symbol)App.Current.Resources["pmsForCamerasLayer"] };//
        public static SimpleRenderer rendererNetBar = new SimpleRenderer() { Symbol = (Symbol)App.Current.Resources["pmsForNetBarLayer"] };//
        public static SimpleRenderer rendererCompany = new SimpleRenderer() { Symbol = (Symbol)App.Current.Resources["pmsForCompanyLayer"] };//
        public static SimpleRenderer rendererBank = new SimpleRenderer() { Symbol = (Symbol)App.Current.Resources["pmsForBankLayer"] };//
        public static SimpleRenderer rendererGasoline = new SimpleRenderer() { Symbol = (Symbol)App.Current.Resources["pmsForGasolineLayer"] };//
        public static SimpleRenderer rendererHospital = new SimpleRenderer() { Symbol = (Symbol)App.Current.Resources["pmsForHospitalLayer"] };//医院

        public static Symbol symbolSearchCameras = App.Current.Resources["SearchCameras"] as Symbol;

        public static SimpleRenderer rendererCluster = new SimpleRenderer() { Symbol = (Symbol)App.Current.Resources["smsCluster"] };
        public static System.Windows.Media.LinearGradientBrush lgbCluster = App.Current.Resources["lgbCluster"] as System.Windows.Media.LinearGradientBrush;

        public static Symbol symbolGreenLight = App.Current.Resources["pmsGreenLight"] as Symbol;//绿灯
        public static Symbol symbolYellowLight = App.Current.Resources["pmsYellowLight"] as Symbol;//黄灯
        public static Symbol symbolRedLight = App.Current.Resources["pmsRedLight"] as Symbol;//红灯

        public static LineSymbol roadFreeSymbol = App.Current.Resources["RoadFreeSymbol"] as LineSymbol;//空闲
        public static LineSymbol roadNormalSymbol = App.Current.Resources["RoadNormalSymbol"] as LineSymbol;//正常
        public static LineSymbol roadBusySymbol = App.Current.Resources["RoadBusySymbol"] as LineSymbol;//忙碌

        public static List<FeatureLayer> listFLayer = new List<FeatureLayer>();//保存当前地图中所有可交互的FeatureLayer图层

        //两个Symbol模板
        public static FillSymbol fillSymbol = App.Current.Resources["DefaultFillSymbol"] as FillSymbol;
        public static LineSymbol lineSymbol = App.Current.Resources["DefaultLineSymbol"] as LineSymbol;
        //测长度、面积相关符号
        public static FillSymbol areaSymbol = App.Current.Resources["AreaSymbol"] as FillSymbol;
        public static LineSymbol lengthSymbol = App.Current.Resources["LengthSymbol"] as LineSymbol;
        public static TextSymbol textSymbol = App.Current.Resources["TxtSymbol"] as TextSymbol;
        //警情数据
        public static ObservableCollection<Case> listCase = new ObservableCollection<Case>();
        public const int MAXCASENUM = 5;

        public static string isLogoShow = GetAppConfigValueByString("IsLogoShow");

        public static double videoPadHeight = Convert.ToDouble(GetAppConfigValueByString("VideoPadHeight"));
        public static double videoPadWidth = Convert.ToDouble(GetAppConfigValueByString("VideoPadWidth"));

        public static bool IsLogWrite = GetAppConfigValueByString("IsLogWrite") == "1" ? true : false;

        /// <summary>
        /// LPY 2015-9-9 添加
        /// 根据Key值，从App.config文件中获取配置项的value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppConfigValueByString(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
