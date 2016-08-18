using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using ESRI.ArcGIS.Client;
using System.Windows.Media;

namespace Jovian.BigMap
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// LPY 2015-9-1 创建 重新规范代码
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 强制软件加速
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            // Before initializing the ArcGIS Runtime first 
            // set the ArcGIS Runtime license by providing the license string 
            // obtained from the License Viewer tool.
            //ArcGISRuntime.SetLicense("Place the License String in here"); 设置 ArcGIS Runtime 许可
            //ArcGISRuntime.SetLicense("runtimeadvanced,101,ecp327916071,01-jan-2021,B5F4LNBLEFJ92MZAD027");
            ArcGISRuntime.SetLicense("runtimestandard,101,rud262318119,none,2KYRMD1AJ4EL6XCFK092");

            //  "runtimestandard,101,rus592188164,18-jan-2015,C6JCA0PZAY0DY0HEY100";
            //  "runtimeadvanced,101,ecp327916071,01-jan-2021,B5F4LNBLEFJ92MZAD027"
            // Initialize the ArcGIS Runtime before any components are created.
            try
            {
                ArcGISRuntime.Initialize();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString());
                // Exit application
                this.Shutdown();
            }
        }

        //设置初始屏幕

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    SplashScreen splashScreen = new SplashScreen("StartScreen.png");
        //    splashScreen.Show(false);
        //    splashScreen.Close(TimeSpan.FromSeconds(1));
        //    base.OnStartup(e);
        //}
    }
}
