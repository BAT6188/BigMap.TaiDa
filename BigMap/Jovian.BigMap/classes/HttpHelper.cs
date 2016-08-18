using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;

namespace Jovian.BigMap.classes
{
    /// <summary>
    /// LPY 2016-4-11 添加
    /// 帮助类 Http
    /// </summary>
    public class HttpHelper
    {
        /// <summary>
        /// LPY 2016-4-12 添加
        /// 检查Url能否连接
        /// </summary>
        /// <param name="url">待检查的Url</param>
        /// <returns></returns>
        public static bool CheckUrl(string url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                return response.IsSuccessStatusCode;
            }
            catch (WebException we)
            {
                LogHelper.WriteLog(we.Message);
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
