using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
namespace Jovian.BigMap.classes
{
    using Jovian.BigMap.parts;
    using System.Windows.Media.Animation;
    public class PadHelper
    {
        public PadHelper()
        { 

        }

        public static void InitPads()
        {
            CreatePadPoweredbyLogo();
            CreatePadVideos();
            CreatePadCaseInfo();
            //CreatePadReserviorInfo();
        }

        private static void CreatePadPoweredbyLogo()
        {
            if (PublicParams.isLogoShow == "0")//控制logo是否显示
                return;
            PadPoweredByLogo padPoweredByLogo = new PadPoweredByLogo() { Width = 1000, Height = 348, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 100, 200),Name="PadPoweredbyLogo" };
            PublicParams.pubLayoutRoot.Children.Add(padPoweredByLogo);
        }

        private static void CreatePadVideos()
        {
            PadVideos padVideos = new PadVideos() { Width=0,Height=0,VerticalAlignment=VerticalAlignment.Top,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,-10,0) ,Name="PadVideos"};
            PublicParams.padVideos = padVideos;
            PublicParams.pubLayoutRoot.Children.Add(padVideos);
        }

        private static void CreatePadCaseInfo()
        {
            PadCaseInfo padCaseInfo = new PadCaseInfo() { Width = 1, Height = 1, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0), Name = "padCaseInfo" };
            PublicParams.padCaseInfo = padCaseInfo;
            PublicParams.pubLayoutRoot.Children.Add(padCaseInfo);
        }

        private static void CreatePadReserviorInfo()
        {
            PadReserviorInfo padReserviorInfo = new PadReserviorInfo() { Width=2400,Height=1800,VerticalAlignment=VerticalAlignment.Top,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0),Name="xxx"};
            PublicParams.pubLayoutRoot.Children.Add(padReserviorInfo);
        }


        public static void SwitchPadByName(string padName, string value)
        {
            try
            {
                switch (padName)
                {
                    case "警情监控"://警情监控-案件点模拟
                        switch (value)
                        {
                            case "0":
                                PublicParams.padCaseInfo.BeginStoryboard(App.Current.FindResource("StoryboardForCloseCasePad") as Storyboard);
                                MapLayers.ShowHideGraphicsLayerByID(PublicParams.gLayerCrimePoint,false);
                                MapLayers.ShowHideGraphicsLayerByID(PublicParams.gLayerCase, false);
                                MapLayers.ShowHideGraphicsLayerByID(PublicParams.gLayerSearchCamerasNearCrime, false);
                                break;
                            case "1":
                                PublicParams.padCaseInfo.BeginStoryboard(App.Current.FindResource("StoryboardForOpenCasePad") as Storyboard);
                                MapLayers.ShowHideGraphicsLayerByID(PublicParams.gLayerCrimePoint,true);
                                MapLayers.ShowHideGraphicsLayerByID(PublicParams.gLayerCase, true);
                                MapLayers.ShowHideGraphicsLayerByID(PublicParams.gLayerSearchCamerasNearCrime, true);
                                break;
                        }
                        break;

                    case "当日案件"://当日案件
                        switch (value)
                        {
                            case "0":
                                //GlobalLayers.TodayAJ.Visible = false;
                                //GlobalLayers.ActiveLayer.Visible = false;
                                break;
                            case "1":
                                //GlobalLayers.TodayAJ.Visible = true;
                                //GlobalLayers.ActiveLayer.Visible = true;
                                break;
                        }
                        break;
                    case "警车":
                        switch (value)
                        {
                            case "0":
                                //GlobalLayers._policepointglr.Visible = false;
                                //GlobalLayers._offlineglr.Visible = false;
                                break;
                            case "1":
                                //GlobalLayers._policepointglr.Visible = true;
                                //GlobalLayers._offlineglr.Visible = true;
                                break;
                        }
                        break;
                    case "警车活动":
                        switch (value)
                        {
                            case "0":
                                //GlobalLayers._carinfowindow.Width = 1;
                                //GlobalLayers._carinfowindow.Height = 1;
                                break;

                            case "1":
                                //GlobalLayers._carinfowindow.Width = 1500;
                                //GlobalLayers._carinfowindow.Height = 1000;
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception)
            {

            }

        }
    }
}
