using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Tasks;
    using ESRI.ArcGIS.Client.Toolkit;
    using ESRI.ArcGIS.Client.Toolkit.DataSources;

    using System.Windows.Media;
    //using System.Drawing;
    public class HeatMap : HeatMapLayer
    {
        //定义过滤条件
        private string filter;
        public string setfilter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
            }
        }

        public string url;
        public string seturl
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
            }
        }

        //定义输出的字段
        private string[] field;
        public string[] setfield
        {
            get
            {
                return field;
            }
            set
            {
                field = value;
            }
        }

        public void setsource()
        {
            try
            {
                QueryTask queryTask = new QueryTask();
                queryTask.Url = url;
                queryTask.ExecuteCompleted += new EventHandler<QueryEventArgs>(queryTask_ExecuteCompleted);
                Query query = new ESRI.ArcGIS.Client.Tasks.Query();
                query.ReturnGeometry = true;
                query.Where = filter;
                //     query.OutFields.Add("OBJECTID");  SDE 发布的需要这条
                queryTask.ExecuteAsync(query);
            }
            catch
            {
                //throw;
            }
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet == null || featureSet.Features.Count < 1)
            {
                this.HeatMapPoints.Clear();
                return;
            }

            if (this.HeatMapPoints.Count > 0)
            {
                this.HeatMapPoints.Clear();
            }

            foreach (Graphic graphic in featureSet.Features)
            {
                this.HeatMapPoints.Add(graphic.Geometry as MapPoint);
            }
        }

        public void refreshnow()
        {
            try
            {
                QueryTask queryTask = new QueryTask();
                queryTask.Url = url;
                queryTask.ExecuteCompleted += new EventHandler<QueryEventArgs>(queryTask_ExecuteCompleted);
                Query query = new ESRI.ArcGIS.Client.Tasks.Query();
                query.ReturnGeometry = true;
                query.Where = filter;

                //   query.OutFields.Add("OBJECTID");
                queryTask.ExecuteAsync(query);
            }
            catch
            {
                //throw;//LPY 2015-8-11 11:09:45 注释掉
            }
        }

        public HeatMap()
        {
            GradientStopCollection gsc = new GradientStopCollection();
            //gsc.Add(new GradientStop(Colors.Transparent, 0));
            //gsc.Add(new GradientStop(Colors.LightBlue, 0.01));
            //gsc.Add(new GradientStop(Colors.Blue, 0.05));
            //gsc.Add(new GradientStop(Colors.Green, 0.1));
            //gsc.Add(new GradientStop(Colors.Yellow, 0.5));
            //gsc.Add(new GradientStop(Colors.White, 0.75));
            //gsc.Add(new GradientStop(Colors.Red, 1));



            gsc.Add(new GradientStop(Colors.Transparent, 0));
            gsc.Add(new GradientStop(Color.FromRgb(0x00, 0x00, 0xff), 0.01));
            gsc.Add(new GradientStop(Color.FromRgb(0x00, 0x75, 0xc5), 0.02));
            gsc.Add(new GradientStop(Color.FromRgb(0x00, 0xa8, 0x84), 0.03));
            gsc.Add(new GradientStop(Color.FromRgb(0x00, 0x90, 0x4a), 0.05));
            gsc.Add(new GradientStop(Color.FromRgb(0x7b, 0xc9, 0x38), 0.1));
            gsc.Add(new GradientStop(Color.FromRgb(0xfe, 0xf8, 0x02), 0.45));
            gsc.Add(new GradientStop(Color.FromRgb(0xeb, 0xb7, 0x01), 0.6));
            gsc.Add(new GradientStop(Color.FromRgb(0xf2, 0x96, 0x31), 0.7));
            gsc.Add(new GradientStop(Color.FromRgb(0xf3, 0x80, 0x01), 0.85));
            gsc.Add(new GradientStop(Color.FromRgb(0xeb, 0x3d, 0x00), 0.95));
            gsc.Add(new GradientStop(Colors.Red, 1));
            this.Gradient = gsc;
        }
    }
}
