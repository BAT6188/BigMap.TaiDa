using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ESRI.ArcGIS.Client;
    using System.Windows.Media.Animation;
    using Jovian.BigMap.parts;
    /// <summary>
    /// LPY 2015-9-6 14:27:06
    /// 处理客户端通过TCP与大屏的连接
    /// </summary>
    class TCPClients
    {
        private Socket socClient = null;
        private static Socket socClientStatic = null;

        public TCPClients(Socket socket)
        {
            socClient = socket;
            socClientStatic = socket;
        }

        /// <summary>
        /// LPY 2015-9-6 添加
        /// 循环接收客户端发送来的指令，转换成标准JSON字符串后，交由HandleCommands()处理
        /// </summary>
        public void ClientWork()
        {
            byte[] bytes = new byte[256 * 1024];
            string strCMD = string.Empty;
            int intReceivedBytesLength;
            string strReceived=string.Empty;

            while (true)
            {
                try
                {
                    intReceivedBytesLength = socClient.Receive(bytes, bytes.Length, 0);
                    if (intReceivedBytesLength == 0)
                        break;

                    strReceived += Encoding.UTF8.GetString(bytes, 0, intReceivedBytesLength);
                    //LogHelper.WriteLog(strReceived);
                    //LPY 2016-4-21 修改 增加一个分包符号，处理数据包粘包问题
                    while (strReceived.Contains(PublicParams.splitChar))
                    {
                        int splitCharIndex=strReceived.IndexOf(PublicParams.splitChar);
                        strCMD += strReceived.Substring(0, splitCharIndex);


                        JObject json = JObject.Parse(strCMD);
                        if (json["CMD"].ToString() == "SYNC")
                        {
                            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                string strMapPosition = string.Format("{{\"CMD\":\"SYNCR\",\"CENTX\":{0},\"CENTY\":{1},\"LEVEL\":{2}}}", PublicParams.pubMainMap.Extent.GetCenter().X, PublicParams.pubMainMap.Extent.GetCenter().Y, (MapMethods.GetLevel(PublicParams.pubMainMap.Resolution) - 4).ToString());
                                SendCommandsToClient(strMapPosition);
                                //LogHelper.WriteLog("发回客户端：" + strMapPosition);
                            }));
                        }
                        else
                        {
                            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                HandleCommands(json);
                            }));
                        }

                        strCMD = string.Empty;
                        strReceived = strReceived.Substring(splitCharIndex + 1, strReceived.Length - splitCharIndex - 1);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        

        /// <summary>
        /// LPY 2015-9-6 添加
        /// 处理指令
        /// </summary>
        /// <param name="rejson">待解析处理的JSON字符串</param>
        public void HandleCommands(JObject rejson)
        {
            switch (rejson["CMD"].ToString())
            {
                case "00000"://底图切换
                    MapMethods.ChangeMapByJson(rejson);                    
                    break;
                case "00001"://地图移动、缩放
                    //MapMethods.MoveAndZoomMapByJson(rejson);
                    PublicParams.pubX = Convert.ToDouble(rejson["CENTX"].ToString());
                    PublicParams.pubY = Convert.ToDouble(rejson["CENTY"].ToString());
                    PublicParams.pubLevel = Convert.ToInt32(rejson["LEVEL"].ToString());
                    //LogHelper.WriteLog(PublicParams.type,"从客户端收到："+ rejson.ToString());
                    break;
                case "00105"://接收到客户端发来的Graphic，显示在大屏上
                    MapMethods.DrawBufferByJSON(rejson);
                    //MapLayers.AddGraphicToGLayerByLayerID(JsonHelper.FromJson<Graphic>(rejson["GRAPHIC"].ToString()), PublicParams.gLayerDrawing);
                    
                    break;
                case "00106"://清空Buffer图层上的Graphics
                    MapMethods.ClearBufferLayer();
                    break;
                case "10003"://根据客户端发来的Pad窗口名称和开关指令，执行大屏上的相应Pad显示或隐藏
                    PadHelper.SwitchPadByName(rejson["ITEM"].ToString(), rejson["SWITCH"].ToString());
                    break;
                case "00107"://打开InfoWindow
                    //PublicParams.pubInfoWin.IsOpen = true;
                    MapMethods.ShowInfoWindowByJSON(rejson);
                    break;
                case "00108":
                    PublicParams.pubInfoWin.IsOpen = false;
                    break;
                case "00109"://清空测距离和面积图层
                    MapMethods.ClearLengthOrAreaLayer();
                    break;
                case "00110"://在大屏上标出测量的距离或者面积结果
                    MapMethods.ShowLengthOrAreaByJSON(rejson);
                    break;
                case "00111"://在大屏上画出新案件点周边的视频点
                    MapMethods.ShowSearchCamerasByJson(rejson);
                    break;
                case "00037"://弹出视频背景板
                    PublicParams.padVideos.Height = PublicParams.videoPadHeight;
                    PublicParams.padVideos.Width = PublicParams.videoPadWidth;
                    //PublicParams.padVideos.BeginStoryboard(App.Current.FindResource("StoryboardForPadVideosOpen") as Storyboard);
                    break;
                case "00038"://收回视频背景板
                    PublicParams.padVideos.Height = 0;PublicParams.padVideos.Width = 0;//PadVideos.ClearPadTitles();
                    //PublicParams.padVideos.BeginStoryboard(App.Current.FindResource("StoryboardForPadVideosClose") as Storyboard);
                    break;
                case "00029"://聚合图
                    MapLayers.SwitchLayerByID(rejson["LAYERID"].ToString(), rejson["SWITCH"].ToString());
                    break;
                case "00030"://热力图显隐
                    MapLayers.SwitchLayerByID(rejson["LAYERID"].ToString(), rejson["SWITCH"].ToString());
                    break;
                case "00031"://显示隐藏相应图层-FeatureLayer
                    MapLayers.SwitchLayerByID(rejson["LAYERID"].ToString(), rejson["SWITCH"].ToString());
                    break;
                case "00032"://显示隐藏相应图层-GraphicsLayer
                    MapLayers.SwitchLayerByID(rejson["LAYERID"].ToString(), rejson["SWITCH"].ToString());
                    break;
                case "00033"://清空GraphicsLayer图层
                    MapLayers.ClearGLayerByID(rejson["LAYERID"].ToString());
                    PublicParams.listCase.Clear();
                    break;
                case "00039"://根据指令显示或清空视频背景板相应背景的文字
                    PadVideos.ShowHideTextByTitle(rejson["TITLE"].ToString(), Convert.ToInt32(rejson["ID"].ToString()), rejson["SWITCH"].ToString());
                    break;
                case "00065"://控制端点击菜单指令
                    //PublicParams.padMenu.BeginStoryboard(App.Current.FindResource("StoryboardForPadMenuShow") as Storyboard);
                    //PublicParams.padMenu.DrawMenu2ByJson(rejson);
                    break;
                case "00061"://隐藏菜单指令
                    //PublicParams.padMenu.DrawMenu2ByJson(rejson);
                    break;
                case "00041"://调整单个开窗位置
                    ParamsHelper.ExecParamsFromJson(rejson);
                    break;
                case "00042"://保存全部窗口位置
                    ParamsHelper.ExecSaveParamsFromJson(rejson);
                    break;
                case "00043"://调整单个开窗位置和窗体名称
                    //ParamsHelper.ExecParamsAndNameFromJson(rejson);
                    break;
                default:
                    break;
            }
            
        }

        /// <summary>
        /// 回发信息给客户端
        /// </summary>
        /// <param name="cmd">待发送的字符串</param>
        public void SendCommandsToClient(string cmd)
        {
            byte[] bytesToSend = Encoding.UTF8.GetBytes(cmd);
            socClient.Send(bytesToSend);
        }

        public static void SendCMD(string cmdStr)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(cmdStr);
            try
            {
                if (socClientStatic != null)
                    socClientStatic.Send(bytes);
            }
            catch (Exception)
            {
            }
        }
    }
}
