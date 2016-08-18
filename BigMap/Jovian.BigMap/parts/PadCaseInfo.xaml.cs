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
    using Apache.NMS;
    using Apache.NMS.ActiveMQ;
    using Apache.NMS.ActiveMQ.Commands;

    using Jovian.BigMap.classes;

    using System.Threading.Tasks;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Symbols;
    using ESRI.ArcGIS.Client.Geometry;
    /// <summary>
    /// LPY 2015-9-9 添加 显示最新5条案件信息，信息由MQ服务器推送
    /// CaseInfoPad.xaml 的交互逻辑
    /// </summary>
    public partial class PadCaseInfo : UserControl
    {
        public PadCaseInfo()
        {
            InitializeComponent();
            //this.Style = App.Current.Resources["BaseUserControlStyle2"] as Style;//通过该方式可更换主题样式

            Task initConsumer = new Task(InitConsumerPadCaseInfo);
            initConsumer.Start();

            lvCase.ItemsSource = PublicParams.listCase; 

            //PublicParams.listCase.Add(new Case("2015012100000030471", "治安案 事件", "李某某", "演示内容", "演示地址", DateTime.Now.ToString("yyyy-MM-dd hh:mm")));
            //PublicParams.listCase.Add(new Case("2015012100000030472", "群众求助", "222", "嘻嘻嘻", "嘎嘎嘎", DateTime.Now.ToString("yyyy-MM-dd hh:mm")));
            //PublicParams.listCase.Add(new Case("2015012100000030471", "治安案 事件", "李某某", "演示内容", "演示地址", DateTime.Now.ToString("yyyy-MM-dd hh:mm")));
            //PublicParams.listCase.Add(new Case("2015012100000030472", "群众求助", "222", "嘻嘻嘻", "嘎嘎嘎", DateTime.Now.ToString("yyyy-MM-dd hh:mm")));
            //PublicParams.listCase.Add(new Case("2015012100000030471", "治安案 事件", "李某某", "演示内容", "演示地址", DateTime.Now.ToString("yyyy-MM-dd hh:mm")));

        }

        public void InitConsumerPadCaseInfo()
        {
            try
            {
                IConnectionFactory factoryCase = new ConnectionFactory(PublicParams.strMQUrl);
                IConnection connectionCase = factoryCase.CreateConnection();
                connectionCase.Start();
                ISession sessionCase = connectionCase.CreateSession();
                IMessageConsumer consumerCase = sessionCase.CreateConsumer(new ActiveMQTopic(PublicParams.topicCase));//, "connectionMQPadCaseInfo", null, false
                consumerCase.Listener += consumer_Listener;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
            
        }        

        void consumer_Listener(IMessage message)
        {
            ITextMessage msg = (ITextMessage)message;
            PublicParams.pubMainMap.Dispatcher.Invoke(new Action(delegate{
                //tbTest.Text = msg.Text;
                JObject json = JObject.Parse(msg.Text);
                if (json["JJDBH"].ToString() == "00000")//演示用的程序会推送一些编号为00000的数据
                {
                    MapLayers.ClearGLayerByID(PublicParams.gLayerCrimePoint);
                    return;
                }
                Case newcase = new Case(json);
                DrawCase(json);
                LoopQueue(newcase);
                UpdateNewest(newcase);
            }));
        }

        private void UpdateNewest(Case newcase)//更新最新警情数据（列表下面的几行）
        {
            try
            {
                tbJJDBH.Text ="接警编号："+ newcase.JJDBH;
                tbBJSJ.Text = "报警时间：" + newcase.BJSJ;
                tbBJLBMC.Text = "报警类别：" + newcase.BJLBMC;
                tbBJRXM.Text = "报警人：" + newcase.BJRXM;
            }
            catch (Exception)
            {
                LogHelper.WriteLog("UpdateNewest出错");
            }
            
        }

        public void LoopQueue(Case _case)
        {
            try
            {
                if (PublicParams.listCase.Count == PublicParams.MAXCASENUM)
                    PublicParams.listCase.RemoveAt(PublicParams.MAXCASENUM - 1);
                PublicParams.listCase.Insert(0, _case);
            }
            catch (Exception)
            {
                LogHelper.WriteLog("LoopQueue出错");
            }            
        }

        private void DrawCase(JObject json)
        {
            try
            {
                Symbol sCase = App.Current.Resources["pmsForCase"] as Symbol;
                Geometry geoCase = new MapPoint(Convert.ToDouble(json["ZDDWXZB"].ToString()), Convert.ToDouble(json["ZDDWYZB"].ToString()), new SpatialReference(4326));
                Graphic gCase = new Graphic() {Symbol=sCase,Geometry=geoCase };
                gCase.Attributes.Add("BJLBMC", json["BJLXMC"].ToString().Trim());
                gCase.Attributes.Add("BJSJ", Convert.ToDateTime( json["BJSJ"].ToString()).ToString("yyyy-MM-dd HH:mm"));
                MapLayers.AddGraphicToGLayerByLayerID(gCase, PublicParams.gLayerCase);

                Symbol sCrimePoint = App.Current.Resources["CrimePointSymbol"] as Symbol;
                Geometry geoCrimePoint = new MapPoint(Convert.ToDouble(json["ZDDWXZB"].ToString()), Convert.ToDouble(json["ZDDWYZB"].ToString()), new SpatialReference(4326));
                Graphic gCrimePoint = new Graphic() { Symbol = sCrimePoint, Geometry = geoCrimePoint };
                MapLayers.ClearGLayerByID(PublicParams.gLayerCrimePoint);
                MapLayers.AddGraphicToGLayerByLayerID(gCrimePoint, PublicParams.gLayerCrimePoint);
            }
            catch (Exception)
            {
            }
        }
    }

    public class Case:INotifyPropertyChanged
    {
        private string jjdbh;//接警单编号
        private string bjlbmc;//报警类别名称
        private string bjrxm;//报警人姓名
        private string bjnr;//报警内容
        private string sfdz;//地址
        private string bjsj;//报警时间

        public string JJDBH {
            get { return jjdbh ; }
            set {
                jjdbh = value;
                OnPropertyChanged(new PropertyChangedEventArgs("JJDBH"));
            }
        }

        public string BJLBMC {
            get { return bjlbmc; }
            set {
                bjlbmc = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BJLBMC"));
            }
        }
        public string BJRXM
        {
            get { return bjrxm; }
            set
            {
                bjrxm = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BJRXM"));
            }
        }

        public string BJNR
        {
            get { return bjnr; }
            set
            {
                bjnr = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BJNR"));
            }
        }
        public string SFDZ
        {
            get { return sfdz; }
            set
            {
                sfdz = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SFDZ"));
            }
        }

        public string BJSJ
        {
            get { return bjsj; }
            set
            {
                bjsj = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BJSJ"));
            }
        }

        public Case(string _jjdbh, string _bjlbmc, string _bjrxm, string _bjnr, string _sfdz, string _bjsj)
        {
            JJDBH = _jjdbh; BJLBMC = _bjlbmc; BJRXM = _bjrxm; BJNR = _bjnr; SFDZ = _sfdz; BJSJ = _bjsj;
        }

        public Case(JObject json)
        {
            try
            {
                JJDBH = json["JJDBH"].ToString();
                BJLBMC = json["BJLXMC"].ToString();
                BJRXM = json["BJRXM"].ToString();
                BJNR = json["BJNR"].ToString();
                SFDZ = json["SFDZ"].ToString();
                BJSJ = Convert.ToDateTime(json["BJSJ"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception)
            {
                
            }
            
        }

        //public People(string _name,string _age)
        //{
        //    Name = _name; Age = _age;
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
