using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Jovian.BigMap.classes;
using Jovian.BigMap.parts;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Local;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Jovian.BigMap
{
    /// <summary>
    /// LPY 2015-9-1 14:52:56 添加 主窗口
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket soc = null;
        //private Task threadListenToServerByTCP = null;
        public MainWindow()
        {
            InitializeComponent();
            //SetFullScreen();//设置本程序全屏显示
            LoadVectorMap(); //加载矢量地图
            LoadImageMap();//加载影像地图-------考虑异步加载，节省程序加载时间
            PublicParams.pubMainMap = mainMap;
            PublicParams.pubLayoutRoot = LayoutRoot;
            PublicParams.pubInfoWin = mainInfoWindow;
            //PublicParams.pubCanvasChild1 = canvasChild1;

            Task listenToClient = new Task(ListenToClientByTCP, TaskCreationOptions.LongRunning);//长时间监听
            listenToClient.Start();//建立监听，等待客户端连接大屏

            Thread threadRefreshMainMap = new Thread(MapMethods.RefreshMainMapThread) { IsBackground = true };
            threadRefreshMainMap.Start();

            if (PublicParams.IsInitLayers != "0")//暂时没有图层服务器，不显示图层
            {
                MapLayers.InitMapLayers();//初始化地图所需的图层，静态方法
            }
            //ShowOneCrimePoint();//添加一个案件点

            PadHelper.InitPads();
            PoliceCarGPS gps = new PoliceCarGPS();//MQ相关，接收警车GPS信号
            Traffic traffic = new Traffic();//红绿灯和道路

            ParamsHelper.ReloadModularsFromXML();
        }

        private void ShowOneCrimePoint()
        {
            GraphicsLayer CrimePointLayer = new GraphicsLayer()
            {
                ID = "FocusLayer",
                DisplayName = "Focus"
            };
            mainMap.Layers.Add(CrimePointLayer);
            Graphic CrimeSymbol = new Graphic();
            CrimeSymbol.Geometry = new MapPoint(121.2481, 28.5545, new SpatialReference(4326));
            CrimeSymbol.Symbol = App.Current.Resources["CrimePointSymbol"] as Symbol;
            CrimeSymbol.SetZIndex(100);
            CrimePointLayer.Graphics.Add(CrimeSymbol);

            //PublicParams.pubMainMap.Dispatcher.Invoke(new Action(() => { 
            mainInfoWindow.IsOpen = false;
            mainInfoWindow.Anchor = new MapPoint(121.2481, 28.5545);
            mainInfoWindow.ContentTemplate = this.Resources["DTBank"] as DataTemplate;
            mainInfoWindow.Content = JObject.Parse("{\"MC\":\"InfoWindow\",\"Age\":\"12\"}");
            mainInfoWindow.IsOpen = true;
            //}));
        }        

        private void CreatePadReserviorInfo()//PadReserviorInfo
        {
            PadReserviorInfo padReserviorInfo = new PadReserviorInfo() { 
                Width=1,Height=1,VerticalAlignment=VerticalAlignment.Top,HorizontalAlignment=HorizontalAlignment.Left,
                Margin=new Thickness(0),Name="padReserviorInfo"
            };
            LayoutRoot.Children.Add(padReserviorInfo);
            padReserviorInfo.BeginStoryboard(App.Current.FindResource("StoryForCasePad") as System.Windows.Media.Animation.Storyboard);
            
        }

        /// <summary>
        /// 通过监听客户端的连接请求，建立与客户端的TCP连接
        /// </summary>
        private void ListenToClientByTCP()
        {
            //TcpClient tc = new TcpClient();
            //tc.GetStream();
            IPAddress localIP = IPAddress.Any; 
            IPEndPoint iep = new IPEndPoint(localIP, PublicParams.LocalPort);

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iep);
            server.Listen(10);
            //PublicParams.pubSocketServer = server;
            while (true)
            {
                try
                {
                    Socket client = server.Accept();//同步方式等待客户端接入
                    TCPClients newClient = new TCPClients(client);
                    PublicParams.pubSocketClients.Add(client);//记录连接的客户端的个数
                    Task newTask = new Task(newClient.ClientWork);
                    newTask.Start();
                }
                catch (Exception)
                {
                    LogHelper.WriteLog("客户端连接不成功");
                }
            }
        }

        /// <summary>
        /// LPY 2015-9-6 添加
        /// 加载矢量地图作为底图
        /// </summary>
        private void LoadVectorMap()
        {
            TiledMapHelper vectorMap = new TiledMapHelper()
            {
                Url = PublicParams.urlVectorMap,
                ID = "SL",
                Visible = true,
                EnableOffline = true,
                SaveOfflineTiles = true,
                LoadOfflineTileFirst = true
            };
            mainMap.Layers.Add(vectorMap);
            mainMap.Extent = new ESRI.ArcGIS.Client.Geometry.Envelope(121.1, 28.5, 121.38, 28.65);
        }

        /// <summary>
        /// LPY  2015-9-6 添加
        /// 加载影像地图
        /// </summary>
        private void LoadImageMap()
        {
            TiledMapHelper imageMap = new TiledMapHelper()
            {
                Url = PublicParams.urlImageMap,
                ID = "YX",
                Visible = false,
                EnableOffline = true,
                SaveOfflineTiles = true,
                LoadOfflineTileFirst = true
            };
            mainMap.Layers.Add(imageMap);
            mainMap.Extent = new ESRI.ArcGIS.Client.Geometry.Envelope(121.1, 28.5, 121.38, 28.65);
        }


        /// <summary>
        /// LPY 2015-9-17 添加
        /// 连接到大屏
        /// </summary>
        /// <returns></returns>
        public bool ConnectToServer()
        {
            try
            {
                if (soc == null)
                {
                    IPAddress ip = IPAddress.Parse(PublicParams.strDVCSIP);
                    int port = PublicParams.strDVCSPort;
                    IPEndPoint ipEP = new IPEndPoint(ip, port);
                    soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    soc.Connect(ipEP);

                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                soc = null;
                return false;
            }
        }

        /// <summary>
        /// 从大屏断开连接
        /// </summary>
        /// <returns></returns>
        public bool DisconnectToServer()
        {
            try
            {
                if (soc != null)
                {
                    soc.Close();
                    soc.Dispose();
                    soc = null;
                    return true;
                }
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// LPY 2015-9-6 15:11:59 添加
        /// 设置程序全屏显示
        /// </summary>
        private void SetFullScreen()
        {
            #region 设置程序全屏代码
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            #endregion
        }

    }
}
