using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using Apache.NMS;
    using Apache.NMS.ActiveMQ;
    using Apache.NMS.ActiveMQ.Commands;
    using System.Threading.Tasks;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Symbols;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Tasks;
    /// <summary>
    /// LPY 2016-4-1 添加
    /// 交通灯、道路
    /// 数据为模拟数据
    /// </summary>
    public class Traffic
    {
        private FeatureLayer flTrafficLight;//红绿灯
        private FeatureLayer flRoad;//道路
        public Traffic()
        {
            try
            {
                flTrafficLight = MapLayers.GetFeatureLayerByID(PublicParams.fLayerTrafficLight);
                flRoad = MapLayers.GetFeatureLayerByID(PublicParams.fLayerRoad);
                if (flTrafficLight == null || flRoad == null)
                    return;
            }
            catch (Exception)
            {
                LogHelper.WriteLog("Traffic初始化错误！");
                return;
            }

            flTrafficLight.OutFields = new OutFields { "*" };
            flRoad.OutFields = new OutFields { "*" };

            ClassBreaksRenderer cbrTrafficLight = new ClassBreaksRenderer();//红绿灯
            cbrTrafficLight.Field = "DLD_STATUS";
            ClassBreakInfo cbiRed = new ClassBreakInfo() { MinimumValue = 0, MaximumValue = 0, Symbol = PublicParams.symbolRedLight };
            ClassBreakInfo cbiYellow = new ClassBreakInfo() { MinimumValue = 1, MaximumValue = 1, Symbol = PublicParams.symbolYellowLight };
            ClassBreakInfo cbiGreen = new ClassBreakInfo() { MinimumValue = 2, MaximumValue = 2, Symbol = PublicParams.symbolGreenLight };
            cbrTrafficLight.Classes.Add(cbiRed); cbrTrafficLight.Classes.Add(cbiYellow); cbrTrafficLight.Classes.Add(cbiGreen);
            flTrafficLight.Renderer = cbrTrafficLight;

            ClassBreaksRenderer cbrRoad = new ClassBreaksRenderer();//道路
            cbrRoad.Field = "DL_COUNT";
            ClassBreakInfo cbiFree = new ClassBreakInfo() { MinimumValue = 0, MaximumValue = 100, Symbol = PublicParams.roadFreeSymbol };
            ClassBreakInfo cbiNormal = new ClassBreakInfo() { MinimumValue = 101, MaximumValue = 200, Symbol = PublicParams.roadNormalSymbol };
            ClassBreakInfo cbiBusy = new ClassBreakInfo() { MinimumValue = 201, MaximumValue = 1000, Symbol = PublicParams.roadBusySymbol };
            cbrRoad.Classes.Add(cbiFree); cbrRoad.Classes.Add(cbiNormal); cbrRoad.Classes.Add(cbiBusy);
            flRoad.Renderer = cbrRoad;






            Task taskInitTrafficLight = new Task(InitTrafficLight);
            taskInitTrafficLight.Start();

            Task taskInitRoad = new Task(InitRoad);
            taskInitRoad.Start();
        }

        private void InitTrafficLight()
        {
            try
            {
                IConnectionFactory factoryTraffic = new ConnectionFactory(PublicParams.strMQUrl);
                IConnection connTraffic = factoryTraffic.CreateConnection();
                connTraffic.Start();
                ISession sessionTraffic = connTraffic.CreateSession();
                IMessageConsumer consumerTraffic = sessionTraffic.CreateConsumer(new ActiveMQTopic(PublicParams.topicLight));
                consumerTraffic.Listener += consumerTraffic_Listener;
            }
            catch (Exception)
            {
            }
        }

        void consumerTraffic_Listener(IMessage message)
        {
            //throw new NotImplementedException();
            ITextMessage msg = (ITextMessage)message;
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() =>
            {
                JObject json = JObject.Parse(msg.Text);

                string lightID = json["ID"].ToString();
                int lightStatus = Convert.ToInt32(json["STATUS"].ToString());
                foreach (Graphic g in flTrafficLight)
                {
                    if (g.Attributes["DLD_ID"].ToString() == lightID)
                    {
                        g.Attributes["DLD_STATUS"] = lightStatus;
                    }
                }
                flTrafficLight.Refresh();

            }));
        }

        private void InitRoad()
        {
            try
            {
                IConnectionFactory factoryRoad = new ConnectionFactory(PublicParams.strMQUrl);
                IConnection connRoad = factoryRoad.CreateConnection();
                connRoad.Start();
                ISession sessionRoad = connRoad.CreateSession();
                IMessageConsumer consumerRoad = sessionRoad.CreateConsumer(new ActiveMQTopic(PublicParams.topicTraffic));
                consumerRoad.Listener += consumerRoad_Listener;
            }
            catch (Exception)
            {
            }
        }

        void consumerRoad_Listener(IMessage message)
        {
            ITextMessage msg = (ITextMessage)message;
            PublicParams.pubMainMap.Dispatcher.BeginInvoke(new Action(() =>
            {
                JObject json = JObject.Parse(msg.Text);

                string roadID = json["ID"].ToString();
                int roadStatus = Convert.ToInt32(json["VALUE"].ToString());
                foreach (Graphic g in flRoad)
                {
                    if (g.Attributes["DL_ID"].ToString() == roadID)
                    {
                        g.Attributes["DL_COUNT"] = roadStatus;
                    }
                }
                flRoad.Refresh();

            }));
        }
    }
}