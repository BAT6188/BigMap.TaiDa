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
    using System.Windows;
    /// <summary>
    public class PoliceCarGPS
    {
        public PoliceCarGPS()
        {
            Task initConsumerGPS = new Task(InitPoliceCarGPS);//防止MQ服务器不通的时候出现卡死
            initConsumerGPS.Start();
        }

        private void InitPoliceCarGPS()
        {
            try
            {
                IConnectionFactory factoryGPS=new ConnectionFactory(PublicParams.strMQUrl);
                IConnection connectionGPS = factoryGPS.CreateConnection();
                connectionGPS.Start();
                ISession sessionGPS = connectionGPS.CreateSession();
                IMessageConsumer consumerGPS = sessionGPS.CreateConsumer(new ActiveMQTopic(PublicParams.topicGPS));
                consumerGPS.Listener += consumerGPS_Listener;

            }
            catch (Exception)
            {
                
            }
        }

        void consumerGPS_Listener(IMessage message)
        {
            ITextMessage msg = (ITextMessage)message;
            PublicParams.pubMainMap.Dispatcher.Invoke(new Action(delegate
            {
                JObject json = JObject.Parse(msg.Text);
                DrawPoliceCarByJson(json);
            }));
            //throw new NotImplementedException();
        }

        private void DrawPoliceCarByJson(JObject json)
        {
            try
            {
                Graphic gPoliceCar = MapLayers.GetGraphicFromGLayerByID("TITLE", json["TITLE"].ToString(), PublicParams.gLayerPoliceCarGPS);
                if (gPoliceCar != null)
                {
                    gPoliceCar.Geometry = new MapPoint((double)json["X"], (double)json["Y"], new SpatialReference(4326));
                    gPoliceCar.Attributes["X"] = (double)json["X"];
                    gPoliceCar.Attributes["Y"] = (double)json["Y"];
                    MapLayers.RefreshGLayerByID(PublicParams.gLayerPoliceCarGPS);

                    if (PublicParams.pubInfoWin.IsOpen == true && (PublicParams.pubInfoWin.Content as JObject)["TITLE"].ToString()==gPoliceCar.Attributes["TITLE"].ToString())
                    { 
                        PublicParams.pubInfoWin.Anchor = gPoliceCar.Geometry as MapPoint;
                        Point screenPoint = PublicParams.pubMainMap.MapToScreen(gPoliceCar.Geometry as MapPoint);
                        MapMethods.SendPoliceCarPositon(gPoliceCar, Convert.ToInt32(screenPoint.X), Convert.ToInt32(screenPoint.Y));
                    }
                    
                }
                else//不存在该车辆，先添加该车辆到地图上
                {
                    MapPoint mpGPS = new MapPoint((double)json["X"], (double)json["Y"], new SpatialReference(4326));
                    Symbol sGPS = App.Current.Resources["PoliceCarOnline"] as Symbol;
                    Graphic gNewCar = new Graphic() { Symbol = sGPS, Geometry = mpGPS };
                    gNewCar.Attributes.Add("TITLE", json["TITLE"].ToString()); 
                    gNewCar.Attributes.Add("HH", json["HH"].ToString());//呼号
                    gNewCar.Attributes.Add("Class", "PoliceCar");
                    gNewCar.Attributes.Add("X", (double)json["X"]); gNewCar.Attributes.Add("Y", (double)json["Y"]);
                    MapLayers.AddGraphicToGLayerByLayerID(gNewCar, PublicParams.gLayerPoliceCarGPS);
                    MapLayers.RefreshGLayerByID(PublicParams.gLayerPoliceCarGPS);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
